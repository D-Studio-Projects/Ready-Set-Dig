using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EquipmentCatalog",
    menuName = "Digging Madness/Equipment/Catalog"
)]
public class EquipmentCatalog : ScriptableObject
{
    #region Fields

    [SerializeField]
    private List<EquipmentItemDefinition> _items = new List<EquipmentItemDefinition>();

    private readonly Dictionary<string, EquipmentItemDefinition> _itemsById =
        new Dictionary<string, EquipmentItemDefinition>();

    private EquipmentItemDefinition _defaultDrill;
    private EquipmentItemDefinition _defaultLauncher;
    private bool _isInitialized;

    #endregion

    #region Properties

    public int ItemCount
    {
        get
        {
            EnsureInitialized();
            return _items == null ? 0 : _items.Count;
        }
    }

    public string DefaultDrillId => GetDefaultId(EquipmentType.Drill);

    public string DefaultLauncherId => GetDefaultId(EquipmentType.Launcher);

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        Initialize();
    }

    #endregion

    #region Public Methods

    public void Initialize()
    {
        _itemsById.Clear();
        _defaultDrill = null;
        _defaultLauncher = null;
        _isInitialized = true;

        if (_items == null)
        {
            Debug.LogError("EquipmentCatalog has a null item list.", this);
            return;
        }

        foreach (EquipmentItemDefinition item in _items)
        {
            if (item == null)
            {
                Debug.LogWarning("EquipmentCatalog contains a null item entry.", this);
                continue;
            }

            if (!item.HasValidId())
            {
                Debug.LogError(
                    $"Equipment item '{item.name}' has an empty ID and will be ignored.",
                    item
                );
                continue;
            }

            if (_itemsById.ContainsKey(item.Id))
            {
                Debug.LogError(
                    $"EquipmentCatalog contains duplicate equipment ID '{item.Id}'. The later entry will be ignored.",
                    this
                );
                continue;
            }

            _itemsById.Add(item.Id, item);

            if (!item.HasValidConfiguration())
            {
                Debug.LogError(
                    $"Equipment item '{item.Id}' has an invalid base configuration and will be unavailable.",
                    item
                );
                continue;
            }

            if (!item.HasValidUpgradeConfiguration())
            {
                Debug.LogError(
                    $"Equipment item '{item.Id}' has an invalid upgrade configuration. Its level 0 remains available.",
                    item
                );
            }

            if (!item.IsDefault)
                continue;

            if (item.EquipmentType == EquipmentType.Drill)
                RegisterDefault(ref _defaultDrill, item);
            else
                RegisterDefault(ref _defaultLauncher, item);
        }

        if (_defaultDrill == null)
            Debug.LogError("EquipmentCatalog has no valid default Drill item.", this);

        if (_defaultLauncher == null)
            Debug.LogError("EquipmentCatalog has no valid default Launcher item.", this);
    }

    public bool TryGetById(string _id, out EquipmentItemDefinition _item)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(_id))
        {
            _item = null;
            return false;
        }

        return _itemsById.TryGetValue(_id, out _item);
    }

    public EquipmentItemDefinition GetItemAt(int _index)
    {
        EnsureInitialized();

        if (_items == null || _index < 0 || _index >= _items.Count)
            return null;

        EquipmentItemDefinition item = _items[_index];

        if (item == null || !_itemsById.TryGetValue(item.Id, out EquipmentItemDefinition resolved))
            return null;

        return resolved == item ? item : null;
    }

    public int GetItemCount(EquipmentType _type)
    {
        EnsureInitialized();
        int count = 0;

        if (_items == null)
            return count;

        foreach (EquipmentItemDefinition item in _items)
        {
            if (IsAvailableItemOfType(item, _type))
                count++;
        }

        return count;
    }

    public EquipmentItemDefinition GetItemAt(EquipmentType _type, int _index)
    {
        EnsureInitialized();

        if (_items == null || _index < 0)
            return null;

        int currentIndex = 0;

        foreach (EquipmentItemDefinition item in _items)
        {
            if (!IsAvailableItemOfType(item, _type))
                continue;

            if (currentIndex == _index)
                return item;

            currentIndex++;
        }

        return null;
    }

    public bool TryGetDefault(EquipmentType _type, out EquipmentItemDefinition _item)
    {
        EnsureInitialized();
        _item = _type == EquipmentType.Drill ? _defaultDrill : _defaultLauncher;
        return _item != null;
    }

    #endregion

    #region Private Methods

    private void EnsureInitialized()
    {
        if (!_isInitialized)
            Initialize();
    }

    private bool IsAvailableItemOfType(
        EquipmentItemDefinition _item,
        EquipmentType _type)
    {
        return _item != null &&
               _item.EquipmentType == _type &&
               _itemsById.TryGetValue(_item.Id, out EquipmentItemDefinition resolved) &&
               resolved == _item;
    }

    private string GetDefaultId(EquipmentType _type)
    {
        return TryGetDefault(_type, out EquipmentItemDefinition item) ? item.Id : string.Empty;
    }

    private void RegisterDefault(
        ref EquipmentItemDefinition _currentDefault,
        EquipmentItemDefinition _candidate)
    {
        if (_currentDefault != null)
        {
            Debug.LogError(
                $"EquipmentCatalog contains more than one default {_candidate.EquipmentType} item. The first entry remains the default.",
                this
            );
            return;
        }

        _currentDefault = _candidate;
    }

    #endregion
}
