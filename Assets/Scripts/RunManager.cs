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

    [SerializeField]
    private RunStatistics _runStatistics;

    private int _currentRunId;

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

    #endregion
}

