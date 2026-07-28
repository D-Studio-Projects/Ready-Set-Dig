public enum PurchaseFailureReason
{
    None,
    NotLoaded,
    OperationInProgress,
    InvalidItem,
    AlreadyOwned,
    InsufficientMoney,
    InvalidPrice,
    SaveFailed
}

public readonly struct PurchaseResult
{
    #region Fields

    private readonly bool _success;
    private readonly PurchaseFailureReason _failureReason;
    private readonly string _itemId;
    private readonly long _pricePaid;
    private readonly long _remainingMoney;
    private readonly bool _persistenceSucceeded;

    #endregion

    #region Properties

    public bool Success => _success;

    public PurchaseFailureReason FailureReason => _failureReason;

    public string ItemId => _itemId;

    public long PricePaid => _pricePaid;

    public long RemainingMoney => _remainingMoney;

    public bool PersistenceSucceeded => _persistenceSucceeded;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public PurchaseResult(
        bool _success,
        PurchaseFailureReason _failureReason,
        string _itemId,
        long _pricePaid,
        long _remainingMoney,
        bool _persistenceSucceeded)
    {
        this._success = _success;
        this._failureReason = _failureReason;
        this._itemId = _itemId;
        this._pricePaid = _pricePaid;
        this._remainingMoney = _remainingMoney;
        this._persistenceSucceeded = _persistenceSucceeded;
    }

    #endregion

    #region Private Methods

    #endregion
}
