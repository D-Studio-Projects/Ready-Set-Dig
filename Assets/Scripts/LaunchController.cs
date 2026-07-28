using System;
using UnityEngine;
using UnityEngine.UI;

public class LaunchController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private Slider _chargeBar;

    [Header("Charge")]
    [SerializeField]
    private float _chargeSpeed = 2f;

    [SerializeField]
    private bool _hideChargeBarOnLaunch = true;

    private float _charge;
    private float _chargeDirection = 1f;
    private float _configuredLaunchForceMultiplier = 1f;
    private bool _hasLaunched;

    #endregion

    #region Properties

    public float CurrentCharge => _charge;

    public bool HasLaunched => _hasLaunched;

    public float ConfiguredLaunchForceMultiplier => _configuredLaunchForceMultiplier;

    #endregion

    #region Events

    public event Action<float> LaunchStarted;

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (_runManager != null)
            _runManager.StartLaunch();
    }

    private void Update()
    {
        if (_hasLaunched ||
            _runManager == null ||
            _runManager.CurrentState != RunState.Launching)
        {
            return;
        }

        UpdateChargeBar();
        HandleLaunchInput();
    }

    #endregion

    #region Public Methods

    public void ApplyEquipment(float _launchForceMultiplier)
    {
        if (float.IsNaN(_launchForceMultiplier) || float.IsInfinity(_launchForceMultiplier))
            _launchForceMultiplier = 1f;

        _configuredLaunchForceMultiplier = Mathf.Max(.01f, _launchForceMultiplier);
    }

    public void ResetEquipment()
    {
        _configuredLaunchForceMultiplier = 1f;
    }

    public void ResetLaunch()
    {
        _hasLaunched = false;
        _charge = 0f;
        _chargeDirection = 1f;

        if (_chargeBar != null)
        {
            _chargeBar.value = _charge;

            if (_hideChargeBarOnLaunch)
                _chargeBar.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Private Methods

    private void UpdateChargeBar()
    {
        float chargeSpeed = Mathf.Max(.01f, _chargeSpeed);
        _charge += _chargeDirection * chargeSpeed * UnityEngine.Time.deltaTime;

        if (_charge >= 1f)
        {
            _charge = 1f;
            _chargeDirection = -1f;
        }
        else if (_charge <= 0f)
        {
            _charge = 0f;
            _chargeDirection = 1f;
        }

        if (_chargeBar != null)
            _chargeBar.value = _charge;
    }

    private void HandleLaunchInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        StartLaunch();
    }

    private void StartLaunch()
    {
        _hasLaunched = true;
        float launchForce = Mathf.Clamp01(_charge * _configuredLaunchForceMultiplier);
        LaunchStarted?.Invoke(launchForce);

        if (_hideChargeBarOnLaunch && _chargeBar != null)
            _chargeBar.gameObject.SetActive(false);
    }

    #endregion
}
