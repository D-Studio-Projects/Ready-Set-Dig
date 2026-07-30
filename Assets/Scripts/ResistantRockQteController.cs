using System;
using System.Collections;
using UnityEngine;

public class ResistantRockQteController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private ResistantRockQteUI _qteUI;

    [Header("QTE Balance")]
    [SerializeField]
    [Min(1)]
    private int _requiredPresses = 8;

    [SerializeField]
    [Min(.1f)]
    private float _duration = 4f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _maximumEnergyDrainRatioPerSecond = .2f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _failureSpeedMultiplier = .5f;

    [SerializeField]
    private bool _acceptLeftClick;

    [SerializeField]
    [Min(0f)]
    private float _resultDisplayDuration = .6f;

    private ResistantRockObstacle _activeRock;
    private int _currentPresses;
    private float _remainingTime;
    private Coroutine _hideResultRoutine;

    #endregion

    #region Properties

    public static ResistantRockQteController Instance { get; private set; }

    public bool IsQteActive => _activeRock != null;

    public ResistantRockObstacle ActiveRock => _activeRock;

    public int CurrentPresses => _currentPresses;

    public int RequiredPresses => Mathf.Max(1, _requiredPresses);

    public float RemainingTime => Mathf.Max(0f, _remainingTime);

    public float Duration => Mathf.Max(.1f, _duration);

    #endregion

    #region Events

    public event Action<ResistantRockObstacle> QteStarted;
    public event Action<int, int> QteProgressChanged;
    public event Action<ResistantRockObstacle> QteSucceeded;
    public event Action<ResistantRockObstacle> QteFailed;
    public event Action<ResistantRockObstacle, ObstacleResolution> QteFinished;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "More than one ResistantRockQteController is active. The newest instance will be used.",
                this
            );
        }

        Instance = this;
        ResolveReferences();

        if (_qteUI != null)
            _qteUI.Hide();
    }

    private void OnEnable()
    {
        Instance = this;
        ResolveReferences();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        CancelActiveQte(false);

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsQteActive)
            return;

        if (_runManager != null && !_runManager.IsRunning)
        {
            CancelActiveQte(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            (_acceptLeftClick && Input.GetMouseButtonDown(0)))
        {
            RegisterPress();
        }

        if (!IsQteActive)
            return;

        _remainingTime -= Time.deltaTime;
        UpdateUI();

        if (_remainingTime <= 0f)
            CompleteFailure();
    }

    #endregion

    #region Public Methods

    public bool TryStartQte(
        ResistantRockObstacle _rock,
        PlayerMovement _movement)
    {
        if (_rock == null ||
            _rock.IsResolved ||
            IsQteActive ||
            (_runManager != null && !_runManager.IsRunning))
        {
            return false;
        }

        ResolveReferences();

        if (_movement != null)
            _playerMovement = _movement;

        if (_playerMovement == null || _playerEnergy == null || !_playerEnergy.HasEnergy)
            return false;

        if (!_playerMovement.PauseForObstacle())
            return false;

        StopHideResultRoutine();
        _activeRock = _rock;
        _currentPresses = 0;
        _remainingTime = Duration;
        float energyDrainPerSecond =
            _playerEnergy.MaxEnergy *
            Mathf.Clamp01(_maximumEnergyDrainRatioPerSecond);
        _playerEnergy.BeginObstacleQteDrain(energyDrainPerSecond);

        if (_qteUI != null)
            _qteUI.Show(RequiredPresses, Duration);

        QteStarted?.Invoke(_activeRock);
        QteProgressChanged?.Invoke(_currentPresses, RequiredPresses);
        return true;
    }

    public void RegisterPress()
    {
        if (!IsQteActive)
            return;

        _currentPresses = Mathf.Min(RequiredPresses, _currentPresses + 1);
        QteProgressChanged?.Invoke(_currentPresses, RequiredPresses);
        UpdateUI();

        if (_currentPresses >= RequiredPresses)
            CompleteSuccess();
    }

    #endregion

    #region Private Methods

    private void CompleteSuccess()
    {
        ResistantRockObstacle rock = _activeRock;
        EndActiveQte(true);

        if (rock != null)
        {
            QteSucceeded?.Invoke(rock);
            QteFinished?.Invoke(rock, ObstacleResolution.QteSuccess);
            rock.Resolve(ObstacleResolution.QteSuccess);
        }

        ShowResult(true);
    }

    private void CompleteFailure()
    {
        ResistantRockObstacle rock = _activeRock;

        if (_playerMovement != null)
            _playerMovement.ApplySpeedPenalty(_failureSpeedMultiplier);

        EndActiveQte(true);

        if (rock != null)
        {
            QteFailed?.Invoke(rock);
            QteFinished?.Invoke(rock, ObstacleResolution.QteFailure);
            rock.Resolve(ObstacleResolution.QteFailure);
        }

        ShowResult(false);
    }

    private void EndActiveQte(bool _resumeMovement)
    {
        _activeRock = null;
        _currentPresses = 0;
        _remainingTime = 0f;

        if (_playerEnergy != null)
            _playerEnergy.EndObstacleQteDrain();

        if (_resumeMovement && _playerMovement != null)
            _playerMovement.ResumeFromObstacle();
    }

    private void CancelActiveQte(bool _resumeMovement)
    {
        if (!IsQteActive && (_playerEnergy == null || !_playerEnergy.IsObstacleQteActive))
            return;

        EndActiveQte(_resumeMovement);

        if (_qteUI != null)
            _qteUI.Hide();
    }

    private void ResolveReferences()
    {
        if (_runManager == null)
            _runManager = FindFirstObjectByType<RunManager>();

        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (_playerEnergy == null)
            _playerEnergy = FindFirstObjectByType<PlayerEnergy>();

        if (_qteUI == null)
            _qteUI = FindFirstObjectByType<ResistantRockQteUI>();
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();

        if (_runManager != null)
            _runManager.RunFinished += HandleRunFinished;

        if (_playerEnergy != null)
            _playerEnergy.EnergyDepleted += HandleEnergyDepleted;
    }

    private void UnsubscribeEvents()
    {
        if (_runManager != null)
            _runManager.RunFinished -= HandleRunFinished;

        if (_playerEnergy != null)
            _playerEnergy.EnergyDepleted -= HandleEnergyDepleted;
    }

    private void HandleRunFinished(RunResult _result)
    {
        CancelActiveQte(false);
    }

    private void HandleEnergyDepleted()
    {
        ResistantRockObstacle rock = _activeRock;
        CancelActiveQte(false);

        if (rock != null)
            QteFailed?.Invoke(rock);
    }

    private void UpdateUI()
    {
        if (_qteUI != null)
        {
            _qteUI.SetProgress(
                _currentPresses,
                RequiredPresses,
                RemainingTime,
                Duration
            );
        }
    }

    private void ShowResult(bool _success)
    {
        if (_qteUI == null)
            return;

        _qteUI.ShowResult(_success);
        StopHideResultRoutine();

        if (_resultDisplayDuration <= 0f)
        {
            _qteUI.Hide();
            return;
        }

        _hideResultRoutine = StartCoroutine(HideResultAfterDelay());
    }

    private IEnumerator HideResultAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, _resultDisplayDuration));

        if (_qteUI != null && !IsQteActive)
            _qteUI.Hide();

        _hideResultRoutine = null;
    }

    private void StopHideResultRoutine()
    {
        if (_hideResultRoutine == null)
            return;

        StopCoroutine(_hideResultRoutine);
        _hideResultRoutine = null;
    }

    #endregion
}
