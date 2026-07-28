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
    private readonly RunEndReason _endReason;

    #endregion

    #region Properties

    public float Time => _time;

    public float Depth => _depth;

    public int DugBlocks => _dugBlocks;

    public RunEndReason EndReason => _endReason;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public RunResult(
        float _time,
        float _depth,
        int _dugBlocks,
        RunEndReason _endReason)
    {
        this._time = _time;
        this._depth = _depth;
        this._dugBlocks = _dugBlocks;
        this._endReason = _endReason;
    }

    #endregion

    #region Private Methods

    #endregion
}
