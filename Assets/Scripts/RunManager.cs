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

    #endregion

    #region Properties

    public RunState CurrentState { get; private set; } = RunState.Waiting;

    public bool IsRunning => CurrentState == RunState.Running;

    #endregion

    #region Events

    public event Action RunStarted;
    public event Action RunFinished;
    public event Action<RunState> StateChanged;

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public void StartLaunch()
    {
        ChangeState(RunState.Launching);
    }

    public void StartRun()
    {
        ResetStatistics();
        ChangeState(RunState.Running);
        RunStarted?.Invoke();
    }

    public void FinishRun()
    {
        if (CurrentState == RunState.Finished)
            return;

        ChangeState(RunState.Finished);
        RunFinished?.Invoke();
    }

    public void ResetRun()
    {
        ResetStatistics();
        ChangeState(RunState.Waiting);
    }

    #endregion

    #region Private Methods

    private void ChangeState(RunState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        StateChanged?.Invoke(CurrentState);
    }

    private void ResetStatistics()
    {
        if (_runStatistics != null)
        {
            _runStatistics.Reset();
        }
    }

    #endregion
}
