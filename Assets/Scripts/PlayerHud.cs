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
    private RunStatistics _runStatistics;

    [SerializeField]
    private RunRewardCalculator _rewardCalculator;

    [SerializeField]
    private Slider _energyBar;
    [SerializeField]
    private Text _speedText;

    [SerializeField]
    private Text _dashText;

    [SerializeField]
    private Text _depthText;

    [SerializeField]
    private Text _scoreText;

    [Header("Display")]
    [SerializeField]
    private string _speedSuffix = " m/s";
    [SerializeField]
    private string _dashFormat = "DASH: {0}/{1}";

    [SerializeField]
    private string _depthFormat = "PROFUNDIDADE: {0:0.0} m";

    [SerializeField]
    private string _scoreFormat = "SCORE: {0:N0}";

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

        UpdateRunStatistics();
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

        UpdateRunStatistics();
    }

    #endregion
    #region Private Methods

    private void UpdateEnergyBar(float _currentEnergy, float _maxEnergy)
    {
        if (_energyBar == null)
            return;

        _energyBar.maxValue = _maxEnergy;
        _energyBar.value = _currentEnergy;
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

    private void UpdateRunStatistics()
    {
        float depth = _runStatistics == null ? 0f : _runStatistics.Depth;
        int dugBlocks = _runStatistics == null ? 0 : _runStatistics.DugBlocks;
        long score = _rewardCalculator == null
            ? 0
            : _rewardCalculator.CalculateScore(dugBlocks, depth);

        if (_depthText != null)
            _depthText.text = string.Format(_depthFormat, depth);

        if (_scoreText != null)
            _scoreText.text = string.Format(_scoreFormat, score);
    }

    #endregion
}
