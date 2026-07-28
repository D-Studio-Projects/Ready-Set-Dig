public readonly struct RunReward
{
    #region Fields

    private readonly long _earnedMoney;
    private readonly long _score;

    #endregion

    #region Properties

    public long EarnedMoney => _earnedMoney;

    public long Score => _score;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public RunReward(long _earnedMoney, long _score)
    {
        this._earnedMoney = _earnedMoney;
        this._score = _score;
    }

    #endregion

    #region Private Methods

    #endregion
}
