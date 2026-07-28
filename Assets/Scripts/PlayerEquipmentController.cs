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

        if (TryResolveEquipment(
                EquipmentType.Drill,
                out EquipmentItemDefinition item,
                out int level) &&
            item.TryGetDrillStats(level, out DrillStats stats))
        {
            _drill.ApplyEquipment(stats);
            return;
        }

        if (_equipmentCatalog != null &&
            _equipmentCatalog.TryGetDefault(EquipmentType.Drill, out EquipmentItemDefinition defaultItem) &&
            defaultItem.TryGetDrillStats(0, out DrillStats defaultStats))
        {
            _drill.ApplyEquipment(defaultStats);
            return;
        }

        _drill.ResetEquipment();
    }

    private void ApplyLauncherEquipment()
    {
        if (_launchController == null)
            return;

        if (TryResolveEquipment(
                EquipmentType.Launcher,
                out EquipmentItemDefinition item,
                out int level) &&
            item.TryGetLauncherData(level, out LauncherData launcherData))
        {
            _launchController.ApplyEquipment(launcherData, level);
            return;
        }

        if (_equipmentCatalog != null &&
            _equipmentCatalog.TryGetDefault(EquipmentType.Launcher, out EquipmentItemDefinition defaultItem) &&
            defaultItem.TryGetLauncherData(0, out LauncherData defaultLauncherData))
        {
            _launchController.ApplyEquipment(defaultLauncherData, 0);
            return;
        }

        _launchController.ResetEquipment();
    }

    private bool TryResolveEquipment(
        EquipmentType _type,
        out EquipmentItemDefinition _item,
        out int _level)
    {
        _item = null;
        _level = 0;

        if (_progressService != null &&
            _progressService.TryGetEquippedEquipment(_type, out EquipmentItemDefinition equippedItem))
        {
            _item = equippedItem;
            _level = _progressService.GetEquipmentLevel(equippedItem.Id);
            return true;
        }

        if (_equipmentCatalog != null &&
            _equipmentCatalog.TryGetDefault(_type, out EquipmentItemDefinition defaultItem))
        {
            _item = defaultItem;
            return true;
        }

        if (!_hasLoggedCatalogError)
        {
            _hasLoggedCatalogError = true;
            Debug.LogError(
                $"PlayerEquipmentController could not resolve a {_type} equipment item.",
                this
            );
        }

        return false;
    }

    #endregion
}
