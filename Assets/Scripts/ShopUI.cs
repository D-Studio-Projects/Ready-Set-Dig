using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    #region Fields

    [Header("Core References")]
    [SerializeField]
    private PlayerProgressService _progressService;

    [SerializeField]
    private EquipmentCatalog _equipmentCatalog;

    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private Text _moneyText;

    [SerializeField]
    private Text _feedbackText;

    [SerializeField]
    private Button _startRunButton;

    [Header("Sections")]
    [SerializeField]
    private ShopSection _defaultSection = ShopSection.Equipment;

    [SerializeField]
    private Button _equipmentTabButton;

    [SerializeField]
    private Button _globalUpgradesTabButton;

    [SerializeField]
    private GameObject _equipmentSectionRoot;

    [SerializeField]
    private GameObject _globalUpgradeSectionRoot;

    [SerializeField]
    private Text _sectionTitleText;

    [Header("Equipment List")]
    [SerializeField]
    private Transform _itemContainer;

    [SerializeField]
    private ShopItemView[] _itemViews;

    [Header("Global Upgrade List")]
    [SerializeField]
    private Transform _globalUpgradeContainer;

    [SerializeField]
    private GlobalUpgradeShopItemView[] _globalUpgradeViews;

    [Header("Selected Product")]
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
    private Text _selectedLevelText;

    [SerializeField]
    private Text _selectedUpgradePreviewText;

    [SerializeField]
    private Text _upgradePriceText;

    [SerializeField]
    private Text _upgradeStatusText;

    [Header("Actions")]
    [SerializeField]
    private Button _buyButton;

    [SerializeField]
    private Button _equipButton;

    [SerializeField]
    private Button _upgradeButton;

    [Header("Merchant")]
    [SerializeField]
    private Text _merchantSpeechText;

    [TextArea]
    [SerializeField]
    private string _defaultMerchantSpeech =
        "Passe o mouse em um produto. Eu explico o que ele faz.";

    private string _selectedItemId;
    private GlobalUpgradeType _selectedGlobalUpgradeType;
    private ShopSection _currentSection;
    private bool _hasSelectedGlobalUpgrade;
    private bool _isVisible;
    private bool _isProcessing;

    #endregion

    #region Properties

    public bool IsVisible => _isVisible;

    public ShopSection CurrentSection => _currentSection;

    #endregion

    #region Events

    public event Action NewRunRequested;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ResolveItemViews();
        _currentSection = _defaultSection;
    }

    private void OnEnable()
    {
        if (_progressService != null)
            _progressService.ProgressChanged += HandleProgressChanged;

        if (_buyButton != null)
            _buyButton.onClick.AddListener(HandleBuyClicked);

        if (_equipButton != null)
            _equipButton.onClick.AddListener(HandleEquipClicked);

        if (_upgradeButton != null)
            _upgradeButton.onClick.AddListener(HandleUpgradeClicked);

        if (_startRunButton != null)
            _startRunButton.onClick.AddListener(HandleStartRunClicked);

        if (_equipmentTabButton != null)
            _equipmentTabButton.onClick.AddListener(ShowEquipmentSection);

        if (_globalUpgradesTabButton != null)
            _globalUpgradesTabButton.onClick.AddListener(ShowGlobalUpgradesSection);

        SubscribeItemViews(true);
    }

    private void OnDisable()
    {
        if (_progressService != null)
            _progressService.ProgressChanged -= HandleProgressChanged;

        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(HandleBuyClicked);

        if (_equipButton != null)
            _equipButton.onClick.RemoveListener(HandleEquipClicked);

        if (_upgradeButton != null)
            _upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);

        if (_startRunButton != null)
            _startRunButton.onClick.RemoveListener(HandleStartRunClicked);

        if (_equipmentTabButton != null)
            _equipmentTabButton.onClick.RemoveListener(ShowEquipmentSection);

        if (_globalUpgradesTabButton != null)
            _globalUpgradesTabButton.onClick.RemoveListener(ShowGlobalUpgradesSection);

        SubscribeItemViews(false);
    }

    #endregion

    #region Public Methods

    public void Open()
    {
        _isVisible = true;
        _isProcessing = false;
        _currentSection = _defaultSection;

        if (_panelRoot != null)
            _panelRoot.SetActive(true);

        ShowMerchantText(_defaultMerchantSpeech);
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
        ConfigureEquipmentViews();
        ConfigureGlobalUpgradeViews();
        ApplySectionVisibility();
        UpdateSelectedProduct();

        UpdateStartRunButton();
    }

    public void ShowEquipmentSection()
    {
        SwitchSection(ShopSection.Equipment);
    }

    public void ShowGlobalUpgradesSection()
    {
        SwitchSection(ShopSection.GlobalUpgrades);
    }

    #endregion

    #region Private Methods

    private void ResolveItemViews()
    {
        if ((_itemViews == null || _itemViews.Length == 0) &&
            _itemContainer != null)
        {
            _itemViews = _itemContainer.GetComponentsInChildren<ShopItemView>(true);
        }

        if ((_globalUpgradeViews == null || _globalUpgradeViews.Length == 0) &&
            _globalUpgradeContainer != null)
        {
            _globalUpgradeViews =
                _globalUpgradeContainer.GetComponentsInChildren<GlobalUpgradeShopItemView>(true);
        }
    }

    private void SubscribeItemViews(bool _subscribe)
    {
        if (_itemViews != null)
        {
            foreach (ShopItemView itemView in _itemViews)
            {
                if (itemView == null)
                    continue;

                if (_subscribe)
                {
                    itemView.Selected += HandleItemSelected;
                    itemView.Hovered += HandleItemHovered;
                }
                else
                {
                    itemView.Selected -= HandleItemSelected;
                    itemView.Hovered -= HandleItemHovered;
                }
            }
        }

        if (_globalUpgradeViews == null)
            return;

        foreach (GlobalUpgradeShopItemView upgradeView in _globalUpgradeViews)
        {
            if (upgradeView == null)
                continue;

            if (_subscribe)
            {
                upgradeView.Selected += HandleGlobalUpgradeSelected;
                upgradeView.Hovered += HandleGlobalUpgradeHovered;
            }
            else
            {
                upgradeView.Selected -= HandleGlobalUpgradeSelected;
                upgradeView.Hovered -= HandleGlobalUpgradeHovered;
            }
        }
    }

    private void SwitchSection(ShopSection _section)
    {
        if (_isProcessing)
            return;

        _currentSection = _section;
        ApplySectionVisibility();
        UpdateSelectedProduct();
        ShowMerchantText(_defaultMerchantSpeech);
    }

    private void ApplySectionVisibility()
    {
        bool showEquipment = _currentSection == ShopSection.Equipment;

        if (_equipmentSectionRoot != null)
            _equipmentSectionRoot.SetActive(showEquipment);

        if (_globalUpgradeSectionRoot != null)
            _globalUpgradeSectionRoot.SetActive(!showEquipment);

        if (_equipmentTabButton != null)
            _equipmentTabButton.interactable = !_isProcessing && !showEquipment;

        if (_globalUpgradesTabButton != null)
            _globalUpgradesTabButton.interactable = !_isProcessing && showEquipment;

        if (_sectionTitleText != null)
        {
            _sectionTitleText.text = showEquipment
                ? "EQUIPAMENTOS"
                : "UPGRADES";
        }
    }

    private void HandleProgressChanged()
    {
        Refresh();
    }

    private void HandleItemSelected(string _itemId)
    {
        if (_isProcessing)
            return;

        _selectedItemId = _itemId;
        _currentSection = ShopSection.Equipment;
        ApplySectionVisibility();
        UpdateSelectedEquipment();

        if (TryResolveEquipment(_itemId, out EquipmentItemDefinition item))
            ShowMerchantText(item.Description);
    }

    private void HandleItemHovered(string _itemId)
    {
        if (TryResolveEquipment(_itemId, out EquipmentItemDefinition item))
            ShowMerchantText(item.Description);
    }

    private void HandleGlobalUpgradeSelected(GlobalUpgradeType _upgradeType)
    {
        if (_isProcessing)
            return;

        _selectedGlobalUpgradeType = _upgradeType;
        _hasSelectedGlobalUpgrade = true;
        _currentSection = ShopSection.GlobalUpgrades;
        ApplySectionVisibility();
        UpdateSelectedGlobalUpgrade();

        if (TryGetGlobalUpgradeInfo(_upgradeType, out GlobalUpgradeInfo info))
            ShowMerchantText(info.Description);
    }

    private void HandleGlobalUpgradeHovered(GlobalUpgradeType _upgradeType)
    {
        if (TryGetGlobalUpgradeInfo(_upgradeType, out GlobalUpgradeInfo info))
            ShowMerchantText(info.Description);
    }

    private void HandleBuyClicked()
    {
        if (_isProcessing ||
            _currentSection != ShopSection.Equipment ||
            string.IsNullOrWhiteSpace(_selectedItemId))
        {
            return;
        }

        BeginOperation();

        try
        {
            PurchaseResult result = _progressService == null
                ? new PurchaseResult(
                    false,
                    PurchaseFailureReason.NotLoaded,
                    _selectedItemId,
                    0,
                    0,
                    false
                )
                : _progressService.TryPurchaseEquipment(_selectedItemId);
            ShowPurchaseFeedback(result);
        }
        finally
        {
            EndOperation();
        }
    }

    private void HandleEquipClicked()
    {
        if (_isProcessing ||
            _currentSection != ShopSection.Equipment ||
            string.IsNullOrWhiteSpace(_selectedItemId))
        {
            return;
        }

        BeginOperation();

        try
        {
            EquipResult result = _progressService == null
                ? new EquipResult(
                    false,
                    EquipFailureReason.NotLoaded,
                    _selectedItemId,
                    EquipmentType.Drill,
                    false,
                    false
                )
                : _progressService.TryEquipEquipment(_selectedItemId);
            ShowEquipFeedback(result);
        }
        finally
        {
            EndOperation();
        }
    }

    private void HandleUpgradeClicked()
    {
        if (_isProcessing)
            return;

        if (_currentSection == ShopSection.GlobalUpgrades)
        {
            PurchaseSelectedGlobalUpgrade();
            return;
        }

        PurchaseSelectedEquipmentUpgrade();
    }

    private void PurchaseSelectedEquipmentUpgrade()
    {
        if (string.IsNullOrWhiteSpace(_selectedItemId))
            return;

        BeginOperation();

        try
        {
            UpgradePurchaseResult result = _progressService == null
                ? new UpgradePurchaseResult(
                    false,
                    UpgradePurchaseFailureReason.ProgressNotLoaded,
                    _selectedItemId,
                    0,
                    0,
                    0,
                    0,
                    false
                )
                : _progressService.TryPurchaseEquipmentUpgrade(_selectedItemId);
            ShowUpgradeFeedback(result);
        }
        finally
        {
            EndOperation();
        }
    }

    private void PurchaseSelectedGlobalUpgrade()
    {
        if (!_hasSelectedGlobalUpgrade)
            return;

        BeginOperation();

        try
        {
            GlobalUpgradePurchaseResult result = _progressService == null
                ? new GlobalUpgradePurchaseResult(
                    false,
                    GlobalUpgradePurchaseFailureReason.ProgressNotLoaded,
                    _selectedGlobalUpgradeType,
                    0,
                    0,
                    0,
                    0,
                    false
                )
                : _progressService.TryPurchaseGlobalUpgrade(_selectedGlobalUpgradeType);
            ShowGlobalUpgradeFeedback(result);
        }
        finally
        {
            EndOperation();
        }
    }

    private void HandleStartRunClicked()
    {
        if (_isProcessing)
            return;

        if (NewRunRequested == null)
            return;

        BeginOperation();

        try
        {
            NewRunRequested.Invoke();
        }
        finally
        {
            EndOperation();
        }
    }

    private void ConfigureEquipmentViews()
    {
        if (_itemViews == null)
            return;

        int catalogCount = _equipmentCatalog == null
            ? 0
            : _equipmentCatalog.ItemCount;

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

            bool isOwned = _progressService != null &&
                           _progressService.OwnsEquipment(item.Id);
            bool isEquipped = IsEquipped(item);
            bool canAfford = _progressService != null &&
                             _progressService.TotalMoney >= item.Price;
            itemView.Configure(
                item,
                isOwned,
                isEquipped,
                canAfford,
                item.HasValidConfiguration()
            );
        }
    }

    private void ConfigureGlobalUpgradeViews()
    {
        if (_globalUpgradeViews == null)
            return;

        Array upgradeTypes = Enum.GetValues(typeof(GlobalUpgradeType));

        for (int index = 0; index < _globalUpgradeViews.Length; index++)
        {
            GlobalUpgradeShopItemView upgradeView = _globalUpgradeViews[index];

            if (upgradeView == null)
                continue;

            if (index >= upgradeTypes.Length)
            {
                upgradeView.SetVisible(false);
                continue;
            }

            GlobalUpgradeType upgradeType =
                (GlobalUpgradeType)upgradeTypes.GetValue(index);

            if (!TryGetGlobalUpgradeInfo(
                    upgradeType,
                    out GlobalUpgradeInfo info))
            {
                upgradeView.SetVisible(false);
                continue;
            }

            upgradeView.Configure(info);
        }
    }

    private void UpdateSelectedProduct()
    {
        if (_currentSection == ShopSection.GlobalUpgrades)
        {
            UpdateSelectedGlobalUpgrade();
            return;
        }

        UpdateSelectedEquipment();
    }

    private void UpdateSelectedEquipment()
    {
        EquipmentItemDefinition item = ResolveSelectedEquipment();

        if (item == null)
        {
            ClearDetails();
            return;
        }

        bool isValid = item.HasValidConfiguration();
        bool isOwned = _progressService != null &&
                       _progressService.OwnsEquipment(item.Id);
        bool isEquipped = IsEquipped(item);
        bool canAfford = _progressService != null &&
                         _progressService.TotalMoney >= item.Price;

        if (_selectedNameText != null)
            _selectedNameText.text = item.DisplayName;

        if (_selectedCategoryText != null)
            _selectedCategoryText.text = GetCategoryText(item);

        if (_selectedDescriptionText != null)
            _selectedDescriptionText.text = item.Description;

        if (_selectedPriceText != null)
        {
            _selectedPriceText.text = isOwned
                ? "POSSE"
                : $"PRECO: $ {item.Price:N0}";
        }

        if (_selectedIcon != null)
        {
            _selectedIcon.sprite = item.Icon;
            _selectedIcon.enabled = item.Icon != null;
        }

        if (_progressService != null &&
            _progressService.TryGetEquipmentUpgradeInfo(
                item.Id,
                out EquipmentUpgradeInfo info))
        {
            if (_selectedAttributesText != null)
                _selectedAttributesText.text = GetCurrentAttributesText(info);

            UpdateEquipmentUpgradeDetails(info);
        }
        else
        {
            if (_selectedAttributesText != null)
                _selectedAttributesText.text = GetBaseAttributesText(item);

            ClearUpgradeDetails();
        }

        if (_buyButton != null)
        {
            _buyButton.interactable =
                !_isProcessing && isValid && !isOwned && canAfford;
        }

        if (_equipButton != null)
        {
            _equipButton.interactable =
                !_isProcessing && isValid && isOwned && !isEquipped;
        }
    }

    private void UpdateSelectedGlobalUpgrade()
    {
        if (!_hasSelectedGlobalUpgrade)
        {
            _selectedGlobalUpgradeType = GlobalUpgradeType.SpeedLimit;
            _hasSelectedGlobalUpgrade = true;
        }

        if (!TryGetGlobalUpgradeInfo(
                _selectedGlobalUpgradeType,
                out GlobalUpgradeInfo info))
        {
            ClearDetails();
            return;
        }

        if (_selectedNameText != null)
            _selectedNameText.text = info.DisplayName;

        if (_selectedCategoryText != null)
            _selectedCategoryText.text = "UPGRADE GLOBAL";

        if (_selectedDescriptionText != null)
            _selectedDescriptionText.text = info.Description;

        if (_selectedPriceText != null)
        {
            _selectedPriceText.text = info.HasNextLevel
                ? $"PRECO: $ {info.NextLevelPrice:N0}"
                : "NIVEL MAXIMO";
        }

        if (_selectedIcon != null)
        {
            _selectedIcon.sprite = null;
            _selectedIcon.enabled = false;
        }

        if (_selectedAttributesText != null)
            _selectedAttributesText.text = GetGlobalCurrentEffectText(info);

        if (_selectedLevelText != null)
            _selectedLevelText.text =
                $"NIVEL: {info.CurrentLevel}/{info.MaximumLevel}";

        if (_selectedUpgradePreviewText != null)
        {
            _selectedUpgradePreviewText.text = info.HasNextLevel
                ? GetGlobalUpgradePreviewText(info)
                : "NIVEL MAXIMO";
        }

        if (_upgradePriceText != null)
        {
            _upgradePriceText.text = info.HasNextLevel
                ? $"PROXIMA MELHORIA: $ {info.NextLevelPrice:N0}"
                : string.Empty;
        }

        if (_upgradeStatusText != null)
        {
            _upgradeStatusText.text = GetGlobalUpgradeStatusText(info);
        }

        if (_buyButton != null)
            _buyButton.interactable = false;

        if (_equipButton != null)
            _equipButton.interactable = false;

        if (_upgradeButton != null)
        {
            _upgradeButton.interactable =
                !_isProcessing && info.CanPurchase;
        }
    }

    private EquipmentItemDefinition ResolveSelectedEquipment()
    {
        if (_equipmentCatalog == null)
            return null;

        if (TryResolveEquipment(
                _selectedItemId,
                out EquipmentItemDefinition selectedItem))
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

    private bool TryResolveEquipment(
        string _itemId,
        out EquipmentItemDefinition _item)
    {
        _item = null;

        return _equipmentCatalog != null &&
               !string.IsNullOrWhiteSpace(_itemId) &&
               _equipmentCatalog.TryGetById(_itemId, out _item);
    }

    private bool TryGetGlobalUpgradeInfo(
        GlobalUpgradeType _upgradeType,
        out GlobalUpgradeInfo _info)
    {
        _info = default;

        return _progressService != null &&
               _progressService.TryGetGlobalUpgradeInfo(
                   _upgradeType,
                   out _info
               );
    }

    private bool IsEquipped(EquipmentItemDefinition _item)
    {
        return _progressService != null &&
               _progressService.GetEquippedEquipmentId(
                   _item.EquipmentType
               ) == _item.Id;
    }

    private void UpdateMoneyText()
    {
        if (_moneyText != null)
        {
            long money = _progressService == null
                ? 0
                : _progressService.TotalMoney;
            _moneyText.text = $"DINHEIRO: $ {money:N0}";
        }
    }

    private void UpdateEquipmentUpgradeDetails(EquipmentUpgradeInfo _info)
    {
        if (_selectedLevelText != null)
        {
            _selectedLevelText.text =
                $"NIVEL: {_info.CurrentLevel}/{_info.MaximumLevel}";
        }

        if (!_info.IsOwned)
        {
            if (_selectedUpgradePreviewText != null)
            {
                _selectedUpgradePreviewText.text =
                    "COMPRE O EQUIPAMENTO PARA LIBERAR MELHORIAS";
            }

            if (_upgradePriceText != null)
                _upgradePriceText.text = string.Empty;

            if (_upgradeStatusText != null)
            {
                _upgradeStatusText.text =
                    "COMPRE O EQUIPAMENTO PARA LIBERAR MELHORIAS";
            }

            if (_upgradeButton != null)
                _upgradeButton.interactable = false;

            return;
        }

        if (!_info.HasNextLevel)
        {
            bool reachedMaximum =
                _info.CurrentLevel >= _info.MaximumLevel;

            if (_selectedUpgradePreviewText != null)
            {
                _selectedUpgradePreviewText.text = reachedMaximum
                    ? "NIVEL MAXIMO"
                    : "MELHORIA INDISPONIVEL";
            }

            if (_upgradePriceText != null)
                _upgradePriceText.text = string.Empty;

            if (_upgradeStatusText != null)
            {
                _upgradeStatusText.text = reachedMaximum
                    ? "NIVEL MAXIMO"
                    : "CONFIGURACAO DE MELHORIA INVALIDA";
            }

            if (_upgradeButton != null)
                _upgradeButton.interactable = false;

            return;
        }

        if (_selectedUpgradePreviewText != null)
        {
            _selectedUpgradePreviewText.text =
                GetEquipmentUpgradePreviewText(_info);
        }

        if (_upgradePriceText != null)
        {
            _upgradePriceText.text =
                $"PROXIMA MELHORIA: $ {_info.NextLevelPrice:N0}";
        }

        if (_upgradeStatusText != null)
        {
            _upgradeStatusText.text =
                _info.CurrentMoney < _info.NextLevelPrice
                    ? "DINHEIRO INSUFICIENTE"
                    : string.Empty;
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.interactable =
                !_isProcessing && _info.CanPurchaseUpgrade;
        }
    }

    private void ClearUpgradeDetails()
    {
        if (_selectedLevelText != null)
            _selectedLevelText.text = string.Empty;

        if (_selectedUpgradePreviewText != null)
            _selectedUpgradePreviewText.text = string.Empty;

        if (_upgradePriceText != null)
            _upgradePriceText.text = string.Empty;

        if (_upgradeStatusText != null)
            _upgradeStatusText.text = string.Empty;

        if (_upgradeButton != null)
            _upgradeButton.interactable = false;
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

        if (_selectedIcon != null)
        {
            _selectedIcon.sprite = null;
            _selectedIcon.enabled = false;
        }

        if (_selectedAttributesText != null)
            _selectedAttributesText.text = string.Empty;

        if (_buyButton != null)
            _buyButton.interactable = false;

        if (_equipButton != null)
            _equipButton.interactable = false;

        ClearUpgradeDetails();
    }

    private void BeginOperation()
    {
        _isProcessing = true;
        SetTransactionControls(false);
    }

    private void EndOperation()
    {
        _isProcessing = false;

        if (_isVisible)
            Refresh();
    }

    private void SetTransactionControls(bool _interactable)
    {
        if (_buyButton != null)
            _buyButton.interactable = _interactable;

        if (_equipButton != null)
            _equipButton.interactable = _interactable;

        if (_upgradeButton != null)
            _upgradeButton.interactable = _interactable;

        if (_equipmentTabButton != null)
            _equipmentTabButton.interactable = _interactable;

        if (_globalUpgradesTabButton != null)
            _globalUpgradesTabButton.interactable = _interactable;
    }

    private void UpdateStartRunButton()
    {
        if (_startRunButton != null)
            _startRunButton.interactable = _isVisible;
    }

    private void ShowMerchantText(string _message)
    {
        if (_merchantSpeechText == null)
            return;

        _merchantSpeechText.text = string.IsNullOrWhiteSpace(_message)
            ? _defaultMerchantSpeech
            : _message;
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

        _feedbackText.text =
            GetPurchaseFailureText(_result.FailureReason);
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

        _feedbackText.text =
            GetEquipFailureText(_result.FailureReason);
    }

    private void ShowUpgradeFeedback(UpgradePurchaseResult _result)
    {
        if (_feedbackText == null)
            return;

        if (_result.Success)
        {
            _feedbackText.text = _result.Persisted
                ? "Melhoria do equipamento comprada."
                : "Melhoria aplicada. Falha ao salvar; tente salvar novamente.";
            return;
        }

        _feedbackText.text =
            GetEquipmentUpgradeFailureText(_result.FailureReason);
    }

    private void ShowGlobalUpgradeFeedback(
        GlobalUpgradePurchaseResult _result)
    {
        if (_feedbackText == null)
            return;

        if (_result.Succeeded)
        {
            _feedbackText.text = _result.Persisted
                ? $"Upgrade comprado. Nivel {_result.NewLevel}."
                : "Upgrade aplicado. Falha ao salvar; tente salvar novamente.";
            return;
        }

        _feedbackText.text =
            GetGlobalUpgradeFailureText(_result.FailureReason);
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

    private string GetEquipmentUpgradeFailureText(
        UpgradePurchaseFailureReason _reason)
    {
        switch (_reason)
        {
            case UpgradePurchaseFailureReason.ProgressNotLoaded:
                return "Progresso ainda nao carregado.";
            case UpgradePurchaseFailureReason.NotOwned:
                return "Compre este equipamento antes de melhorar.";
            case UpgradePurchaseFailureReason.MaximumLevelReached:
                return "Este equipamento ja esta no nivel maximo.";
            case UpgradePurchaseFailureReason.InsufficientMoney:
                return "Dinheiro insuficiente.";
            case UpgradePurchaseFailureReason.InvalidPrice:
                return "Preco de melhoria invalido.";
            case UpgradePurchaseFailureReason.OperationInProgress:
                return "A melhoria ja esta sendo processada.";
            default:
                return "Melhoria indisponivel.";
        }
    }

    private string GetGlobalUpgradeFailureText(
        GlobalUpgradePurchaseFailureReason _reason)
    {
        switch (_reason)
        {
            case GlobalUpgradePurchaseFailureReason.ProgressNotLoaded:
                return "Progresso ainda nao carregado.";
            case GlobalUpgradePurchaseFailureReason.MaximumLevelReached:
                return "Este upgrade ja esta no nivel maximo.";
            case GlobalUpgradePurchaseFailureReason.InsufficientMoney:
                return "Dinheiro insuficiente.";
            case GlobalUpgradePurchaseFailureReason.InvalidPrice:
                return "Preco de upgrade invalido.";
            case GlobalUpgradePurchaseFailureReason.OperationInProgress:
                return "A compra ja esta sendo processada.";
            case GlobalUpgradePurchaseFailureReason.SaveFailed:
                return "Upgrade aplicado, mas o save falhou.";
            default:
                return "Upgrade indisponivel.";
        }
    }

    private string GetCategoryText(EquipmentItemDefinition _item)
    {
        string category = _item.EquipmentType == EquipmentType.Drill
            ? "BROCA"
            : "LANCADOR";
        return $"{category} - TIER {_item.Tier}";
    }

    private string GetCurrentAttributesText(EquipmentUpgradeInfo _info)
    {
        if (_info.EquipmentType == EquipmentType.Drill)
        {
            DrillStats stats = _info.CurrentDrillStats;
            return $"Dano: x{stats.DigDamage:0.##}" +
                   Environment.NewLine +
                   $"Velocidade: x{stats.DigSpeed:0.##}" +
                   Environment.NewLine +
                   $"Consumo: {stats.EnergyConsumption:0.##}";
        }

        return $"Forca de lancamento: x{_info.CurrentLaunchForceMultiplier:0.##}";
    }

    private string GetBaseAttributesText(EquipmentItemDefinition _item)
    {
        if (_item.EquipmentType == EquipmentType.Drill &&
            _item.DrillToolData != null)
        {
            ToolData data = _item.DrillToolData;
            return $"Dano: x{data.DigDamage:0.##}" +
                   Environment.NewLine +
                   $"Velocidade: x{data.DigSpeed:0.##}" +
                   Environment.NewLine +
                   $"Consumo: {data.EnergyConsumption:0.##}";
        }

        if (_item.EquipmentType == EquipmentType.Launcher)
            return $"Forca de lancamento: x{_item.LaunchForceMultiplier:0.##}";

        return string.Empty;
    }

    private string GetEquipmentUpgradePreviewText(
        EquipmentUpgradeInfo _info)
    {
        if (_info.EquipmentType == EquipmentType.Drill)
        {
            DrillStats current = _info.CurrentDrillStats;
            DrillStats next = _info.NextDrillStats;
            return "Proximo nivel:" +
                   Environment.NewLine +
                   $"Dano x{current.DigDamage:0.##} -> x{next.DigDamage:0.##} | " +
                   $"Velocidade x{current.DigSpeed:0.##} -> x{next.DigSpeed:0.##} | " +
                   $"Consumo {current.EnergyConsumption:0.##} -> {next.EnergyConsumption:0.##}";
        }

        return "Proximo nivel: Forca de lancamento " +
               $"x{_info.CurrentLaunchForceMultiplier:0.##} -> " +
               $"x{_info.NextLaunchForceMultiplier:0.##}";
    }

    private string GetGlobalCurrentEffectText(GlobalUpgradeInfo _info)
    {
        if (_info.UpgradeType == GlobalUpgradeType.DashCount)
            return $"Usos de dash por run: {_info.CurrentFlatBonus + 1}";

        return $"{GetGlobalEffectLabel(_info.UpgradeType)}: " +
               $"x{_info.CurrentMultiplier:0.##}";
    }

    private string GetGlobalUpgradePreviewText(GlobalUpgradeInfo _info)
    {
        if (_info.UpgradeType == GlobalUpgradeType.DashCount)
        {
            return "Proximo nivel: " +
                   $"{_info.CurrentFlatBonus + 1} -> " +
                   $"{_info.NextFlatBonus + 1} usos";
        }

        return "Proximo nivel: " +
               $"x{_info.CurrentMultiplier:0.##} -> " +
               $"x{_info.NextMultiplier:0.##}";
    }

    private string GetGlobalUpgradeStatusText(GlobalUpgradeInfo _info)
    {
        if (!_info.HasNextLevel)
            return "NIVEL MAXIMO";

        return _info.CurrentMoney < _info.NextLevelPrice
            ? "DINHEIRO INSUFICIENTE"
            : string.Empty;
    }

    private string GetGlobalEffectLabel(GlobalUpgradeType _upgradeType)
    {
        switch (_upgradeType)
        {
            case GlobalUpgradeType.SpeedLimit:
                return "Limite de velocidade";
            case GlobalUpgradeType.SteeringSpeed:
                return "Velocidade de curva";
            case GlobalUpgradeType.MaximumEnergy:
                return "Energia maxima";
            case GlobalUpgradeType.Luck:
                return "Sorte";
            case GlobalUpgradeType.MoneyMultiplier:
                return "Recompensa em dinheiro";
            default:
                return "Efeito";
        }
    }

    #endregion
}
