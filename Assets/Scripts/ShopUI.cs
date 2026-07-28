using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private PlayerProgressService _progressService;

    [SerializeField]
    private EquipmentCatalog _equipmentCatalog;

    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private Text _moneyText;

    [SerializeField]
    private Transform _itemContainer;

    [SerializeField]
    private ShopItemView[] _itemViews;

    [SerializeField]
    private Text _selectedNameText;

    [SerializeField]
    private Text _selectedCategoryText;

    [SerializeField]
    private Text _selectedDescriptionText;

    [SerializeField]
    private Text _selectedPriceText;

    [SerializeField]
    private Image _selectedIcon;

    [SerializeField]
    private Text _selectedAttributesText;

    [SerializeField]
    private Button _buyButton;

    [SerializeField]
    private Button _equipButton;

    [SerializeField]
    private Text _feedbackText;

    [SerializeField]
    private Button _startRunButton;

    private string _selectedItemId;
    private bool _isVisible;
    private bool _isProcessing;

    #endregion

    #region Properties

    public bool IsVisible => _isVisible;

    #endregion

    #region Events

    public event Action NewRunRequested;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_progressService != null)
            _progressService.ProgressChanged += HandleProgressChanged;

        if (_buyButton != null)
            _buyButton.onClick.AddListener(HandleBuyClicked);

        if (_equipButton != null)
            _equipButton.onClick.AddListener(HandleEquipClicked);

        if (_startRunButton != null)
            _startRunButton.onClick.AddListener(HandleStartRunClicked);

        if (_itemViews == null)
            return;

        foreach (ShopItemView itemView in _itemViews)
        {
            if (itemView != null)
                itemView.Selected += HandleItemSelected;
        }
    }

    private void OnDisable()
    {
        if (_progressService != null)
            _progressService.ProgressChanged -= HandleProgressChanged;

        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(HandleBuyClicked);

        if (_equipButton != null)
            _equipButton.onClick.RemoveListener(HandleEquipClicked);

        if (_startRunButton != null)
            _startRunButton.onClick.RemoveListener(HandleStartRunClicked);

        if (_itemViews == null)
            return;

        foreach (ShopItemView itemView in _itemViews)
        {
            if (itemView != null)
                itemView.Selected -= HandleItemSelected;
        }
    }

    #endregion

    #region Public Methods

    public void Open()
    {
        _isVisible = true;
        _isProcessing = false;

        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        _isVisible = false;
        _isProcessing = false;

        if (_panelRoot != null)
            _panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        if (!_isVisible)
            return;

        UpdateMoneyText();
        ConfigureItemViews();
        UpdateSelectedItem();
    }

    #endregion

    #region Private Methods

    private void HandleProgressChanged()
    {
        Refresh();
    }

    private void HandleItemSelected(string _itemId)
    {
        if (_isProcessing)
            return;

        _selectedItemId = _itemId;
        UpdateSelectedItem();
    }

    private void HandleBuyClicked()
    {
        if (_isProcessing || string.IsNullOrWhiteSpace(_selectedItemId))
            return;

        _isProcessing = true;
        SetOperationButtons(false);
        PurchaseResult result = _progressService == null
            ? new PurchaseResult(false, PurchaseFailureReason.NotLoaded, _selectedItemId, 0, 0, true)
            : _progressService.TryPurchaseEquipment(_selectedItemId);
        ShowPurchaseFeedback(result);
        _isProcessing = false;
        Refresh();
    }

    private void HandleEquipClicked()
    {
        if (_isProcessing || string.IsNullOrWhiteSpace(_selectedItemId))
            return;

        _isProcessing = true;
        SetOperationButtons(false);
        EquipResult result = _progressService == null
            ? new EquipResult(false, EquipFailureReason.NotLoaded, _selectedItemId, EquipmentType.Drill, false, true)
            : _progressService.TryEquipEquipment(_selectedItemId);
        ShowEquipFeedback(result);
        _isProcessing = false;
        Refresh();
    }

    private void HandleStartRunClicked()
    {
        if (_isProcessing)
            return;

        _isProcessing = true;
        NewRunRequested?.Invoke();
    }

    private void ConfigureItemViews()
    {
        if (_itemViews == null)
            return;

        int catalogCount = _equipmentCatalog == null ? 0 : _equipmentCatalog.ItemCount;

        for (int index = 0; index < _itemViews.Length; index++)
        {
            ShopItemView itemView = _itemViews[index];

            if (itemView == null)
                continue;

            EquipmentItemDefinition item = index < catalogCount
                ? _equipmentCatalog.GetItemAt(index)
                : null;

            if (item == null)
            {
                itemView.SetVisible(false);
                continue;
            }

            bool isOwned = _progressService != null && _progressService.OwnsEquipment(item.Id);
            bool isEquipped = IsEquipped(item);
            bool canAfford = _progressService != null && _progressService.TotalMoney >= item.Price;
            itemView.Configure(item, isOwned, isEquipped, canAfford, item.HasValidConfiguration());
        }
    }

    private void UpdateSelectedItem()
    {
        EquipmentItemDefinition item = ResolveSelectedItem();

        if (item == null)
        {
            ClearDetails();
            return;
        }

        bool isValid = item.HasValidConfiguration();
        bool isOwned = _progressService != null && _progressService.OwnsEquipment(item.Id);
        bool isEquipped = IsEquipped(item);
        bool canAfford = _progressService != null && _progressService.TotalMoney >= item.Price;

        if (_moneyText != null)
        {
            long totalMoney = _progressService == null ? 0 : _progressService.TotalMoney;
            _moneyText.text = $"DINHEIRO: $ {totalMoney:N0}";
        }

        if (_selectedNameText != null)
            _selectedNameText.text = item.DisplayName;

        if (_selectedCategoryText != null)
            _selectedCategoryText.text = GetCategoryText(item.EquipmentType);

        if (_selectedDescriptionText != null)
            _selectedDescriptionText.text = item.Description;

        if (_selectedPriceText != null)
            _selectedPriceText.text = isOwned ? "POSSE" : $"PRECO: $ {item.Price:N0}";

        if (_selectedIcon != null)
        {
            _selectedIcon.sprite = item.Icon;
            _selectedIcon.enabled = item.Icon != null;
        }

        if (_selectedAttributesText != null)
            _selectedAttributesText.text = GetAttributesText(item);

        if (_buyButton != null)
            _buyButton.interactable = !_isProcessing && isValid && !isOwned && canAfford;

        if (_equipButton != null)
            _equipButton.interactable = !_isProcessing && isValid && isOwned && !isEquipped;
    }

    private EquipmentItemDefinition ResolveSelectedItem()
    {
        if (_equipmentCatalog == null)
            return null;

        if (!string.IsNullOrWhiteSpace(_selectedItemId) &&
            _equipmentCatalog.TryGetById(_selectedItemId, out EquipmentItemDefinition selectedItem))
        {
            return selectedItem;
        }

        for (int index = 0; index < _equipmentCatalog.ItemCount; index++)
        {
            EquipmentItemDefinition item = _equipmentCatalog.GetItemAt(index);

            if (item == null)
                continue;

            _selectedItemId = item.Id;
            return item;
        }

        return null;
    }

    private bool IsEquipped(EquipmentItemDefinition _item)
    {
        return _progressService != null &&
               _progressService.GetEquippedEquipmentId(_item.EquipmentType) == _item.Id;
    }

    private void UpdateMoneyText()
    {
        if (_moneyText != null)
            _moneyText.text = $"DINHEIRO: $ {(_progressService == null ? 0 : _progressService.TotalMoney):N0}";
    }

    private void ClearDetails()
    {
        if (_selectedNameText != null)
            _selectedNameText.text = "NENHUM ITEM";

        if (_selectedCategoryText != null)
            _selectedCategoryText.text = string.Empty;

        if (_selectedDescriptionText != null)
            _selectedDescriptionText.text = string.Empty;

        if (_selectedPriceText != null)
            _selectedPriceText.text = string.Empty;

        if (_selectedAttributesText != null)
            _selectedAttributesText.text = string.Empty;

        if (_buyButton != null)
            _buyButton.interactable = false;

        if (_equipButton != null)
            _equipButton.interactable = false;
    }

    private void SetOperationButtons(bool _interactable)
    {
        if (_buyButton != null)
            _buyButton.interactable = _interactable;

        if (_equipButton != null)
            _equipButton.interactable = _interactable;

        if (_startRunButton != null)
            _startRunButton.interactable = _interactable;
    }

    private void ShowPurchaseFeedback(PurchaseResult _result)
    {
        if (_feedbackText == null)
            return;

        if (_result.Success)
        {
            _feedbackText.text = _result.PersistenceSucceeded
                ? "Compra concluida."
                : "Compra aplicada. Falha ao salvar; tente salvar novamente.";
            return;
        }

        _feedbackText.text = GetPurchaseFailureText(_result.FailureReason);
    }

    private void ShowEquipFeedback(EquipResult _result)
    {
        if (_feedbackText == null)
            return;

        if (_result.Success)
        {
            _feedbackText.text = _result.PersistenceSucceeded
                ? "Equipamento selecionado."
                : "Equipamento aplicado. Falha ao salvar; tente salvar novamente.";
            return;
        }

        _feedbackText.text = GetEquipFailureText(_result.FailureReason);
    }

    private string GetPurchaseFailureText(PurchaseFailureReason _reason)
    {
        switch (_reason)
        {
            case PurchaseFailureReason.AlreadyOwned:
                return "Este item ja foi comprado.";
            case PurchaseFailureReason.InsufficientMoney:
                return "Dinheiro insuficiente.";
            case PurchaseFailureReason.InvalidPrice:
                return "Preco invalido.";
            case PurchaseFailureReason.OperationInProgress:
                return "A compra ja esta sendo processada.";
            case PurchaseFailureReason.NotLoaded:
                return "Progresso ainda nao carregado.";
            default:
                return "Item indisponivel.";
        }
    }

    private string GetEquipFailureText(EquipFailureReason _reason)
    {
        switch (_reason)
        {
            case EquipFailureReason.NotOwned:
                return "Compre este item antes de equipar.";
            case EquipFailureReason.OperationInProgress:
                return "A troca ja esta sendo processada.";
            case EquipFailureReason.NotLoaded:
                return "Progresso ainda nao carregado.";
            default:
                return "Equipamento indisponivel.";
        }
    }

    private string GetCategoryText(EquipmentType _type)
    {
        return _type == EquipmentType.Drill ? "BROCA" : "LANCADOR";
    }

    private string GetAttributesText(EquipmentItemDefinition _item)
    {
        if (_item.EquipmentType == EquipmentType.Drill && _item.DrillToolData != null)
        {
            ToolData data = _item.DrillToolData;
            return $"Dano: x{data.DigDamage:0.##}\nVelocidade: x{data.DigSpeed:0.##}\nConsumo: {data.EnergyConsumption:0.##}";
        }

        if (_item.EquipmentType == EquipmentType.Launcher)
            return $"Forca de lancamento: x{_item.LaunchForceMultiplier:0.##}";

        return string.Empty;
    }

    #endregion
}



