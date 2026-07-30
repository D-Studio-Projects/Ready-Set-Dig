public enum GlobalUpgradePurchaseFailureReason
{
    None,
    ProgressNotLoaded,
    InvalidUpgrade,
    MaximumLevelReached,
    InsufficientMoney,
    InvalidPrice,
    OperationInProgress,
    SaveFailed
}

public readonly struct GlobalUpgradePurchaseResult
{
    #region Fields

    private readonly bool _succeeded;
    private readonly GlobalUpgradePurchaseFailureReason _failureReason;
    private readonly GlobalUpgradeType _upgradeType;
    private readonly int _previousLevel;
    private readonly int _newLevel;
    private readonly long _spentMoney;
    private readonly long _remainingMoney;
    private readonly bool _persisted;

    #endregion

    #region Properties

    public bool Succeeded => _succeeded;

    public GlobalUpgradePurchaseFailureReason FailureReason => _failureReason;

    public GlobalUpgradeType UpgradeType => _upgradeType;

    public int PreviousLevel => _previousLevel;

    public int NewLevel => _newLevel;

    public long SpentMoney => _spentMoney;

    public long RemainingMoney => _remainingMoney;

    public bool Persisted => _persisted;

    #endregion

    #region Public Methods

    public GlobalUpgradePurchaseResult(
        bool _succeeded,
        GlobalUpgradePurchaseFailureReason _failureReason,
        GlobalUpgradeType _upgradeType,
        int _previousLevel,
        int _newLevel,
        long _spentMoney,
        long _remainingMoney,
        bool _persisted)
    {
        this._succeeded = _succeeded;
        this._failureReason = _failureReason;
        this._upgradeType = _upgradeType;
        this._previousLevel = _previousLevel;
        this._newLevel = _newLevel;
        this._spentMoney = _spentMoney;
        this._remainingMoney = _remainingMoney;
        this._persisted = _persisted;
    }

    #endregion
}
