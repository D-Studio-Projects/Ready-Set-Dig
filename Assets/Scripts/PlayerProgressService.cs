using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class PlayerProgressService : MonoBehaviour
{
    #region Fields

    public const int CurrentSaveVersion = 2;

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

    private PlayerProgressData _data;
    private int _lastAppliedRunId;
    private bool _hasLoaded;
    private bool _hasLoggedRewardCalculatorError;
    private bool _hasLoggedCatalogError;
    private bool _purchaseInProgress;
    private bool _equipInProgress;

    #endregion

    #region Properties

    public bool IsLoaded => _hasLoaded;

    public long TotalMoney => _data == null ? 0 : _data.TotalMoney;

    public float BestDepth => _data == null ? 0f : _data.BestDepth;

    public long BestScore => _data == null ? 0 : _data.BestScore;

    public int TotalRuns => _data == null ? 0 : _data.TotalRuns;

    public long TotalDugBlocks => _data == null ? 0 : _data.TotalDugBlocks;

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
        return _data != null && _data.OwnsEquipment(_itemId);
    }

    public string GetEquippedEquipmentId(EquipmentType _type)
    {
        EnsureLoaded();
        return _data == null ? string.Empty : _data.GetEquippedEquipmentId(_type);
    }

    public bool TryGetEquippedEquipment(
        EquipmentType _type,
        out EquipmentItemDefinition _item)
    {
        _item = null;

        if (!EnsureLoaded() || _equipmentCatalog == null || _data == null)
            return false;

        string equippedId = _data.GetEquippedEquipmentId(_type);

        if (!_equipmentCatalog.TryGetById(equippedId, out EquipmentItemDefinition item) ||
            item.EquipmentType != _type ||
            !item.HasValidConfiguration() ||
            !_data.OwnsEquipment(equippedId))
        {
            return false;
        }

        _item = item;
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

            if (_data.OwnsEquipment(item.Id))
            {
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.AlreadyOwned);
            }

            if (item.Price < 0)
            {
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InvalidPrice);
            }

            if (!item.HasValidConfiguration())
            {
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InvalidItem);
            }
            if (_data.TotalMoney < item.Price)
            {
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InsufficientMoney);
            }

            if (!TrySpendMoney(item.Price))
            {
                return CreatePurchaseFailure(item.Id, PurchaseFailureReason.InsufficientMoney);
            }

            _data.AddOwnedEquipment(item.Id);
            bool persistenceSucceeded = Save();
            ProgressChanged?.Invoke();

            return new PurchaseResult(
                true,
                persistenceSucceeded ? PurchaseFailureReason.None : PurchaseFailureReason.SaveFailed,
                item.Id,
                item.Price,
                _data.TotalMoney,
                persistenceSucceeded
            );
        }
        finally
        {
            _purchaseInProgress = false;
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
            if (!_data.OwnsEquipment(item.Id))
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

            string equippedId = _data.GetEquippedEquipmentId(item.EquipmentType);

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

            bool changed = _data.SetEquippedEquipmentId(item.EquipmentType, item.Id);
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
        _data.ApplyRunResult(
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
        _lastAppliedRunId = 0;
        _hasLoaded = true;
        Save();
        ProgressChanged?.Invoke();
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

            PlayerProgressData loadedData = JsonUtility.FromJson<PlayerProgressData>(json);

            if (loadedData == null ||
                loadedData.SaveVersion <= 0 ||
                loadedData.SaveVersion > CurrentSaveVersion)
            {
                HandleInvalidSave("The progress file has an unsupported save version.");
                return;
            }

            _data = loadedData;
            bool changed = _data.Normalize();

            if (_data.SaveVersion == 1)
                changed |= MigrateFromVersionOne();

            changed |= EnsureEquipmentData();

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

    private bool MigrateFromVersionOne()
    {
        bool changed = _data.SetSaveVersion(CurrentSaveVersion);
        return changed;
    }

    private bool EnsureEquipmentData()
    {
        if (_data == null)
            return false;

        bool changed = _data.EnsureOwnedEquipmentCollection();

        if (_equipmentCatalog == null)
        {
            LogMissingCatalog();
            return changed;
        }

        HashSet<string> seenIds = new HashSet<string>();

        for (int index = _data.OwnedEquipmentCount - 1; index >= 0; index--)
        {
            string itemId = _data.GetOwnedEquipmentAt(index);

            if (string.IsNullOrWhiteSpace(itemId) || !seenIds.Add(itemId))
            {
                _data.RemoveOwnedEquipmentAt(index);
                changed = true;
            }
        }

        changed |= AddDefaultEquipment(EquipmentType.Drill);
        changed |= AddDefaultEquipment(EquipmentType.Launcher);
        changed |= EnsureValidEquippedEquipment(EquipmentType.Drill);
        changed |= EnsureValidEquippedEquipment(EquipmentType.Launcher);
        return changed;
    }

    private bool AddDefaultEquipment(EquipmentType _type)
    {
        if (!_equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition item))
            return false;

        return _data.AddOwnedEquipment(item.Id);
    }

    private bool EnsureValidEquippedEquipment(EquipmentType _type)
    {
        string currentId = _data.GetEquippedEquipmentId(_type);

        if (IsValidEquippedId(currentId, _type))
            return false;

        string fallbackId = string.Empty;

        if (_equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition defaultItem))
            fallbackId = defaultItem.Id;

        return _data.SetEquippedEquipmentId(_type, fallbackId);
    }

    private bool IsValidEquippedId(string _itemId, EquipmentType _type)
    {
        return _data.OwnsEquipment(_itemId) &&
               _equipmentCatalog.TryGetById(_itemId, out EquipmentItemDefinition item) &&
               item.EquipmentType == _type &&
               item.HasValidConfiguration();
    }

    private bool TrySpendMoney(long _amount)
    {
        return _data != null && _data.TrySpendMoney(_amount);
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

    private PlayerProgressData CreateDefaultData()
    {
        return new PlayerProgressData();
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

    #endregion
}


