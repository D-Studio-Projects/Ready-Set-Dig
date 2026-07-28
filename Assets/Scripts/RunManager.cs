using System;
using UnityEngine;

public enum RunState
{
    Waiting,
    Launching,
    Running,
    Finished
}

public class RunManager : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunStatistics _runStatistics;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private PlayerMovement _playerMovement;

    private int _currentRunId;
    private PlayerEnergy _subscribedPlayerEnergy;

    #endregion

    #region Properties

    public RunState CurrentState { get; private set; } = RunState.Waiting;

    public bool IsRunning => CurrentState == RunState.Running;

    public bool IsFinished => CurrentState == RunState.Finished;

    public int CurrentRunId => _currentRunId;

    #endregion

    #region Events

    public event Action RunStarted;
    public event Action<RunResult> RunFinished;
    public event Action<RunState> StateChanged;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        BindEnergyDepletion();
    }

    private void OnDisable()
    {
        UnbindEnergyDepletion();
    }

    #endregion

    #region Public Methods

    public void StartLaunch()
    {
        if (CurrentState != RunState.Waiting)
            return;

        ChangeState(RunState.Launching);
    }

    public void StartRun()
    {
        if (CurrentState != RunState.Launching)
            return;

        BindEnergyDepletion();
        _currentRunId++;

        ChangeState(RunState.Running);
        RunStarted?.Invoke();
    }

    public void FinishRun()
    {
        FinishRun(RunEndReason.EnergyDepleted);
    }

    public void FinishRun(RunEndReason _reason)
    {
        if (CurrentState != RunState.Running)
            return;

        RunResult result = _runStatistics == null
            ? new RunResult(_currentRunId, 0f, 0f, 0, _reason)
            : _runStatistics.CreateResult(_currentRunId, _reason);

        ChangeState(RunState.Finished);

        if (_playerMovement != null)
            _playerMovement.StopMovement();

        RunFinished?.Invoke(result);
    }

    public void ResetRun()
    {
        if (CurrentState != RunState.Finished)
            return;

        ResetStatistics();
        ChangeState(RunState.Waiting);
    }

    #endregion

    #region Private Methods

    private void ChangeState(RunState _newState)
    {
        if (CurrentState == _newState)
            return;

        CurrentState = _newState;
        StateChanged?.Invoke(CurrentState);
    }

    private void ResetStatistics()
    {
        if (_runStatistics != null)
            _runStatistics.Reset();
    }

    private void ResolveReferences()
    {
        if (_playerEnergy == null)
            _playerEnergy = FindFirstObjectByType<PlayerEnergy>();

        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void BindEnergyDepletion()
    {
        ResolveReferences();

        if (_subscribedPlayerEnergy == _playerEnergy)
            return;

        UnbindEnergyDepletion();
        _subscribedPlayerEnergy = _playerEnergy;

        if (_subscribedPlayerEnergy != null)
            _subscribedPlayerEnergy.EnergyDepleted += HandleEnergyDepleted;
    }

    private void UnbindEnergyDepletion()
    {
        if (_subscribedPlayerEnergy != null)
            _subscribedPlayerEnergy.EnergyDepleted -= HandleEnergyDepleted;

        _subscribedPlayerEnergy = null;
    }

    private void HandleEnergyDepleted()
    {
        FinishRun(RunEndReason.EnergyDepleted);
    }

    #endregion
}

