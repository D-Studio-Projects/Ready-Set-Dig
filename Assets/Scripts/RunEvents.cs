using UnityEngine;

public class RunEvents : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private LaunchController _launchController;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private Drill _drill;

    [SerializeField]
    private RunStatistics _runStatistics;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void SubscribeEvents()
    {
        if (_launchController != null)
            _launchController.LaunchStarted += HandleLaunchStarted;

        if (_runManager != null && _runStatistics != null)
            _runManager.RunStarted += _runStatistics.BeginRun;

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted += _playerMovement.StartMovement;
            _runManager.RunFinished += HandleRunFinished;
        }

        if (_playerEnergy != null && _runManager != null)
            _playerEnergy.EnergyDepleted += HandleEnergyDepleted;

        if (_drill != null)
            _drill.DigCompleted += HandleDigCompleted;

        if (_playerMovement != null && _runStatistics != null)
        {
            _playerMovement.DistanceMovedDown += _runStatistics.AddDistance;
            _playerMovement.DashStarted += _runStatistics.IncrementDashCount;
        }
    }

    private void UnsubscribeEvents()
    {
        if (_launchController != null)
            _launchController.LaunchStarted -= HandleLaunchStarted;

        if (_runManager != null && _runStatistics != null)
            _runManager.RunStarted -= _runStatistics.BeginRun;

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted -= _playerMovement.StartMovement;
            _runManager.RunFinished -= HandleRunFinished;
        }

        if (_playerEnergy != null && _runManager != null)
            _playerEnergy.EnergyDepleted -= HandleEnergyDepleted;

        if (_drill != null)
            _drill.DigCompleted -= HandleDigCompleted;

        if (_playerMovement != null && _runStatistics != null)
        {
            _playerMovement.DistanceMovedDown -= _runStatistics.AddDistance;
            _playerMovement.DashStarted -= _runStatistics.IncrementDashCount;
        }
    }

    private void HandleLaunchStarted(float _launchForce)
    {
        if (_playerMovement != null)
            _playerMovement.SetLaunchForce(_launchForce);
    }

    private void HandleEnergyDepleted()
    {
        if (_runManager != null)
            _runManager.FinishRun(RunEndReason.EnergyDepleted);
    }

    private void HandleRunFinished(RunResult _result)
    {
        if (_playerMovement != null)
            _playerMovement.StopMovement();
    }

    private void HandleDigCompleted(DigResult _result)
    {
        if (_runStatistics != null)
            _runStatistics.AddDigResult(_result);
    }

    #endregion
}
