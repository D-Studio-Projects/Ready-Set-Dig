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

    [SerializeField]
    private TerrainChunkManager _terrain;

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

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted += _playerMovement.StartMovement;
            _runManager.RunFinished += _playerMovement.StopMovement;
        }

        if (_runManager != null && _terrain != null)
            _runManager.RunStarted += _terrain.ResetTerrain;

        if (_playerEnergy != null && _runManager != null)
            _playerEnergy.EnergyDepleted += _runManager.FinishRun;

        if (_drill != null)
            _drill.DigCompleted += HandleDigCompleted;

        if (_playerMovement != null && _runStatistics != null)
            _playerMovement.DistanceMovedDown += _runStatistics.AddDistance;
    }

    private void UnsubscribeEvents()
    {
        if (_launchController != null)
            _launchController.LaunchStarted -= HandleLaunchStarted;

        if (_runManager != null && _playerMovement != null)
        {
            _runManager.RunStarted -= _playerMovement.StartMovement;
            _runManager.RunFinished -= _playerMovement.StopMovement;
        }

        if (_runManager != null && _terrain != null)
            _runManager.RunStarted -= _terrain.ResetTerrain;

        if (_playerEnergy != null && _runManager != null)
            _playerEnergy.EnergyDepleted -= _runManager.FinishRun;

        if (_drill != null)
            _drill.DigCompleted -= HandleDigCompleted;

        if (_playerMovement != null && _runStatistics != null)
            _playerMovement.DistanceMovedDown -= _runStatistics.AddDistance;
    }

    private void HandleLaunchStarted(float _launchForce)
    {
        if (_playerMovement != null)
            _playerMovement.SetLaunchForce(_launchForce);

        if (_runManager != null)
            _runManager.StartRun();
    }

    private void HandleDigCompleted(DigResult _result)
    {
        if (_runStatistics != null)
            _runStatistics.AddDiggedBlocks(_result.TotalCells);
    }

    #endregion
}