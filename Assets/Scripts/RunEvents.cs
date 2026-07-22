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
        {
            _launchController.LaunchStarted += HandleLaunchStarted;
        }

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted += _playerMovement.StartMovement;
            _runManager.RunFinished += _playerMovement.StopMovement;
        }

        if (_playerEnergy != null && _runManager != null)
        {
            _playerEnergy.EnergyDepleted += _runManager.FinishRun;
        }

        if (_drill != null && _runStatistics != null)
        {
            _drill.BlocksDug += _runStatistics.AddDiggedBlocks;
        }
    }

    private void UnsubscribeEvents()
    {
        if (_launchController != null)
        {
            _launchController.LaunchStarted -= HandleLaunchStarted;
        }

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted -= _playerMovement.StartMovement;
            _runManager.RunFinished -= _playerMovement.StopMovement;
        }

        if (_playerEnergy != null && _runManager != null)
        {
            _playerEnergy.EnergyDepleted -= _runManager.FinishRun;
        }

        if (_drill != null && _runStatistics != null)
        {
            _drill.BlocksDug -= _runStatistics.AddDiggedBlocks;
        }
    }

    private void HandleLaunchStarted(float launchForce)
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetLaunchForce(launchForce);
        }

        if (_runManager != null)
        {
            _runManager.StartRun();
        }
    }

    #endregion
}
