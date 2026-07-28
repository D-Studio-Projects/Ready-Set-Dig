public readonly struct ProgressUpdate
{
    #region Fields

    private readonly RunResult _runResult;
    private readonly long _score;
    private readonly long _earnedMoney;
    private readonly long _previousMoney;
    private readonly long _totalMoney;
    private readonly bool _isNewBestDepth;
    private readonly bool _isNewBestScore;

    #endregion

    #region Properties

    public RunResult RunResult => _runResult;

    public long Score => _score;

    public long EarnedMoney => _earnedMoney;

    public long PreviousMoney => _previousMoney;

    public long TotalMoney => _totalMoney;

    public bool IsNewBestDepth => _isNewBestDepth;

    public bool IsNewBestScore => _isNewBestScore;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public ProgressUpdate(
        RunResult _runResult,
        long _score,
        long _earnedMoney,
        long _previousMoney,
        long _totalMoney,
        bool _isNewBestDepth,
        bool _isNewBestScore)
    {
        this._runResult = _runResult;
        this._score = _score;
        this._earnedMoney = _earnedMoney;
        this._previousMoney = _previousMoney;
        this._totalMoney = _totalMoney;
        this._isNewBestDepth = _isNewBestDepth;
        this._isNewBestScore = _isNewBestScore;
    }

    #endregion

    #region Private Methods

    #endregion
}
