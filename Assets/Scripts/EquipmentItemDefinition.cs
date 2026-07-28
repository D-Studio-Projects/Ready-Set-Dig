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
    private float _launchForceMultiplier = 1f;

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

    public float LaunchForceMultiplier => _launchForceMultiplier;

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

        return _launchForceMultiplier > 0f &&
               !float.IsNaN(_launchForceMultiplier) &&
               !float.IsInfinity(_launchForceMultiplier);
    }

    #endregion

    #region Private Methods

    #endregion
}
