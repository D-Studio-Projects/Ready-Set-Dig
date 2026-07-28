public enum UpgradePurchaseFailureReason
{
    None,
    ProgressNotLoaded,
    InvalidItem,
    NotOwned,
    MaximumLevelReached,
    InvalidUpgradeConfiguration,
    InvalidPrice,
    InsufficientMoney,
    OperationInProgress,
    SaveFailed
}

public readonly struct UpgradePurchaseResult
{
    #region Fields

    private readonly bool _success;
    private readonly UpgradePurchaseFailureReason _failureReason;
    private readonly string _equipmentId;
    private readonly int _previousLevel;
    private readonly int _newLevel;
    private readonly long _pricePaid;
    private readonly long _remainingMoney;
    private readonly bool _persisted;

    #endregion

    #region Properties

    public bool Success => _success;

    public UpgradePurchaseFailureReason FailureReason => _failureReason;

    public string EquipmentId => _equipmentId;

    public int PreviousLevel => _previousLevel;

    public int NewLevel => _newLevel;

    public long PricePaid => _pricePaid;

    public long RemainingMoney => _remainingMoney;

    public bool Persisted => _persisted;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public UpgradePurchaseResult(
        bool _success,
        UpgradePurchaseFailureReason _failureReason,
        string _equipmentId,
        int _previousLevel,
        int _newLevel,
        long _pricePaid,
        long _remainingMoney,
        bool _persisted)
    {
        this._success = _success;
        this._failureReason = _failureReason;
        this._equipmentId = _equipmentId;
        this._previousLevel = _previousLevel;
        this._newLevel = _newLevel;
        this._pricePaid = _pricePaid;
        this._remainingMoney = _remainingMoney;
        this._persisted = _persisted;
    }

    #endregion

    #region Private Methods

    #endregion
}
