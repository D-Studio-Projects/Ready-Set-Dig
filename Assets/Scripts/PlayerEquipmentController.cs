using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerEquipmentController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private PlayerProgressService _progressService;

    [SerializeField]
    private EquipmentCatalog _equipmentCatalog;

    [SerializeField]
    private Drill _drill;

    [SerializeField]
    private LaunchController _launchController;

    private bool _hasLoggedCatalogError;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ApplyEquippedEquipment();
    }

    #endregion

    #region Public Methods

    public void ApplyEquippedEquipment()
    {
        ApplyDrillEquipment();
        ApplyLauncherEquipment();
    }

    #endregion

    #region Private Methods

    private void ApplyDrillEquipment()
    {
        if (_drill == null)
            return;

        EquipmentItemDefinition item = ResolveEquipment(EquipmentType.Drill);

        if (item == null || item.DrillToolData == null)
        {
            _drill.ResetEquipment();
            return;
        }

        _drill.ApplyEquipment(item.DrillToolData);
    }

    private void ApplyLauncherEquipment()
    {
        if (_launchController == null)
            return;

        EquipmentItemDefinition item = ResolveEquipment(EquipmentType.Launcher);

        if (item == null)
        {
            _launchController.ResetEquipment();
            return;
        }

        _launchController.ApplyEquipment(item.LaunchForceMultiplier);
    }

    private EquipmentItemDefinition ResolveEquipment(EquipmentType _type)
    {
        if (_progressService != null &&
            _progressService.TryGetEquippedEquipment(_type, out EquipmentItemDefinition equippedItem))
        {
            return equippedItem;
        }

        if (_equipmentCatalog != null &&
            _equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition defaultItem))
        {
            return defaultItem;
        }

        if (!_hasLoggedCatalogError)
        {
            _hasLoggedCatalogError = true;
            Debug.LogError(
                $"PlayerEquipmentController could not resolve a {_type} equipment item.",
                this
            );
        }

        return null;
    }

    #endregion
}
