public readonly struct EquipmentUpgradeInfo
{
    #region Fields

    private readonly string _equipmentId;
    private readonly string _displayName;
    private readonly EquipmentType _equipmentType;
    private readonly bool _isOwned;
    private readonly bool _isEquipped;
    private readonly int _currentLevel;
    private readonly int _maximumLevel;
    private readonly bool _hasNextLevel;
    private readonly bool _canPurchaseUpgrade;
    private readonly long _nextLevelPrice;
    private readonly long _currentMoney;
    private readonly DrillStats _currentDrillStats;
    private readonly DrillStats _nextDrillStats;
    private readonly float _currentLaunchForceMultiplier;
    private readonly float _nextLaunchForceMultiplier;

    #endregion

    #region Properties

    public string EquipmentId => _equipmentId;

    public string DisplayName => _displayName;

    public EquipmentType EquipmentType => _equipmentType;

    public bool IsOwned => _isOwned;

    public bool IsEquipped => _isEquipped;

    public int CurrentLevel => _currentLevel;

    public int MaximumLevel => _maximumLevel;

    public bool HasNextLevel => _hasNextLevel;

    public bool CanPurchaseUpgrade => _canPurchaseUpgrade;

    public long NextLevelPrice => _nextLevelPrice;

    public long CurrentMoney => _currentMoney;

    public DrillStats CurrentDrillStats => _currentDrillStats;

    public DrillStats NextDrillStats => _nextDrillStats;

    public float CurrentLaunchForceMultiplier => _currentLaunchForceMultiplier;

    public float NextLaunchForceMultiplier => _nextLaunchForceMultiplier;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public EquipmentUpgradeInfo(
        string _equipmentId,
        string _displayName,
        EquipmentType _equipmentType,
        bool _isOwned,
        bool _isEquipped,
        int _currentLevel,
        int _maximumLevel,
        bool _hasNextLevel,
        bool _canPurchaseUpgrade,
        long _nextLevelPrice,
        long _currentMoney,
        DrillStats _currentDrillStats,
        DrillStats _nextDrillStats,
        float _currentLaunchForceMultiplier,
        float _nextLaunchForceMultiplier)
    {
        this._equipmentId = _equipmentId;
        this._displayName = _displayName;
        this._equipmentType = _equipmentType;
        this._isOwned = _isOwned;
        this._isEquipped = _isEquipped;
        this._currentLevel = _currentLevel;
        this._maximumLevel = _maximumLevel;
        this._hasNextLevel = _hasNextLevel;
        this._canPurchaseUpgrade = _canPurchaseUpgrade;
        this._nextLevelPrice = _nextLevelPrice;
        this._currentMoney = _currentMoney;
        this._currentDrillStats = _currentDrillStats;
        this._nextDrillStats = _nextDrillStats;
        this._currentLaunchForceMultiplier = _currentLaunchForceMultiplier;
        this._nextLaunchForceMultiplier = _nextLaunchForceMultiplier;
    }

    #endregion

    #region Private Methods

    #endregion
}
