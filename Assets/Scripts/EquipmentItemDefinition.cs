using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EquipmentItemDefinition",
    menuName = "Digging Madness/Equipment/Item Definition"
)]
public class EquipmentItemDefinition : ScriptableObject
{
    #region Fields

    [Header("Identity")]
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private EquipmentType _equipmentType;

    [SerializeField]
    private long _price;

    [SerializeField]
    private Sprite _icon;

    [TextArea]
    [SerializeField]
    private string _description;

    [Header("Default")]
    [SerializeField]
    private bool _isDefault;

    [Header("Drill Gameplay")]
    [SerializeField]
    private ToolData _drillToolData;

    [Header("Launcher Gameplay")]
    [SerializeField]
    private LauncherData _launcherData;

    [Header("Upgrades")]
    [SerializeField]
    private int _maximumUpgradeLevel = 3;

    [SerializeField]
    private List<DrillUpgradeLevel> _drillUpgradeLevels = new List<DrillUpgradeLevel>();

    [SerializeField]
    private List<LauncherUpgradeLevel> _launcherUpgradeLevels = new List<LauncherUpgradeLevel>();

    #endregion

    #region Properties

    public string Id => _id;

    public string DisplayName => _displayName;

    public EquipmentType EquipmentType => _equipmentType;

    public long Price => _price;

    public Sprite Icon => _icon;

    public string Description => _description;

    public bool IsDefault => _isDefault;

    public ToolData DrillToolData => _drillToolData;

    public LauncherData LauncherData => _launcherData;

    public float LaunchForceMultiplier =>
        _launcherData == null ? 0f : _launcherData.GetForceMultiplier(0);

    public int MaximumUpgradeLevel => Mathf.Max(0, _maximumUpgradeLevel);

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool HasValidId()
    {
        return !string.IsNullOrWhiteSpace(_id);
    }

    public bool HasValidConfiguration()
    {
        if (!HasValidId() || string.IsNullOrWhiteSpace(_displayName) || _price < 0)
            return false;

        if (_equipmentType == EquipmentType.Drill)
            return _drillToolData != null;

        if (_equipmentType != EquipmentType.Launcher)
            return false;

        return _launcherData != null &&
               _launcherData.HasValidConfiguration() &&
               _launcherData.Id == _id;
    }

    public bool HasValidUpgradeConfiguration()
    {
        if (_maximumUpgradeLevel < 0)
            return false;

        if (MaximumUpgradeLevel == 0)
            return IsUpgradeListEmpty();

        if (_equipmentType == EquipmentType.Drill)
            return HasValidDrillUpgradeConfiguration();

        if (_equipmentType == EquipmentType.Launcher)
            return HasValidLauncherUpgradeConfiguration();

        return false;
    }

    public int GetMaximumUpgradeLevel()
    {
        return MaximumUpgradeLevel;
    }

    public bool TryGetDrillStats(int _level, out DrillStats _stats)
    {
        _stats = DrillStats.FromToolData(_drillToolData);

        if (_equipmentType != EquipmentType.Drill || _level < 0 || _level > MaximumUpgradeLevel)
            return false;

        if (_level == 0)
            return _stats.IsValid();

        int index = _level - 1;

        if (_drillUpgradeLevels == null || index >= _drillUpgradeLevels.Count)
            return false;

        DrillUpgradeLevel upgrade = _drillUpgradeLevels[index];

        if (upgrade == null || !upgrade.HasValidConfiguration())
            return false;

        _stats = upgrade.GetStats();
        return _stats.IsValid();
    }

    public bool TryGetLauncherForceMultiplier(int _level, out float _multiplier)
    {
        _multiplier = 0f;

        if (!TryGetLauncherData(_level, out LauncherData launcherData))
            return false;

        _multiplier = launcherData.GetForceMultiplier(_level);
        return IsValidMultiplier(_multiplier);
    }

    public bool TryGetLauncherData(int _level, out LauncherData _launcher)
    {
        _launcher = _launcherData;

        if (_equipmentType != EquipmentType.Launcher ||
            _level < 0 ||
            _level > MaximumUpgradeLevel ||
            _launcher == null ||
            !_launcher.HasValidConfiguration() ||
            _launcher.Id != _id)
        {
            return false;
        }

        if (_level == 0)
            return true;

        int index = _level - 1;

        return _launcherUpgradeLevels != null &&
               index < _launcherUpgradeLevels.Count &&
               _launcherUpgradeLevels[index] != null &&
               _launcherUpgradeLevels[index].HasValidConfiguration();
    }

    public bool TryGetUpgradePrice(int _level, out long _price)
    {
        _price = 0;

        if (_level <= 0 || _level > MaximumUpgradeLevel)
            return false;

        int index = _level - 1;

        if (_equipmentType == EquipmentType.Drill)
        {
            if (_drillUpgradeLevels == null || index >= _drillUpgradeLevels.Count || _drillUpgradeLevels[index] == null)
                return false;

            _price = _drillUpgradeLevels[index].Price;
            return true;
        }

        if (_equipmentType != EquipmentType.Launcher ||
            _launcherUpgradeLevels == null ||
            index >= _launcherUpgradeLevels.Count ||
            _launcherUpgradeLevels[index] == null)
        {
            return false;
        }

        _price = _launcherUpgradeLevels[index].Price;
        return true;
    }

    #endregion

    #region Private Methods

    private bool HasValidDrillUpgradeConfiguration()
    {
        if (_equipmentType != EquipmentType.Drill ||
            _drillUpgradeLevels == null ||
            _drillUpgradeLevels.Count != MaximumUpgradeLevel ||
            !IsUpgradeListEmpty(_launcherUpgradeLevels))
        {
            return false;
        }

        long previousPrice = -1;

        for (int index = 0; index < MaximumUpgradeLevel; index++)
        {
            DrillUpgradeLevel upgrade = _drillUpgradeLevels[index];

            if (upgrade == null ||
                !upgrade.HasValidConfiguration() ||
                upgrade.Price <= previousPrice)
            {
                return false;
            }

            previousPrice = upgrade.Price;
        }

        return true;
    }

    private bool HasValidLauncherUpgradeConfiguration()
    {
        if (_equipmentType != EquipmentType.Launcher ||
            _launcherUpgradeLevels == null ||
            _launcherUpgradeLevels.Count != MaximumUpgradeLevel ||
            !IsUpgradeListEmpty(_drillUpgradeLevels))
        {
            return false;
        }

        long previousPrice = -1;

        for (int index = 0; index < MaximumUpgradeLevel; index++)
        {
            LauncherUpgradeLevel upgrade = _launcherUpgradeLevels[index];

            if (upgrade == null ||
                !upgrade.HasValidConfiguration() ||
                upgrade.Price <= previousPrice)
            {
                return false;
            }

            previousPrice = upgrade.Price;
        }

        return true;
    }

    private bool IsUpgradeListEmpty<T>(List<T> _levels) where T : class
    {
        return _levels == null || _levels.Count == 0;
    }

    private bool IsUpgradeListEmpty()
    {
        return IsUpgradeListEmpty(_drillUpgradeLevels) &&
               IsUpgradeListEmpty(_launcherUpgradeLevels);
    }

    private bool IsValidMultiplier(float _multiplier)
    {
        return _multiplier > 0f &&
               !float.IsNaN(_multiplier) &&
               !float.IsInfinity(_multiplier);
    }

    #endregion
}
