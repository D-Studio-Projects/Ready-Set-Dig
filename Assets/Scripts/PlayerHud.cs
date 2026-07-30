using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private Slider _energyBar;

    [SerializeField]
    private Text _speedText;

    [SerializeField]
    private Text _dashText;

    [Header("Display")]
    [SerializeField]
    private string _speedSuffix = " m/s";

    [SerializeField]
    private string _dashFormat = "DASH: {0}/{1}";

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_playerEnergy != null)
        {
            _playerEnergy.EnergyChanged += UpdateEnergyBar;
        }

        if (_playerMovement != null)
            _playerMovement.DashCountChanged += UpdateDashText;
    }

    private void OnDisable()
    {
        if (_playerEnergy != null)
        {
            _playerEnergy.EnergyChanged -= UpdateEnergyBar;
        }

        if (_playerMovement != null)
            _playerMovement.DashCountChanged -= UpdateDashText;
    }

    private void Start()
    {
        if (_playerEnergy != null)
        {
            UpdateEnergyBar(_playerEnergy.CurrentEnergy, _playerEnergy.MaxEnergy);
        }

        if (_playerMovement != null)
        {
            UpdateDashText(
                _playerMovement.RemainingDashCount,
                _playerMovement.MaximumDashCount
            );
        }
    }

    private void Update()
    {
        if (_speedText != null && _playerMovement != null)
        {
            _speedText.text = $"{_playerMovement.CurrentSpeed:0.0}{_speedSuffix}";
        }
    }

    #endregion

    #region Public Methods

    public void ResetDisplay()
    {
        if (_playerEnergy != null)
            UpdateEnergyBar(_playerEnergy.CurrentEnergy, _playerEnergy.MaxEnergy);

        if (_speedText != null)
            _speedText.text = $"0.0{_speedSuffix}";

        if (_playerMovement != null)
        {
            UpdateDashText(
                _playerMovement.RemainingDashCount,
                _playerMovement.MaximumDashCount
            );
        }
    }

    #endregion

    #region Private Methods

    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        if (_energyBar == null)
            return;

        _energyBar.maxValue = maxEnergy;
        _energyBar.value = currentEnergy;
    }

    private void UpdateDashText(int _remainingDashes, int _maximumDashes)
    {
        if (_dashText == null)
            return;

        _dashText.text = string.Format(
            _dashFormat,
            Mathf.Max(0, _remainingDashes),
            Mathf.Max(0, _maximumDashes)
        );
    }

    #endregion
}
