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

    [Header("Fallback")]
    [SerializeField]
    private string _speedSuffix = " m/s";

    [SerializeField]
    private bool _drawFallbackHud = true;

    private const float FallbackWidth = 220f;
    private const float FallbackHeight = 22f;

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

    private void OnGUI()
    {
        if (!_drawFallbackHud || _playerEnergy == null || _playerMovement == null)
            return;

        DrawFallbackEnergyBar();
        DrawFallbackSpeedText();
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        if (_energyBar == null)
            return;

        _energyBar.maxValue = maxEnergy;
        _energyBar.value = currentEnergy;
    }

    private void DrawFallbackEnergyBar()
    {
        if (_energyBar != null)
            return;

        Rect background = new Rect(20f, 20f, FallbackWidth, FallbackHeight);
        Rect fill = new Rect(20f, 20f, FallbackWidth * _playerEnergy.Normalized, FallbackHeight);

        GUI.Box(background, string.Empty);
        GUI.Box(fill, string.Empty);
        GUI.Label(new Rect(24f, 20f, FallbackWidth, FallbackHeight), $"Energy {_playerEnergy.CurrentEnergy:0}/{_playerEnergy.MaxEnergy:0}");
    }

    private void DrawFallbackSpeedText()
    {
        if (_speedText != null)
            return;

        GUI.Label(new Rect(20f, 48f, FallbackWidth, FallbackHeight), $"Speed {_playerMovement.CurrentSpeed:0.0}{_speedSuffix}");
    }

    #endregion
}
