public enum EquipFailureReason
{
    None,
    NotLoaded,
    OperationInProgress,
    InvalidItem,
    NotOwned,
    WrongCategory,
    SaveFailed
}

public readonly struct EquipResult
{
    #region Fields

    private readonly bool _success;
    private readonly EquipFailureReason _failureReason;
    private readonly string _itemId;
    private readonly EquipmentType _equipmentType;
    private readonly bool _changed;
    private readonly bool _persistenceSucceeded;

    #endregion

    #region Properties

    public bool Success => _success;

    public EquipFailureReason FailureReason => _failureReason;

    public string ItemId => _itemId;

    public EquipmentType EquipmentType => _equipmentType;

    public bool Changed => _changed;

    public bool PersistenceSucceeded => _persistenceSucceeded;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public EquipResult(
        bool _success,
        EquipFailureReason _failureReason,
        string _itemId,
        EquipmentType _equipmentType,
        bool _changed,
        bool _persistenceSucceeded)
    {
        this._success = _success;
        this._failureReason = _failureReason;
        this._itemId = _itemId;
        this._equipmentType = _equipmentType;
        this._changed = _changed;
        this._persistenceSucceeded = _persistenceSucceeded;
    }

    #endregion

    #region Private Methods

    #endregion
}
