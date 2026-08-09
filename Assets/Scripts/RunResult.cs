public enum RunEndReason
{
    EnergyDepleted,
    ReachedCore,
    ManualRestart
}

public readonly struct RunResult
{
    #region Fields

    private readonly float _time;
    private readonly float _depth;
    private readonly int _dugBlocks;
    private readonly long _collectedMoney;
    private readonly float _collectedMineralValue;
    private readonly RunEndReason _endReason;
    private readonly int _runId;

    #endregion

    #region Properties

    public float Time => _time;

    public float Depth => _depth;

    public int DugBlocks => _dugBlocks;

    public long CollectedMoney => _collectedMoney;

    public float CollectedMineralValue => _collectedMineralValue;

    public RunEndReason EndReason => _endReason;

    public int RunId => _runId;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public RunResult(
        int _runId,
        float _time,
        float _depth,
        int _dugBlocks,
        RunEndReason _endReason)
        : this(
            _runId,
            _time,
            _depth,
            _dugBlocks,
            0,
            _endReason)
    {
    }

    public RunResult(
        int _runId,
        float _time,
        float _depth,
        int _dugBlocks,
        long _collectedMoney,
        RunEndReason _endReason)
        : this(
            _runId,
            _time,
            _depth,
            _dugBlocks,
            _collectedMoney,
            0f,
            _endReason)
    {
    }

    public RunResult(
        int _runId,
        float _time,
        float _depth,
        int _dugBlocks,
        long _collectedMoney,
        float _collectedMineralValue,
        RunEndReason _endReason)
    {
        this._runId = _runId;
        this._time = _time;
        this._depth = _depth;
        this._dugBlocks = _dugBlocks;
        this._collectedMoney = _collectedMoney;
        this._collectedMineralValue = _collectedMineralValue;
        this._endReason = _endReason;
    }

    #endregion

    #region Private Methods

    #endregion
}
