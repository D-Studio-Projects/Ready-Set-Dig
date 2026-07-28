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

    [Header("Display")]
    [SerializeField]
    private string _speedSuffix = " m/s";

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
    }

    private void OnDisable()
    {
        if (_playerEnergy != null)
        {
            _playerEnergy.EnergyChanged -= UpdateEnergyBar;
        }
    }

    private void Start()
    {
        if (_playerEnergy != null)
        {
            UpdateEnergyBar(_playerEnergy.CurrentEnergy, _playerEnergy.MaxEnergy);
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

    #endregion
}