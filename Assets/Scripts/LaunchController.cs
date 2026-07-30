using System;
using System.Collections.Generic;
using UnityEngine;

public class LaunchController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private LauncherData _fallbackLauncherData;

    [SerializeField]
    private MonoBehaviour[] _minigameBehaviors;

    private readonly Dictionary<LauncherBehaviorId, ILaunchMinigame> _minigames =
        new Dictionary<LauncherBehaviorId, ILaunchMinigame>();

    private readonly HashSet<LauncherBehaviorId> _fallbackWarnings =
        new HashSet<LauncherBehaviorId>();

    private LauncherData _activeLauncherData;
    private ILaunchMinigame _activeMinigame;
    private int _activeLauncherLevel;
    private bool _isUsingFallbackMinigame;
    private bool _hasLaunched;
    private bool _isInitialized;

    #endregion

    #region Properties

    public float CurrentCharge => _activeMinigame == null ? 0f : _activeMinigame.CurrentResult;

    public bool HasLaunched => _hasLaunched;

    public LauncherBehaviorId RequestedBehaviorId =>
        _activeLauncherData == null
            ? LauncherBehaviorId.PowerBar
            : _activeLauncherData.BehaviorId;

    public LauncherBehaviorId ActiveBehaviorId =>
        _activeMinigame == null
            ? LauncherBehaviorId.PowerBar
            : _activeMinigame.BehaviorId;

    public bool IsUsingFallbackMinigame => _isUsingFallbackMinigame;

    public float ConfiguredLaunchForceMultiplier =>
        _activeLauncherData == null
            ? 1f
            : _activeLauncherData.GetForceMultiplier(_activeLauncherLevel);

    #endregion

    #region Events

    public event Action<float> LaunchStarted;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        RegisterMinigames();
        _isInitialized = true;
    }

    private void Start()
    {
        ResetLaunch();

        if (_runManager == null)
        {
            Debug.LogError("LaunchController needs a RunManager reference.", this);
            return;
        }

        _runManager.StartLaunch();
    }

    private void Update()
    {
        if (_hasLaunched ||
            _runManager == null ||
            _runManager.CurrentState != RunState.Launching ||
            _activeMinigame == null)
        {
            return;
        }

        _activeMinigame.Tick(UnityEngine.Time.deltaTime);

        if (_activeMinigame.TryComplete(out float minigameResult))
            CompleteLaunch(minigameResult);
    }

    #endregion

    #region Public Methods

    public void ApplyEquipment(LauncherData _launcherData, int _level)
    {
        if (_launcherData == null || !_launcherData.HasValidConfiguration())
        {
            ResetEquipment();
            return;
        }

        _activeLauncherData = _launcherData;
        _activeLauncherLevel = Mathf.Max(0, _level);

        if (_isInitialized)
            PrepareActiveMinigame();
    }

    public void ResetEquipment()
    {
        _activeLauncherData =
            _fallbackLauncherData != null && _fallbackLauncherData.HasValidConfiguration()
                ? _fallbackLauncherData
                : null;
        _activeLauncherLevel = 0;

        if (_isInitialized)
            PrepareActiveMinigame();
    }

    public void ResetLaunch()
    {
        _hasLaunched = false;

        if (_activeLauncherData == null || !_activeLauncherData.HasValidConfiguration())
            ResetEquipment();
        else
            PrepareActiveMinigame();
    }

    #endregion

    #region Private Methods

    private void RegisterMinigames()
    {
        _minigames.Clear();
        _fallbackWarnings.Clear();

        if (_minigameBehaviors == null)
            return;

        foreach (MonoBehaviour behavior in _minigameBehaviors)
        {
            if (!(behavior is ILaunchMinigame minigame))
            {
                if (behavior != null)
                    Debug.LogError($"{behavior.name} does not implement ILaunchMinigame.", behavior);

                continue;
            }

            if (_minigames.ContainsKey(minigame.BehaviorId))
            {
                Debug.LogError(
                    $"LaunchController has more than one minigame for {minigame.BehaviorId}.",
                    this
                );
                continue;
            }

            _minigames.Add(minigame.BehaviorId, minigame);
        }
    }

    private void PrepareActiveMinigame()
    {
        _activeMinigame = null;
        _isUsingFallbackMinigame = false;

        if (_activeLauncherData == null)
            return;

        LauncherBehaviorId requestedBehavior = _activeLauncherData.BehaviorId;

        if (_minigames.TryGetValue(
                requestedBehavior,
                out ILaunchMinigame configuredMinigame))
        {
            ActivateMinigame(configuredMinigame);
            return;
        }

        if (!_minigames.TryGetValue(
                LauncherBehaviorId.PowerBar,
                out ILaunchMinigame fallbackMinigame))
        {
            Debug.LogError(
                $"No launch minigame is configured for {requestedBehavior}, and the PowerBar fallback is unavailable.",
                this
            );
            return;
        }

        _isUsingFallbackMinigame = true;

        if (requestedBehavior != LauncherBehaviorId.PowerBar &&
            _fallbackWarnings.Add(requestedBehavior))
        {
            Debug.LogWarning(
                $"{requestedBehavior} has no minigame yet. LaunchController will temporarily use PowerBar.",
                this
            );
        }

        ActivateMinigame(fallbackMinigame);
    }

    private void ActivateMinigame(ILaunchMinigame _minigame)
    {
        _activeMinigame = _minigame;
        _activeMinigame.ResetMinigame();
    }

    private void CompleteLaunch(float _minigameResult)
    {
        if (_hasLaunched ||
            _runManager == null ||
            _runManager.CurrentState != RunState.Launching)
        {
            return;
        }

        _hasLaunched = true;
        float launchForce = LauncherForceCalculator.Calculate(
            _minigameResult,
            _activeLauncherData,
            _activeLauncherLevel
        );

        LaunchStarted?.Invoke(launchForce);
        _runManager.StartRun();

        if (!_runManager.IsRunning)
        {
            Debug.LogError(
                "LaunchController could not start the run. Check its RunManager reference.",
                this
            );
        }
    }

    #endregion
}
