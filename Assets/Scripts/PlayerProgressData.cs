using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{
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
    private List<EquipmentUpgradeProgress> _equipmentUpgrades =
        new List<EquipmentUpgradeProgress>();

    #endregion

    #region Properties

    public int SaveVersion => _saveVersion;

    public long TotalMoney => _totalMoney;

    public float BestDepth => _bestDepth;

    public long BestScore => _bestScore;

    public int TotalRuns => _totalRuns;

    public long TotalDugBlocks => _totalDugBlocks;

    public string EquippedDrillId => _equippedDrillId;

    public string EquippedLauncherId => _equippedLauncherId;

    public int OwnedEquipmentCount => _ownedEquipmentIds == null ? 0 : _ownedEquipmentIds.Count;

    public int EquipmentUpgradeCount => _equipmentUpgrades == null ? 0 : _equipmentUpgrades.Count;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public PlayerProgressData()
    {
        _saveVersion = 3;
        _ownedEquipmentIds = new List<string>();
        _equipmentUpgrades = new List<EquipmentUpgradeProgress>();
    }

    public bool EnsureOwnedEquipmentCollection()
    {
        if (_ownedEquipmentIds != null)
            return false;

        _ownedEquipmentIds = new List<string>();
        return true;
    }

    public bool EnsureUpgradeCollection()
    {
        if (_equipmentUpgrades != null)
            return false;

        _equipmentUpgrades = new List<EquipmentUpgradeProgress>();
        return true;
    }

    public bool OwnsEquipment(string _itemId)
    {
        return !string.IsNullOrWhiteSpace(_itemId) &&
               _ownedEquipmentIds != null &&
               _ownedEquipmentIds.Contains(_itemId);
    }

    public bool AddOwnedEquipment(string _itemId)
    {
        if (string.IsNullOrWhiteSpace(_itemId) || OwnsEquipment(_itemId))
            return false;

        EnsureOwnedEquipmentCollection();
        _ownedEquipmentIds.Add(_itemId);
        return true;
    }

    public string GetOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return string.Empty;

        return _ownedEquipmentIds[_index];
    }

    public void RemoveOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return;

        _ownedEquipmentIds.RemoveAt(_index);
    }

    public string GetEquippedEquipmentId(EquipmentType _type)
    {
        if (_type == EquipmentType.Drill)
            return _equippedDrillId;

        if (_type == EquipmentType.Launcher)
            return _equippedLauncherId;

        return string.Empty;
    }

    public bool SetEquippedEquipmentId(EquipmentType _type, string _itemId)
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

    public int GetEquipmentLevel(string _equipmentId)
    {
        if (string.IsNullOrWhiteSpace(_equipmentId) || _equipmentUpgrades == null)
            return 0;

        for (int index = 0; index < _equipmentUpgrades.Count; index++)
        {
            EquipmentUpgradeProgress progress = _equipmentUpgrades[index];

            if (progress != null && progress.EquipmentId == _equipmentId)
                return Mathf.Max(0, progress.Level);
        }

        return 0;
    }

    public EquipmentUpgradeProgress GetEquipmentUpgradeAt(int _index)
    {
        if (_equipmentUpgrades == null || _index < 0 || _index >= _equipmentUpgrades.Count)
            return null;

        return _equipmentUpgrades[_index];
    }

    public bool TrySetEquipmentLevel(string _equipmentId, int _level)
    {
        if (string.IsNullOrWhiteSpace(_equipmentId) || _level < 0)
            return false;

        EnsureUpgradeCollection();

        for (int index = 0; index < _equipmentUpgrades.Count; index++)
        {
            EquipmentUpgradeProgress progress = _equipmentUpgrades[index];

            if (progress == null || progress.EquipmentId != _equipmentId)
                continue;

            if (progress.Level == _level)
                return false;

            _equipmentUpgrades[index] = new EquipmentUpgradeProgress(_equipmentId, _level);
            return true;
        }

        _equipmentUpgrades.Add(new EquipmentUpgradeProgress(_equipmentId, _level));
        return true;
    }

    public void RemoveEquipmentUpgradeAt(int _index)
    {
        if (_equipmentUpgrades == null || _index < 0 || _index >= _equipmentUpgrades.Count)
            return;

        _equipmentUpgrades.RemoveAt(_index);
    }

    public void ClearEquipmentUpgrades()
    {
        EnsureUpgradeCollection();
        _equipmentUpgrades.Clear();
    }

    public bool SetSaveVersion(int _version)
    {
        if (_version <= 0 || _saveVersion == _version)
            return false;

        _saveVersion = _version;
        return true;
    }

    public bool TrySpendMoney(long _amount)
    {
        if (_amount < 0 || _totalMoney < _amount)
            return false;

        _totalMoney -= _amount;
        return true;
    }

    public bool TryAddMoney(long _amount)
    {
        if (_amount <= 0 || long.MaxValue - _totalMoney < _amount)
            return false;

        _totalMoney += _amount;
        return true;
    }

    public bool Normalize()
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

    public void ApplyRunResult(
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
}