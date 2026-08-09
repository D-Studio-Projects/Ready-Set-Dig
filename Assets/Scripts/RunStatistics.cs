using System;
using UnityEngine;

public class RunStatistics : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private Transform _player;


    private float _time;
    private float _distance;
    private float _maxDepth;
    private float _startPositionY;
    private long _money;
    private double _collectedMineralValue;
    private int _dugBlocks;
    private int _dashCount;
    private bool _hasRunStarted;

    #endregion

    #region Properties

    public float Time => _time;

    public float Distance => _distance;

    public float Depth => _maxDepth;

    public float MaxDepth => _maxDepth;

    public long Money => AddSaturated(
        _money,
        FloorMineralValue(_collectedMineralValue)
    );

    public float CollectedMineralValue => _collectedMineralValue >= float.MaxValue
        ? float.MaxValue
        : (float)_collectedMineralValue;

    public int DugBlocks => _dugBlocks;

    public int DashCount => _dashCount;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Reset();
    }

    private void Update()
    {
        if (_runManager == null || !_runManager.IsRunning)
            return;

        _time += UnityEngine.Time.deltaTime;
        TrackDepth();
    }

    #endregion

    #region Public Methods

    public void BeginRun()
    {
        if (_player == null)
        {
            _hasRunStarted = false;
            return;
        }

        _startPositionY = _player.position.y;
        _maxDepth = 0f;
        _hasRunStarted = true;
    }

    public void AddDistance(float _amount)
    {
        if (_amount <= 0f)
            return;

        _distance += _amount;
    }

    public void AddMoney(long _amount)
    {
        if (_amount <= 0)
            return;

        _money = AddSaturated(_money, _amount);
    }

    public void AddMineralValue(float _amount)
    {
        if (_amount <= 0f || float.IsNaN(_amount) || float.IsInfinity(_amount))
            return;

        _collectedMineralValue = Math.Min(
            float.MaxValue,
            _collectedMineralValue + _amount
        );
    }

    public void AddDiggedBlocks(int _amount)
    {
        if (_amount <= 0)
            return;

        _dugBlocks += _amount;
    }

    public void AddDigResult(DigResult _result)
    {
        AddDiggedBlocks(_result.TotalCells);
    }

    public void IncrementDashCount()
    {
        _dashCount++;
    }

    public RunResult CreateResult(int _runId, RunEndReason _reason)
    {
        return new RunResult(
            _runId,
            _time,
            _maxDepth,
            _dugBlocks,
            _money,
            CollectedMineralValue,
            _reason
        );
    }

    public void Reset()
    {
        _time = 0f;
        _distance = 0f;
        _maxDepth = 0f;
        _startPositionY = 0f;
        _money = 0;
        _collectedMineralValue = 0d;
        _dugBlocks = 0;
        _dashCount = 0;
        _hasRunStarted = false;
    }

    #endregion

    #region Private Methods

    private void TrackDepth()
    {
        if (!_hasRunStarted || _player == null)
            return;

        float currentDepth = Mathf.Max(
            0f,
            _startPositionY - _player.position.y
        );
        _maxDepth = Mathf.Max(_maxDepth, currentDepth);
    }

    private long FloorMineralValue(double _value)
    {
        if (_value <= 0d || double.IsNaN(_value))
            return 0;

        if (_value >= long.MaxValue || double.IsPositiveInfinity(_value))
            return long.MaxValue;

        return (long)Math.Floor(_value);
    }

    private long AddSaturated(long _left, long _right)
    {
        if (_right <= 0)
            return _left < 0 ? 0 : _left;

        if (long.MaxValue - _left < _right)
            return long.MaxValue;

        return _left + _right;
    }

    #endregion
}
