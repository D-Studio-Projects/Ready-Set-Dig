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
    private int _money;
    private int _dugBlocks;
    private int _dashCount;
    private bool _hasRunStarted;

    #endregion

    #region Properties

    public float Time => _time;

    public float Distance => _distance;

    public float Depth => _maxDepth;

    public float MaxDepth => _maxDepth;

    public int Money => _money;

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

    public void AddMoney(int _amount)
    {
        if (_amount <= 0)
            return;

        _money += _amount;
    }

    public void AddDiggedBlocks(int _amount)
    {
        if (_amount <= 0)
            return;

        _dugBlocks += _amount;
    }

    public void IncrementDashCount()
    {
        _dashCount++;
    }

    public RunResult CreateResult(RunEndReason _reason)
    {
        return new RunResult(
            _time,
            _maxDepth,
            _dugBlocks,
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

    #endregion
}