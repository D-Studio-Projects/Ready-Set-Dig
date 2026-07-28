using UnityEngine;

public class RunRestartController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private RunResultUI _runResultUI;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private Transform _spawnPoint;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private PlayerHud _playerHud;

    [SerializeField]
    private LaunchController _launchController;

    [SerializeField]
    private TerrainChunkManager _terrain;

    [Header("Terrain Retry")]
    [SerializeField]
    private bool _useSameSeedOnRetry = true;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _isRestarting;

    #endregion

    #region Properties

    public bool IsRestarting => _isRestarting;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        CaptureStartTransform();
    }

    private void OnEnable()
    {
        if (_runResultUI != null)
            _runResultUI.RetryRequested += RestartRun;
    }

    private void OnDisable()
    {
        if (_runResultUI != null)
            _runResultUI.RetryRequested -= RestartRun;
    }

    #endregion

    #region Public Methods

    public void RestartRun()
    {
        if (_isRestarting || _runManager == null || !_runManager.IsFinished)
            return;

        _isRestarting = true;

        if (_runResultUI != null)
            _runResultUI.Hide();

        if (_playerMovement != null)
        {
            _playerMovement.StopMovement();
            _playerMovement.ResetMovement(_startPosition, _startRotation);
        }
        else if (_player != null)
        {
            _player.SetPositionAndRotation(_startPosition, _startRotation);
        }

        if (_playerEnergy != null)
            _playerEnergy.ResetForRun();

        if (_playerHud != null)
            _playerHud.ResetDisplay();

        if (_launchController != null)
            _launchController.ResetLaunch();

        ResetTerrain();
        _runManager.ResetRun();
        _runManager.StartLaunch();
        _isRestarting = false;
    }

    #endregion

    #region Private Methods

    private void CaptureStartTransform()
    {
        if (_spawnPoint != null)
        {
            _startPosition = _spawnPoint.position;
            _startRotation = _spawnPoint.rotation;
            return;
        }

        if (_player != null)
        {
            _startPosition = _player.position;
            _startRotation = _player.rotation;
        }
    }

    private void ResetTerrain()
    {
        if (_terrain == null)
            return;

        if (_useSameSeedOnRetry)
        {
            _terrain.ResetTerrain();
            return;
        }

        _terrain.ResetTerrain(GenerateRetrySeed());
    }

    private int GenerateRetrySeed()
    {
        int seed = Random.Range(int.MinValue + 1, int.MaxValue);

        if (_terrain != null && seed == _terrain.Seed)
            seed++;

        return seed;
    }

    #endregion
}
