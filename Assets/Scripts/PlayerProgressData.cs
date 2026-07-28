using System;
using UnityEngine;

[Serializable]
public class PlayerProgressData
{
    #region Fields

    [SerializeField]
    private int _saveVersion = 1;

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

    #endregion

    #region Properties

    public int SaveVersion => _saveVersion;

    public long TotalMoney => _totalMoney;

    public float BestDepth => _bestDepth;

    public long BestScore => _bestScore;

    public int TotalRuns => _totalRuns;

    public long TotalDugBlocks => _totalDugBlocks;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public PlayerProgressData()
    {
        _saveVersion = 1;
    }

    #endregion

    #region Private Methods

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
