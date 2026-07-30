public readonly struct GlobalUpgradeInfo
{
    #region Fields

    private readonly GlobalUpgradeType _upgradeType;
    private readonly string _displayName;
    private readonly string _description;
    private readonly int _currentLevel;
    private readonly int _maximumLevel;
    private readonly bool _hasNextLevel;
    private readonly bool _canPurchase;
    private readonly long _nextLevelPrice;
    private readonly long _currentMoney;
    private readonly float _currentMultiplier;
    private readonly float _nextMultiplier;
    private readonly int _currentFlatBonus;
    private readonly int _nextFlatBonus;

    #endregion

    #region Properties

    public GlobalUpgradeType UpgradeType => _upgradeType;

    public string DisplayName => _displayName;

    public string Description => _description;

    public int CurrentLevel => _currentLevel;

    public int MaximumLevel => _maximumLevel;

    public bool HasNextLevel => _hasNextLevel;

    public bool CanPurchase => _canPurchase;

    public long NextLevelPrice => _nextLevelPrice;

    public long CurrentMoney => _currentMoney;

    public float CurrentMultiplier => _currentMultiplier;

    public float NextMultiplier => _nextMultiplier;

    public int CurrentFlatBonus => _currentFlatBonus;

    public int NextFlatBonus => _nextFlatBonus;

    #endregion

    #region Public Methods

    public GlobalUpgradeInfo(
        GlobalUpgradeType _upgradeType,
        string _displayName,
        string _description,
        int _currentLevel,
        int _maximumLevel,
        bool _hasNextLevel,
        bool _canPurchase,
        long _nextLevelPrice,
        long _currentMoney,
        float _currentMultiplier,
        float _nextMultiplier,
        int _currentFlatBonus,
        int _nextFlatBonus)
    {
        this._upgradeType = _upgradeType;
        this._displayName = _displayName;
        this._description = _description;
        this._currentLevel = _currentLevel;
        this._maximumLevel = _maximumLevel;
        this._hasNextLevel = _hasNextLevel;
        this._canPurchase = _canPurchase;
        this._nextLevelPrice = _nextLevelPrice;
        this._currentMoney = _currentMoney;
        this._currentMultiplier = _currentMultiplier;
        this._nextMultiplier = _nextMultiplier;
        this._currentFlatBonus = _currentFlatBonus;
        this._nextFlatBonus = _nextFlatBonus;
    }

    #endregion
}
