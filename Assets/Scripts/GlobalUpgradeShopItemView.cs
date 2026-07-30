using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalUpgradeShopItemView : MonoBehaviour, IPointerEnterHandler
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private Button _selectButton;

    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Text _levelText;

    [SerializeField]
    private Text _priceText;

    [SerializeField]
    private Text _effectText;

    [SerializeField]
    private Text _statusText;

    private GlobalUpgradeType _upgradeType;
    private bool _isConfigured;

    #endregion

    #region Properties

    public GlobalUpgradeType UpgradeType => _upgradeType;

    #endregion

    #region Events

    public event Action<GlobalUpgradeType> Selected;
    public event Action<GlobalUpgradeType> Hovered;

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

    public void Configure(GlobalUpgradeInfo _info)
    {
        _isConfigured = true;
        _upgradeType = _info.UpgradeType;
        SetVisible(true);

        if (_nameText != null)
            _nameText.text = _info.DisplayName;

        if (_levelText != null)
            _levelText.text = $"NIVEL {_info.CurrentLevel}/{_info.MaximumLevel}";

        if (_priceText != null)
        {
            _priceText.text = _info.HasNextLevel
                ? $"$ {_info.NextLevelPrice:N0}"
                : "MAXIMO";
        }

        if (_effectText != null)
            _effectText.text = GetEffectText(_info);

        if (_statusText != null)
            _statusText.text = GetStatusText(_info);

        if (_selectButton != null)
            _selectButton.interactable = true;
    }

    public void SetVisible(bool _visible)
    {
        if (!_visible)
            _isConfigured = false;

        gameObject.SetActive(_visible);
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        if (_isConfigured)
            Hovered?.Invoke(_upgradeType);
    }

    #endregion

    #region Private Methods

    private void HandleSelected()
    {
        if (_isConfigured)
            Selected?.Invoke(_upgradeType);
    }

    private string GetEffectText(GlobalUpgradeInfo _info)
    {
        if (_info.UpgradeType == GlobalUpgradeType.DashCount)
            return $"{_info.CurrentFlatBonus + 1} DASH";

        return $"x{_info.CurrentMultiplier:0.##}";
    }

    private string GetStatusText(GlobalUpgradeInfo _info)
    {
        if (!_info.HasNextLevel)
            return "NIVEL MAXIMO";

        return _info.CurrentMoney >= _info.NextLevelPrice
            ? "MELHORAR"
            : "SEM DINHEIRO";
    }

    #endregion
}
