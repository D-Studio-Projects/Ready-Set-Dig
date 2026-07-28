using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private Button _selectButton;

    [SerializeField]
    private Image _icon;

    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Text _priceText;

    [SerializeField]
    private Text _categoryText;

    [SerializeField]
    private Text _statusText;

    private string _itemId;
    private bool _isConfigured;

    #endregion

    #region Properties

    public string ItemId => _itemId;

    #endregion

    #region Events

    public event Action<string> Selected;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_selectButton != null)
            _selectButton.onClick.AddListener(HandleSelected);
    }

    private void OnDisable()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelected);
    }

    #endregion

    #region Public Methods

    public void Configure(
        EquipmentItemDefinition _item,
        bool _isOwned,
        bool _isEquipped,
        bool _canAfford,
        bool _isValid)
    {
        _isConfigured = _item != null;
        _itemId = _item == null ? string.Empty : _item.Id;

        if (!_isConfigured)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (_icon != null)
        {
            _icon.sprite = _item.Icon;
            _icon.enabled = _item.Icon != null;
        }

        if (_nameText != null)
            _nameText.text = _item.DisplayName;

        if (_priceText != null)
            _priceText.text = _isOwned ? "POSSE" : $"$ {_item.Price:N0}";

        if (_categoryText != null)
            _categoryText.text = GetCategoryText(_item.EquipmentType);

        if (_statusText != null)
        {
            _statusText.text = !_isValid
                ? "INDISPONIVEL"
                : GetStatusText(_isOwned, _isEquipped, _canAfford);
        }

        if (_selectButton != null)
            _selectButton.interactable = _isValid;
    }

    public void SetVisible(bool _visible)
    {
        gameObject.SetActive(_visible);
    }

    #endregion

    #region Private Methods

    private void HandleSelected()
    {
        if (!_isConfigured || string.IsNullOrWhiteSpace(_itemId))
            return;

        Selected?.Invoke(_itemId);
    }

    private string GetStatusText(bool _isOwned, bool _isEquipped, bool _canAfford)
    {
        if (_isEquipped)
            return "EQUIPADO";

        if (_isOwned)
            return "POSSE";

        return _canAfford ? "COMPRAR" : "SEM DINHEIRO";
    }

    private string GetCategoryText(EquipmentType _type)
    {
        return _type == EquipmentType.Drill ? "BROCA" : "LANCADOR";
    }

    #endregion
}
