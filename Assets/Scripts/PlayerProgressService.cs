using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PlayerProgressService : MonoBehaviour
{
    #region Fields

    private const int CurrentSaveVersion = 3;
    private const string SaveFileName = "player_progress.json";
    private const string SaveTemporarySuffix = ".tmp";
    private const string SaveBackupSuffix = ".backup.json";

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private RunRewardCalculator _rewardCalculator;

    [SerializeField]
    private EquipmentCatalog _equipmentCatalog;

    private ProgressData _data;
    private int _lastAppliedRunId;
    private bool _hasLoaded;
    private bool _hasLoggedRewardCalculatorError;
    private bool _hasLoggedCatalogError;
    private bool _purchaseInProgress;
    private bool _equipInProgress;
    private bool _upgradeInProgress;

    private IProgressDataOperations DataOperations => _data;

    #endregion

    #region Properties

    public bool IsLoaded => _hasLoaded;

    public long TotalMoney => _data == null ? 0 : DataOperations.TotalMoney;

    public float BestDepth => _data == null ? 0f : DataOperations.BestDepth;

    public long BestScore => _data == null ? 0 : DataOperations.BestScore;

    public int TotalRuns => _data == null ? 0 : DataOperations.TotalRuns;

    public long TotalDugBlocks => _data == null ? 0 : DataOperations.TotalDugBlocks;

    public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public string SaveBackupPath => SaveFilePath + SaveBackupSuffix;

    #endregion

    #region Events

    public event Action ProgressChanged;
    public event Action<ProgressUpdate> RunSettled;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Load();
    }

    private void OnEnable()
    {
        if (_runManager != null)
            _runManager.RunFinished += HandleRunFinished;
    }

    private void OnDisable()
    {
        if (_runManager != null)
            _runManager.RunFinished -= HandleRunFinished;
    }

    #endregion

    #region Public Methods

    public bool OwnsEquipment(string _itemId)
    {
        EnsureLoaded();
        return _data != null && DataOperations.OwnsEquipment(_itemId);
    }

    public int GetEquipmentLevel(string _equipmentId)
    {
        EnsureLoaded();
        return _data == null ? 0 : DataOperations.GetEquipmentLevel(_equipmentId);
    }

    public string GetEquippedEquipmentId(EquipmentType _type)
    {
        EnsureLoaded();
        return _data == null ? string.Empty : DataOperations.GetEquippedEquipmentId(_type);
    }

    public bool TryGetEquippedEquipment(
        EquipmentType _type,
        out EquipmentItemDefinition _item)
    {
        _item = null;

        if (!EnsureLoaded() || _equipmentCatalog == null || _data == null)
            return false;

        string equippedId = DataOperations.GetEquippedEquipmentId(_type);

        if (!_equipmentCatalog.TryGetById(equippedId, out EquipmentItemDefinition item) ||
            item.EquipmentType != _type ||
            !item.HasValidConfiguration() ||
            !DataOperations.OwnsEquipment(equippedId))
        {
            return false;
        }

        _item = item;
        return true;
    }

    public bool TryGetEquipmentUpgradeInfo(
        string _equipmentId,
        out EquipmentUpgradeInfo _info)
    {
        _info = default;

        if (!EnsureLoaded() || _data == null)
            return false;

        if (_equipmentCatalog == null ||
            !_equipmentCatalog.TryGetById(_equipmentId, out EquipmentItemDefinition item) ||
            !item.HasValidConfiguration())
        {
            return false;
        }

        bool isOwned = DataOperations.OwnsEquipment(item.Id);
        bool isEquipped = DataOperations.GetEquippedEquipmentId(item.EquipmentType) == item.Id;
        int maximumLevel = item.GetMaximumUpgradeLevel();
        int currentLevel = Mathf.Clamp(DataOperations.GetEquipmentLevel(item.Id), 0, maximumLevel);

        if (!TryGetCurrentAttributes(item, currentLevel, out DrillStats currentDrillStats, out float currentLauncherMultiplier))
            return false;

        bool hasNextLevel = false;
        bool canPurchaseUpgrade = false;
        long nextLevelPrice = 0;
        DrillStats nextDrillStats = default;
        float nextLauncherMultiplier = 0f;

        if (isOwned &&
            currentLevel < maximumLevel &&
            item.HasValidUpgradeConfiguration())
        {
            int nextLevel = currentLevel + 1;
            bool hasPrice = item.TryGetUpgradePrice(nextLevel, out nextLevelPrice);
            bool hasAttributes = TryGetAttributes(
                item,
                nextLevel,
                out nextDrillStats,
                out nextLauncherMultiplier
            );

            hasNextLevel = hasPrice && hasAttributes;

            if (hasNextLevel)
            {
                canPurchaseUpgrade = nextLevelPrice >= 0 && TotalMoney >= nextLevelPrice;
            }
        }

        _info = new EquipmentUpgradeInfo(
            item.Id,
            item.DisplayName,
            item.EquipmentType,
            isOwned,
            isEquipped,
            currentLevel,
            maximumLevel,
            hasNextLevel,
            canPurchaseUpgrade,
            nextLevelPrice,
            TotalMoney,
            currentDrillStats,
            nextDrillStats,
            currentLauncherMultiplier,
            nextLauncherMultiplier
        );
        return true;
    }

    public PurchaseResult TryPurchaseEquipment(string _itemId)
    {
        if (_purchaseInProgress)
        {
            return new PurchaseResult(
                false,
                PurchaseFailureReason.OperationInProgress,
                _itemId,
                0,
                TotalMoney,
                true
            );
        }

        _purchaseInProgress = true;

        try
        {
            if (!EnsureLoaded() || _data == null)
                return CreatePurchaseFailure(_itemId, PurchaseFailureReason.NotLoaded);

            if (_equipmentCatalog == null ||
                !_equipmentCatalog.TryGetById(_itemId, out EquipmentItemDefinition item))
            {
                return CreatePurchaseFailure(_itemId, PurchaseFailureReason.InvalidItem);
            }

            if (DataOperations.OwnsEquipment(item.Id))
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.AlreadyOwned);

            if (item.Price < 0)
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InvalidPrice);

            if (!item.HasValidConfiguration())
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InvalidItem);

            if (DataOperations.TotalMoney < item.Price)
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InsufficientMoney);

            if (!TrySpendMoney(item.Price))
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InsufficientMoney);

            DataOperations.AddOwnedEquipment(item.Id);
            bool persistenceSucceeded = Save();
            ProgressChanged?.Invoke();

            return new PurchaseResult(
                true,
                persistenceSucceeded ? PurchaseFailureReason.None : PurchaseFailureReason.SaveFailed,
                item.Id,
                item.Price,
                DataOperations.TotalMoney,
                persistenceSucceeded
            );
        }
        finally
        {
            _purchaseInProgress = false;
        }
    }

    public UpgradePurchaseResult TryPurchaseEquipmentUpgrade(string _equipmentId)
    {
        if (_upgradeInProgress)
        {
            return CreateUpgradeFailure(
                _equipmentId,
                UpgradePurchaseFailureReason.OperationInProgress,
                0,
                true
            );
        }

        _upgradeInProgress = true;

        try
        {
            if (!EnsureLoaded() || _data == null)
            {
                return CreateUpgradeFailure(
                    _equipmentId,
                    UpgradePurchaseFailureReason.ProgressNotLoaded,
                    0,
                    false
                );
            }

            if (_equipmentCatalog == null ||
                !_equipmentCatalog.TryGetById(_equipmentId, out EquipmentItemDefinition item))
            {
                return CreateUpgradeFailure(
                    _equipmentId,
                    UpgradePurchaseFailureReason.InvalidItem,
                    0,
                    true
                );
            }

            if (!item.HasValidConfiguration())
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidItem,
                    0,
                    true
                );
            }

            if (_runManager != null && (_runManager.CurrentState == RunState.Launching || _runManager.IsRunning))
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.OperationInProgress,
                    0,
                    true
                );
            }

            if (!DataOperations.OwnsEquipment(item.Id))
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.NotOwned,
                    0,
                    true
                );
            }

            int maximumLevel = item.GetMaximumUpgradeLevel();
            int previousLevel = Mathf.Clamp(DataOperations.GetEquipmentLevel(item.Id), 0, maximumLevel);

            if (previousLevel >= maximumLevel)
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.MaximumLevelReached,
                    previousLevel,
                    true
                );
            }

            int newLevel = previousLevel + 1;

            if (!item.TryGetUpgradePrice(newLevel, out long price))
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidUpgradeConfiguration,
                    previousLevel,
                    true
                );
            }

            if (price < 0)
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidPrice,
                    previousLevel,
                    true
                );
            }

            if (!item.HasValidUpgradeConfiguration())
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidUpgradeConfiguration,
                    previousLevel,
                    true
                );
            }

            if (!TryGetAttributes(
                    item,
                    newLevel,
                    out DrillStats ignoredDrillStats,
                    out float ignoredLauncherMultiplier))
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidUpgradeConfiguration,
                    previousLevel,
                    true
                );
            }

            if (DataOperations.TotalMoney < price)
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InsufficientMoney,
                    previousLevel,
                    true
                );
            }

            if (!DataOperations.TrySetEquipmentLevel(item.Id, newLevel))
            {
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InvalidUpgradeConfiguration,
                    previousLevel,
                    true
                );
            }

            if (!DataOperations.TrySpendMoney(price))
            {
                DataOperations.TrySetEquipmentLevel(item.Id, previousLevel);
                return CreateUpgradeFailure(
                    item.Id,
                    UpgradePurchaseFailureReason.InsufficientMoney,
                    previousLevel,
                    true
                );
            }

            bool persistenceSucceeded = Save();
            ProgressChanged?.Invoke();

            return new UpgradePurchaseResult(
                true,
                persistenceSucceeded
                    ? UpgradePurchaseFailureReason.None
                    : UpgradePurchaseFailureReason.SaveFailed,
                item.Id,
                previousLevel,
                newLevel,
                price,
                DataOperations.TotalMoney,
                persistenceSucceeded
            );
        }
        finally
        {
            _upgradeInProgress = false;
        }
    }

    public EquipResult TryEquipEquipment(string _itemId)
    {
        if (_equipInProgress)
        {
            return new EquipResult(
                false,
                EquipFailureReason.OperationInProgress,
                _itemId,
                EquipmentType.Drill,
                false,
                true
            );
        }

        _equipInProgress = true;

        try
        {
            if (!EnsureLoaded() || _data == null)
                return CreateEquipFailure(_itemId, EquipFailureReason.NotLoaded);

            if (_equipmentCatalog == null ||
                !_equipmentCatalog.TryGetById(_itemId, out EquipmentItemDefinition item))
            {
                return CreateEquipFailure(_itemId, EquipFailureReason.InvalidItem);
            }

            if (!item.HasValidConfiguration())
            {
                return CreateEquipFailure(item.Id, EquipFailureReason.InvalidItem);
            }

            if (!DataOperations.OwnsEquipment(item.Id))
            {
                return new EquipResult(
                    false,
                    EquipFailureReason.NotOwned,
                    item.Id,
                    item.EquipmentType,
                    false,
                    true
                );
            }

            string equippedId = DataOperations.GetEquippedEquipmentId(item.EquipmentType);

            if (equippedId == item.Id)
            {
                return new EquipResult(
                    true,
                    EquipFailureReason.None,
                    item.Id,
                    item.EquipmentType,
                    false,
                    true
                );
            }

            bool changed = DataOperations.SetEquippedEquipmentId(item.EquipmentType, item.Id);
            bool persistenceSucceeded = Save();
            ProgressChanged?.Invoke();

            return new EquipResult(
                changed,
                persistenceSucceeded ? EquipFailureReason.None : EquipFailureReason.SaveFailed,
                item.Id,
                item.EquipmentType,
                changed,
                persistenceSucceeded
            );
        }
        finally
        {
            _equipInProgress = false;
        }
    }

    public bool ApplyRunResult(RunResult _result)
    {
        if (!CanApplyRunResult(_result))
            return false;

        RunReward reward = _rewardCalculator.Calculate(_result);
        DataOperations.ApplyRunResult(
            _result.Depth,
            reward.Score,
            reward.EarnedMoney,
            _result.DugBlocks,
            out long previousMoney,
            out bool isNewBestDepth,
            out bool isNewBestScore
        );

        _lastAppliedRunId = _result.RunId;
        Save();
        ProgressChanged?.Invoke();

        ProgressUpdate update = new ProgressUpdate(
            _result,
            reward.Score,
            reward.EarnedMoney,
            previousMoney,
            TotalMoney,
            isNewBestDepth,
            isNewBestScore
        );
        RunSettled?.Invoke(update);
        return true;
    }

    public bool Save()
    {
        if (!_hasLoaded)
            Load();

        if (_data == null)
            return false;

        string temporaryPath = SaveFilePath + SaveTemporarySuffix;

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(temporaryPath, json);

            if (!File.Exists(temporaryPath))
                throw new IOException("The progress temporary file was not created.");

            if (File.Exists(SaveFilePath))
                File.Copy(SaveFilePath, SaveBackupPath, true);

            File.Copy(temporaryPath, SaveFilePath, true);
            File.Delete(temporaryPath);
            return true;
        }
        catch (IOException _exception)
        {
            LogSaveError(_exception);
            return false;
        }
        catch (UnauthorizedAccessException _exception)
        {
            LogSaveError(_exception);
            return false;
        }
        catch (Exception _exception)
        {
            LogSaveError(_exception);
            return false;
        }
    }

    public void DeleteProgress()
    {
        DeleteFileIfExists(SaveFilePath);
        DeleteFileIfExists(SaveFilePath + SaveTemporarySuffix);
        DeleteFileIfExists(SaveBackupPath);
        DeleteInvalidSaveFiles();

        _data = CreateDefaultData();
        EnsureEquipmentData();
        NormalizeUpgradeData();
        _lastAppliedRunId = 0;
        _hasLoaded = true;
        Save();
        ProgressChanged?.Invoke();
    }

    #endregion

    #region Private Types

    private interface IProgressDataOperations
    {
        int SaveVersion { get; }
        long TotalMoney { get; }
        float BestDepth { get; }
        long BestScore { get; }
        int TotalRuns { get; }
        long TotalDugBlocks { get; }
        int OwnedEquipmentCount { get; }
        int EquipmentUpgradeCount { get; }

        bool EnsureOwnedEquipmentCollection();
        bool EnsureUpgradeCollection();
        bool OwnsEquipment(string _itemId);
        bool AddOwnedEquipment(string _itemId);
        string GetOwnedEquipmentAt(int _index);
        void RemoveOwnedEquipmentAt(int _index);
        string GetEquippedEquipmentId(EquipmentType _type);
        bool SetEquippedEquipmentId(EquipmentType _type, string _itemId);
        int GetEquipmentLevel(string _equipmentId);
        bool TryGetEquipmentUpgradeAt(int _index, out string _equipmentId, out int _level);
        bool TrySetEquipmentLevel(string _equipmentId, int _level);
        void ClearEquipmentUpgrades();
        bool SetSaveVersion(int _version);
        bool TrySpendMoney(long _amount);
        bool TryAddMoney(long _amount);
        bool Normalize();
        void ApplyRunResult(
            float _depth,
            long _score,
            long _earnedMoney,
            int _dugBlocks,
            out long _previousMoney,
            out bool _isNewBestDepth,
            out bool _isNewBestScore);
    }
private class ProgressData : IProgressDataOperations
{
    private interface IUpgradeProgressOperations
    {
        string EquipmentId { get; }
        int Level { get; }
    }

    [Serializable]
    private class UpgradeProgress : IUpgradeProgressOperations
    {
        #region Fields

        [SerializeField]
        private string _equipmentId;

        [SerializeField]
        private int _level;

        #endregion

        #region Properties

        string IUpgradeProgressOperations.EquipmentId => _equipmentId;

        int IUpgradeProgressOperations.Level => _level;

        #endregion

        #region Events

        #endregion

        #region Unity Methods

        #endregion

        #region Private Methods

        public UpgradeProgress()
        {
            _equipmentId = string.Empty;
            _level = 0;
        }

        public UpgradeProgress(string _equipmentId, int _level)
        {
            this._equipmentId = _equipmentId;
            this._level = _level;
        }

        #endregion
    }
    #region Fields

    [SerializeField]
    private int _saveVersion = 3;

    [SerializeField]
    private long _totalMoney;

    [SerializeField]
    private float _bestDepth;

    [SerializeField]
    private long _bestScore;

    [SerializeField]
    private int _totalRuns;

    [SerializeField]
    private long _totalDugBlocks;

    [SerializeField]
    private List<string> _ownedEquipmentIds = new List<string>();

    [SerializeField]
    private string _equippedDrillId;

    [SerializeField]
    private string _equippedLauncherId;

    [SerializeField]
    private List<UpgradeProgress> _equipmentUpgrades =
        new List<UpgradeProgress>();

    #endregion

    #region Properties

    private int SaveVersion => _saveVersion;

    private long TotalMoney => _totalMoney;

    private float BestDepth => _bestDepth;

    private long BestScore => _bestScore;

    private int TotalRuns => _totalRuns;

    private long TotalDugBlocks => _totalDugBlocks;

    private string EquippedDrillId => _equippedDrillId;

    private string EquippedLauncherId => _equippedLauncherId;

    private int OwnedEquipmentCount => _ownedEquipmentIds == null ? 0 : _ownedEquipmentIds.Count;

    private int EquipmentUpgradeCount => _equipmentUpgrades == null ? 0 : _equipmentUpgrades.Count;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Private Methods

    public ProgressData()
    {
        _saveVersion = 3;
        _ownedEquipmentIds = new List<string>();
        _equipmentUpgrades = new List<UpgradeProgress>();
    }

    private bool EnsureOwnedEquipmentCollection()
    {
        if (_ownedEquipmentIds != null)
            return false;

        _ownedEquipmentIds = new List<string>();
        return true;
    }

    private bool EnsureUpgradeCollection()
    {
        if (_equipmentUpgrades != null)
            return false;

        _equipmentUpgrades = new List<UpgradeProgress>();
        return true;
    }

    private bool OwnsEquipment(string _itemId)
    {
        return !string.IsNullOrWhiteSpace(_itemId) &&
               _ownedEquipmentIds != null &&
               _ownedEquipmentIds.Contains(_itemId);
    }

    private bool AddOwnedEquipment(string _itemId)
    {
        if (string.IsNullOrWhiteSpace(_itemId) || OwnsEquipment(_itemId))
            return false;

        EnsureOwnedEquipmentCollection();
        _ownedEquipmentIds.Add(_itemId);
        return true;
    }

    private string GetOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return string.Empty;

        return _ownedEquipmentIds[_index];
    }

    private void RemoveOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return;

        _ownedEquipmentIds.RemoveAt(_index);
    }

    private string GetEquippedEquipmentId(EquipmentType _type)
    {
        if (_type == EquipmentType.Drill)
            return _equippedDrillId;

        if (_type == EquipmentType.Launcher)
            return _equippedLauncherId;

        return string.Empty;
    }

    private bool SetEquippedEquipmentId(EquipmentType _type, string _itemId)
    {
        if (_type != EquipmentType.Drill && _type != EquipmentType.Launcher)
            return false;

        string normalizedId = _itemId ?? string.Empty;

        if (_type == EquipmentType.Drill)
        {
            if (_equippedDrillId == normalizedId)
                return false;

            _equippedDrillId = normalizedId;
            return true;
        }

        if (_equippedLauncherId == normalizedId)
            return false;

        _equippedLauncherId = normalizedId;
        return true;
    }

    private int GetEquipmentLevel(string _equipmentId)
    {
        if (string.IsNullOrWhiteSpace(_equipmentId) || _equipmentUpgrades == null)
            return 0;

        for (int index = 0; index < _equipmentUpgrades.Count; index++)
        {
            UpgradeProgress progress = _equipmentUpgrades[index];

            if (progress != null && ((IUpgradeProgressOperations)progress).EquipmentId == _equipmentId)
                return Mathf.Max(0, ((IUpgradeProgressOperations)progress).Level);
        }

        return 0;
    }

    private UpgradeProgress GetEquipmentUpgradeAt(int _index)
    {
        if (_equipmentUpgrades == null || _index < 0 || _index >= _equipmentUpgrades.Count)
            return null;

        return _equipmentUpgrades[_index];
    }

    private bool TrySetEquipmentLevel(string _equipmentId, int _level)
    {
        if (string.IsNullOrWhiteSpace(_equipmentId) || _level < 0)
            return false;

        EnsureUpgradeCollection();

        for (int index = 0; index < _equipmentUpgrades.Count; index++)
        {
            UpgradeProgress progress = _equipmentUpgrades[index];

            if (progress == null || ((IUpgradeProgressOperations)progress).EquipmentId != _equipmentId)
                continue;

            if (((IUpgradeProgressOperations)progress).Level == _level)
                return false;

            _equipmentUpgrades[index] = new UpgradeProgress(_equipmentId, _level);
            return true;
        }

        _equipmentUpgrades.Add(new UpgradeProgress(_equipmentId, _level));
        return true;
    }

    private void RemoveEquipmentUpgradeAt(int _index)
    {
        if (_equipmentUpgrades == null || _index < 0 || _index >= _equipmentUpgrades.Count)
            return;

        _equipmentUpgrades.RemoveAt(_index);
    }

    private void ClearEquipmentUpgrades()
    {
        EnsureUpgradeCollection();
        _equipmentUpgrades.Clear();
    }

    private bool SetSaveVersion(int _version)
    {
        if (_version <= 0 || _saveVersion == _version)
            return false;

        _saveVersion = _version;
        return true;
    }

    private bool TrySpendMoney(long _amount)
    {
        if (_amount < 0 || _totalMoney < _amount)
            return false;

        _totalMoney -= _amount;
        return true;
    }

    private bool TryAddMoney(long _amount)
    {
        if (_amount <= 0 || long.MaxValue - _totalMoney < _amount)
            return false;

        _totalMoney += _amount;
        return true;
    }

    private bool Normalize()
    {
        bool changed = false;

        if (_totalMoney < 0)
        {
            _totalMoney = 0;
            changed = true;
        }

        if (_bestDepth < 0f || float.IsNaN(_bestDepth) || float.IsInfinity(_bestDepth))
        {
            _bestDepth = 0f;
            changed = true;
        }

        if (_bestScore < 0)
        {
            _bestScore = 0;
            changed = true;
        }

        if (_totalRuns < 0)
        {
            _totalRuns = 0;
            changed = true;
        }

        if (_totalDugBlocks < 0)
        {
            _totalDugBlocks = 0;
            changed = true;
        }

        changed |= EnsureOwnedEquipmentCollection();
        changed |= EnsureUpgradeCollection();
        return changed;
    }

    private void ApplyRunResult(
        float _depth,
        long _score,
        long _earnedMoney,
        int _dugBlocks,
        out long _previousMoney,
        out bool _isNewBestDepth,
        out bool _isNewBestScore)
    {
        _previousMoney = _totalMoney;
        _isNewBestDepth = _depth > _bestDepth;
        _isNewBestScore = _score > _bestScore;

        _totalMoney = AddSaturated(_totalMoney, _earnedMoney);
        _bestDepth = Math.Max(_bestDepth, _depth);
        _bestScore = Math.Max(_bestScore, _score);
        _totalRuns = _totalRuns == int.MaxValue ? int.MaxValue : _totalRuns + 1;
        _totalDugBlocks = AddSaturated(_totalDugBlocks, Math.Max(0, _dugBlocks));
    }

    #endregion

    #region Private Methods

    private long AddSaturated(long _current, long _amount)
    {
        if (_amount <= 0)
            return _current;

        if (long.MaxValue - _current < _amount)
            return long.MaxValue;

        return _current + _amount;
    }

    #endregion
    #region Service Accessors

    int IProgressDataOperations.SaveVersion => SaveVersion;
    long IProgressDataOperations.TotalMoney => TotalMoney;
    float IProgressDataOperations.BestDepth => BestDepth;
    long IProgressDataOperations.BestScore => BestScore;
    int IProgressDataOperations.TotalRuns => TotalRuns;
    long IProgressDataOperations.TotalDugBlocks => TotalDugBlocks;
    int IProgressDataOperations.OwnedEquipmentCount => OwnedEquipmentCount;
    int IProgressDataOperations.EquipmentUpgradeCount => EquipmentUpgradeCount;

    bool IProgressDataOperations.EnsureOwnedEquipmentCollection()
    {
        return EnsureOwnedEquipmentCollection();
    }

    bool IProgressDataOperations.EnsureUpgradeCollection()
    {
        return EnsureUpgradeCollection();
    }

    bool IProgressDataOperations.OwnsEquipment(string _itemId)
    {
        return OwnsEquipment(_itemId);
    }

    bool IProgressDataOperations.AddOwnedEquipment(string _itemId)
    {
        return AddOwnedEquipment(_itemId);
    }

    string IProgressDataOperations.GetOwnedEquipmentAt(int _index)
    {
        return GetOwnedEquipmentAt(_index);
    }

    void IProgressDataOperations.RemoveOwnedEquipmentAt(int _index)
    {
        RemoveOwnedEquipmentAt(_index);
    }

    string IProgressDataOperations.GetEquippedEquipmentId(EquipmentType _type)
    {
        return GetEquippedEquipmentId(_type);
    }

    bool IProgressDataOperations.SetEquippedEquipmentId(
        EquipmentType _type,
        string _itemId)
    {
        return SetEquippedEquipmentId(_type, _itemId);
    }

    int IProgressDataOperations.GetEquipmentLevel(string _equipmentId)
    {
        return GetEquipmentLevel(_equipmentId);
    }

    bool IProgressDataOperations.TryGetEquipmentUpgradeAt(
        int _index,
        out string _equipmentId,
        out int _level)
    {
        _equipmentId = string.Empty;
        _level = 0;

        if (_equipmentUpgrades == null ||
            _index < 0 ||
            _index >= _equipmentUpgrades.Count)
        {
            return false;
        }

        UpgradeProgress progress = _equipmentUpgrades[_index];

        if (progress == null)
            return false;

        _equipmentId = ((IUpgradeProgressOperations)progress).EquipmentId;
        _level = ((IUpgradeProgressOperations)progress).Level;
        return true;
    }

    bool IProgressDataOperations.TrySetEquipmentLevel(
        string _equipmentId,
        int _level)
    {
        return TrySetEquipmentLevel(_equipmentId, _level);
    }

    void IProgressDataOperations.ClearEquipmentUpgrades()
    {
        ClearEquipmentUpgrades();
    }

    bool IProgressDataOperations.SetSaveVersion(int _version)
    {
        return SetSaveVersion(_version);
    }

    bool IProgressDataOperations.TrySpendMoney(long _amount)
    {
        return TrySpendMoney(_amount);
    }

    bool IProgressDataOperations.TryAddMoney(long _amount)
    {
        return TryAddMoney(_amount);
    }

    bool IProgressDataOperations.Normalize()
    {
        return Normalize();
    }

    void IProgressDataOperations.ApplyRunResult(
        float _depth,
        long _score,
        long _earnedMoney,
        int _dugBlocks,
        out long _previousMoney,
        out bool _isNewBestDepth,
        out bool _isNewBestScore)
    {
        ApplyRunResult(
            _depth,
            _score,
            _earnedMoney,
            _dugBlocks,
            out _previousMoney,
            out _isNewBestDepth,
            out _isNewBestScore
        );
    }

    #endregion
}
    #endregion

    #region Private Methods

    private void HandleRunFinished(RunResult _result)
    {
        ApplyRunResult(_result);
    }

    private void Load()
    {
        _data = CreateDefaultData();
        _hasLoaded = true;
        EnsureEquipmentData();
        NormalizeUpgradeData();

        if (!File.Exists(SaveFilePath))
        {
            Save();
            ProgressChanged?.Invoke();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                HandleInvalidSave("The progress file is empty.");
                return;
            }

            ProgressData loadedData = JsonUtility.FromJson<ProgressData>(json);

            if (loadedData == null ||
                ((IProgressDataOperations)loadedData).SaveVersion <= 0 ||
                ((IProgressDataOperations)loadedData).SaveVersion > CurrentSaveVersion)
            {
                HandleInvalidSave("The progress file has an unsupported save version.");
                return;
            }

            _data = loadedData;
            bool changed = DataOperations.Normalize();
            changed |= MigrateToCurrentVersion();
            changed |= EnsureEquipmentData();
            changed |= NormalizeUpgradeData();

            if (changed)
                Save();

            ProgressChanged?.Invoke();
        }
        catch (ArgumentException _exception)
        {
            HandleInvalidSave("The progress file contains invalid JSON.", _exception);
        }
        catch (IOException _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
        catch (UnauthorizedAccessException _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
    }

    private bool MigrateToCurrentVersion()
    {
        bool changed = false;

        while (DataOperations.SaveVersion < CurrentSaveVersion)
        {
            if (DataOperations.SaveVersion == 1)
            {
                changed |= DataOperations.SetSaveVersion(2);
                continue;
            }

            if (DataOperations.SaveVersion == 2)
            {
                changed |= DataOperations.SetSaveVersion(3);
                continue;
            }

            return changed;
        }

        return changed;
    }

    private bool EnsureEquipmentData()
    {
        if (_data == null)
            return false;

        bool changed = DataOperations.EnsureOwnedEquipmentCollection();

        if (_equipmentCatalog == null)
        {
            LogMissingCatalog();
            return changed;
        }

        HashSet<string> seenIds = new HashSet<string>();

        for (int index = DataOperations.OwnedEquipmentCount - 1; index >= 0; index--)
        {
            string itemId = DataOperations.GetOwnedEquipmentAt(index);

            if (string.IsNullOrWhiteSpace(itemId) || !seenIds.Add(itemId))
            {
                DataOperations.RemoveOwnedEquipmentAt(index);
                changed = true;
            }
        }

        changed |= AddDefaultEquipment(EquipmentType.Drill);
        changed |= AddDefaultEquipment(EquipmentType.Launcher);
        changed |= EnsureValidEquippedEquipment(EquipmentType.Drill);
        changed |= EnsureValidEquippedEquipment(EquipmentType.Launcher);
        return changed;
    }

    private bool NormalizeUpgradeData()
    {
        if (_data == null)
            return false;

        bool changed = DataOperations.EnsureUpgradeCollection();
        Dictionary<string, int> normalizedLevels = new Dictionary<string, int>();

        for (int index = 0; index < DataOperations.EquipmentUpgradeCount; index++)
        {
            if (!DataOperations.TryGetEquipmentUpgradeAt(
                    index,
                    out string equipmentId,
                    out int rawLevel))
            {
                changed = true;
                continue;
            }

            int normalizedLevel = Mathf.Max(0, rawLevel);

            if (_equipmentCatalog != null &&
                _equipmentCatalog.TryGetById(equipmentId, out EquipmentItemDefinition item))
            {
                normalizedLevel = Mathf.Clamp(
                    normalizedLevel,
                    0,
                    item.GetMaximumUpgradeLevel()
                );
            }

            if (normalizedLevel != rawLevel)
                changed = true;

            if (normalizedLevels.TryGetValue(equipmentId, out int existingLevel))
            {
                normalizedLevels[equipmentId] = Mathf.Max(existingLevel, normalizedLevel);
                changed = true;
                continue;
            }

            normalizedLevels.Add(equipmentId, normalizedLevel);
        }

        if (!changed)
            return false;

        DataOperations.ClearEquipmentUpgrades();

        foreach (KeyValuePair<string, int> entry in normalizedLevels)
            DataOperations.TrySetEquipmentLevel(entry.Key, entry.Value);

        return true;
    }

    private bool AddDefaultEquipment(EquipmentType _type)
    {
        if (!_equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition item))
            return false;

        return DataOperations.AddOwnedEquipment(item.Id);
    }

    private bool EnsureValidEquippedEquipment(EquipmentType _type)
    {
        string currentId = DataOperations.GetEquippedEquipmentId(_type);

        if (IsValidEquippedId(currentId, _type))
            return false;

        string fallbackId = string.Empty;

        if (_equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition defaultItem))
            fallbackId = defaultItem.Id;

        return DataOperations.SetEquippedEquipmentId(_type, fallbackId);
    }

    private bool IsValidEquippedId(string _itemId, EquipmentType _type)
    {
        return DataOperations.OwnsEquipment(_itemId) &&
               _equipmentCatalog.TryGetById(_itemId, out EquipmentItemDefinition item) &&
               item.EquipmentType == _type &&
               item.HasValidConfiguration();
    }

    private bool TryGetCurrentAttributes(
        EquipmentItemDefinition _item,
        int _level,
        out DrillStats _drillStats,
        out float _launcherMultiplier)
    {
        return TryGetAttributes(_item, _level, out _drillStats, out _launcherMultiplier);
    }

    private bool TryGetAttributes(
        EquipmentItemDefinition _item,
        int _level,
        out DrillStats _drillStats,
        out float _launcherMultiplier)
    {
        _drillStats = default;
        _launcherMultiplier = 0f;

        if (_item.EquipmentType == EquipmentType.Drill)
            return _item.TryGetDrillStats(_level, out _drillStats);

        if (_item.EquipmentType == EquipmentType.Launcher)
            return _item.TryGetLauncherForceMultiplier(_level, out _launcherMultiplier);

        return false;
    }

    private bool TrySpendMoney(long _amount)
    {
        return _data != null && DataOperations.TrySpendMoney(_amount);
    }

    private bool CanApplyRunResult(RunResult _result)
    {
        if (!EnsureLoaded() || _rewardCalculator == null)
        {
            if (!_hasLoggedRewardCalculatorError)
            {
                Debug.LogError("PlayerProgressService needs a RunRewardCalculator reference.", this);
                _hasLoggedRewardCalculatorError = true;
            }

            return false;
        }

        if (_result.EndReason != RunEndReason.EnergyDepleted)
            return false;

        if (_result.RunId <= 0 || _lastAppliedRunId >= _result.RunId)
            return false;

        if (_result.Time < 0f || float.IsNaN(_result.Time) || float.IsInfinity(_result.Time))
            return false;

        if (_result.Depth < 0f || float.IsNaN(_result.Depth) || float.IsInfinity(_result.Depth))
            return false;

        return _result.DugBlocks >= 0;
    }

    private bool EnsureLoaded()
    {
        if (!_hasLoaded)
            Load();

        return _hasLoaded && _data != null;
    }

    private ProgressData CreateDefaultData()
    {
        return new ProgressData();
    }

    private PurchaseResult CreatePurchaseFailure(
        string _itemId,
        PurchaseFailureReason _reason)
    {
        return new PurchaseResult(
            false,
            _reason,
            _itemId,
            0,
            TotalMoney,
            true
        );
    }

    private UpgradePurchaseResult CreateUpgradeFailure(
        string _equipmentId,
        UpgradePurchaseFailureReason _reason,
        int _previousLevel,
        bool _persisted)
    {
        return new UpgradePurchaseResult(
            false,
            _reason,
            _equipmentId,
            _previousLevel,
            _previousLevel,
            0,
            TotalMoney,
            _persisted
        );
    }

    private EquipResult CreateEquipFailure(
        string _itemId,
        EquipFailureReason _reason)
    {
        return new EquipResult(
            false,
            _reason,
            _itemId,
            EquipmentType.Drill,
            false,
            true
        );
    }

    private void HandleInvalidSave(string _message)
    {
        HandleInvalidSave(_message, null);
    }

    private void HandleInvalidSave(string _message, Exception _exception)
    {
        PreserveInvalidSave();

        if (_exception == null)
            Debug.LogWarning($"{_message} A new progress file will be created.", this);
        else
            Debug.LogWarning($"{_message} A new progress file will be created. {_exception.Message}", this);

        _data = CreateDefaultData();
        EnsureEquipmentData();
        NormalizeUpgradeData();
        Save();
        ProgressChanged?.Invoke();
    }

    private void PreserveInvalidSave()
    {
        if (!File.Exists(SaveFilePath))
            return;

        string invalidPath = SaveFilePath + ".invalid." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json";

        try
        {
            File.Move(SaveFilePath, invalidPath);
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
        }
    }

    private void DeleteInvalidSaveFiles()
    {
        try
        {
            string directory = Application.persistentDataPath;
            string searchPattern = SaveFileName + ".invalid.*.json";

            foreach (string path in Directory.GetFiles(directory, searchPattern))
                DeleteFileIfExists(path);
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
        }
    }

    private void DeleteFileIfExists(string _path)
    {
        try
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
        catch (Exception _exception)
        {
            LogSaveError(_exception);
        }
    }

    private void LogMissingCatalog()
    {
        if (_hasLoggedCatalogError)
            return;

        _hasLoggedCatalogError = true;
        Debug.LogError("PlayerProgressService needs an EquipmentCatalog reference.", this);
    }

    private void LogSaveError(Exception _exception)
    {
        Debug.LogError($"Player progress could not be saved: {_exception.Message}", this);
    }

    private void LogLoadError(Exception _exception)
    {
        Debug.LogError($"Player progress could not be loaded: {_exception.Message}", this);
    }

#if UNITY_EDITOR
    [ContextMenu("Development/Add Test Money")]
    private void AddTestMoney()
    {
        if (!EnsureLoaded() || _data == null || !DataOperations.TryAddMoney(1000))
            return;

        Save();
        ProgressChanged?.Invoke();
    }
#endif

    #endregion
}