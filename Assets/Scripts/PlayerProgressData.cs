using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{
    #region Fields

    [SerializeField]
    private int _saveVersion = 2;

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

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public PlayerProgressData()
    {
        _saveVersion = 2;
        _ownedEquipmentIds = new List<string>();
    }

    #endregion

    #region Internal Methods

    internal bool EnsureOwnedEquipmentCollection()
    {
        if (_ownedEquipmentIds != null)
            return false;

        _ownedEquipmentIds = new List<string>();
        return true;
    }

    internal bool OwnsEquipment(string _itemId)
    {
        return !string.IsNullOrWhiteSpace(_itemId) &&
               _ownedEquipmentIds != null &&
               _ownedEquipmentIds.Contains(_itemId);
    }

    internal bool AddOwnedEquipment(string _itemId)
    {
        if (string.IsNullOrWhiteSpace(_itemId) || OwnsEquipment(_itemId))
            return false;

        EnsureOwnedEquipmentCollection();
        _ownedEquipmentIds.Add(_itemId);
        return true;
    }

    internal int OwnedEquipmentCount => _ownedEquipmentIds == null ? 0 : _ownedEquipmentIds.Count;

    internal string GetOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return string.Empty;

        return _ownedEquipmentIds[_index];
    }

    internal void RemoveOwnedEquipmentAt(int _index)
    {
        if (_ownedEquipmentIds == null || _index < 0 || _index >= _ownedEquipmentIds.Count)
            return;

        _ownedEquipmentIds.RemoveAt(_index);
    }

    internal string GetEquippedEquipmentId(EquipmentType _type)
    {
        return _type == EquipmentType.Drill ? _equippedDrillId : _equippedLauncherId;
    }

    internal bool SetEquippedEquipmentId(EquipmentType _type, string _itemId)
    {
        if (_type == EquipmentType.Drill)
        {
            if (_equippedDrillId == _itemId)
                return false;

            _equippedDrillId = _itemId;
            return true;
        }

        if (_equippedLauncherId == _itemId)
            return false;

        _equippedLauncherId = _itemId;
        return true;
    }

    internal bool SetSaveVersion(int _version)
    {
        if (_saveVersion == _version)
            return false;

        _saveVersion = _version;
        return true;
    }

    internal bool TrySpendMoney(long _amount)
    {
        if (_amount < 0 || _totalMoney < _amount)
            return false;

        _totalMoney -= _amount;
        return true;
    }

    internal bool Normalize()
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

        return changed;
    }

    internal void ApplyRunResult(
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

