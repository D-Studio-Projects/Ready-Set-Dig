using UnityEngine;

public class MineralSpawner : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private RunStatistics _runStatistics;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private MineralCatalog _catalog;

    [SerializeField]
    private MineralPickup[] _pickupPool;

    [Header("Spawn Area")]
    [SerializeField]
    [Min(.1f)]
    private float _initialSpawnOffset = 8f;

    [SerializeField]
    [Min(1f)]
    private float _spawnAheadDistance = 80f;

    [SerializeField]
    [Min(0f)]
    private float _despawnBehindDistance = 14f;

    [SerializeField]
    [Min(.1f)]
    private float _horizontalHalfWidth = 3.25f;

    [SerializeField]
    private float _spawnZ;

    [Header("Distribution")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _spawnChance = .55f;

    [SerializeField]
    [Min(.1f)]
    private float _minimumVerticalSpacing = 4f;

    [SerializeField]
    [Min(.1f)]
    private float _maximumVerticalSpacing = 7f;

    [SerializeField]
    private int _baseSeed = 41717;

    [Header("Diagnostics")]
    [SerializeField]
    private bool _logConfigurationErrors = true;

    private System.Random _random;
    private RunManager _subscribedRunManager;
    private float _runStartY;
    private float _centerX;
    private float _nextSpawnY;
    private float _luckMultiplier = 1f;
    private bool _isSpawning;
    private string _lastConfigurationError;

    #endregion

    #region Properties

    public float LuckMultiplier => _luckMultiplier;

    public int ActivePickupCount => CountActivePickups();

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ResolveReferences();
        PreparePool();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BindRunManager();
    }

    private void Start()
    {
        ResolveReferences();
        BindRunManager();

        if (_runManager != null && _runManager.IsRunning)
            BeginRun();
    }

    private void Update()
    {
        if (!_isSpawning ||
            _runManager == null ||
            !_runManager.IsRunning ||
            _player == null)
        {
            return;
        }

        RecyclePickupsBehindPlayer();
        FillSpawnArea();
    }

    private void OnDisable()
    {
        UnbindRunManager();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _initialSpawnOffset = Mathf.Max(.1f, _initialSpawnOffset);
        _spawnAheadDistance = Mathf.Max(1f, _spawnAheadDistance);
        _despawnBehindDistance = Mathf.Max(0f, _despawnBehindDistance);
        _horizontalHalfWidth = Mathf.Max(.1f, _horizontalHalfWidth);
        _minimumVerticalSpacing = Mathf.Max(.1f, _minimumVerticalSpacing);
        _maximumVerticalSpacing = Mathf.Max(
            _minimumVerticalSpacing,
            _maximumVerticalSpacing
        );
    }
#endif

    #endregion

    #region Public Methods

    public void ApplyLuckMultiplier(float _multiplier)
    {
        _luckMultiplier = SanitizeLuckMultiplier(_multiplier);
    }

    public void ResetSpawnedMinerals()
    {
        _isSpawning = false;
        DeactivateAllPickups();
    }

    #endregion

    #region Private Methods

    private void ResolveReferences()
    {
        if (_runManager == null)
            _runManager = FindFirstObjectByType<RunManager>();

        if (_runStatistics == null)
            _runStatistics = FindFirstObjectByType<RunStatistics>();

        if (_player == null)
        {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();

            if (movement != null)
                _player = movement.transform;
        }
    }

    private void BindRunManager()
    {
        if (_subscribedRunManager == _runManager)
            return;

        UnbindRunManager();
        _subscribedRunManager = _runManager;

        if (_subscribedRunManager == null)
            return;

        _subscribedRunManager.RunStarted += BeginRun;
        _subscribedRunManager.RunFinished += HandleRunFinished;
    }

    private void UnbindRunManager()
    {
        if (_subscribedRunManager != null)
        {
            _subscribedRunManager.RunStarted -= BeginRun;
            _subscribedRunManager.RunFinished -= HandleRunFinished;
        }

        _subscribedRunManager = null;
    }

    private void BeginRun()
    {
        ResolveReferences();
        PreparePool();

        string configurationError = GetConfigurationError();

        if (!string.IsNullOrEmpty(configurationError))
        {
            _isSpawning = false;
            LogConfigurationError(configurationError);
            return;
        }

        _lastConfigurationError = null;
        DeactivateAllPickups();
        _runStartY = _player.position.y;
        _centerX = _player.position.x;
        _nextSpawnY = _runStartY - Mathf.Max(.1f, _initialSpawnOffset);
        _random = new System.Random(CreateRunSeed());
        _isSpawning = true;
        FillSpawnArea();
    }

    private void HandleRunFinished(RunResult _result)
    {
        ResetSpawnedMinerals();
    }

    private void PreparePool()
    {
        if (_pickupPool == null)
            return;

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup == null)
                continue;

            pickup.Initialize(_runStatistics, _player);
            pickup.Deactivate();
        }
    }

    private void FillSpawnArea()
    {
        if (_player == null || _random == null)
            return;

        float minimumAhead = Mathf.Max(.1f, _initialSpawnOffset);
        _nextSpawnY = Mathf.Min(_nextSpawnY, _player.position.y - minimumAhead);
        float lowestSpawnY = _player.position.y - Mathf.Max(
            minimumAhead,
            _spawnAheadDistance
        );
        int safetyLimit = Mathf.Max(16, (_pickupPool == null ? 0 : _pickupPool.Length) * 4);

        while (_nextSpawnY >= lowestSpawnY && safetyLimit > 0)
        {
            safetyLimit--;

            if (ShouldSpawn())
            {
                MineralPickup pickup = GetAvailablePickup();

                if (pickup == null)
                    return;

                float depth = Mathf.Max(0f, _runStartY - _nextSpawnY);
                MineralData mineral = SelectMineral(depth);

                if (mineral != null)
                {
                    float x = _centerX + RandomRange(
                        -Mathf.Max(.1f, _horizontalHalfWidth),
                        Mathf.Max(.1f, _horizontalHalfWidth)
                    );
                    pickup.Activate(
                        mineral,
                        new Vector3(x, _nextSpawnY, _spawnZ)
                    );
                }
            }

            _nextSpawnY -= RandomRange(
                Mathf.Max(.1f, _minimumVerticalSpacing),
                Mathf.Max(_minimumVerticalSpacing, _maximumVerticalSpacing)
            );
        }
    }

    private MineralData SelectMineral(float _depth)
    {
        if (_catalog == null || _random == null)
            return null;

        float totalWeight = 0f;

        for (int index = 0; index < _catalog.Count; index++)
        {
            MineralData mineral = _catalog.GetAt(index);

            if (mineral != null && mineral.IsAvailableAtDepth(_depth))
                totalWeight += mineral.SpawnWeight;
        }

        if (totalWeight <= 0f)
            return null;

        double choice = _random.NextDouble() * totalWeight;

        for (int index = 0; index < _catalog.Count; index++)
        {
            MineralData mineral = _catalog.GetAt(index);

            if (mineral == null || !mineral.IsAvailableAtDepth(_depth))
                continue;

            choice -= mineral.SpawnWeight;

            if (choice <= 0d)
                return mineral;
        }

        return null;
    }

    private bool ShouldSpawn()
    {
        if (_random == null)
            return false;

        float chance = Mathf.Clamp01(_spawnChance * _luckMultiplier);
        return _random.NextDouble() <= chance;
    }

    private MineralPickup GetAvailablePickup()
    {
        if (_pickupPool == null)
            return null;

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup != null && !pickup.gameObject.activeSelf)
                return pickup;
        }

        return null;
    }

    private void RecyclePickupsBehindPlayer()
    {
        if (_pickupPool == null || _player == null)
            return;

        float highestAllowedY = _player.position.y + Mathf.Max(
            0f,
            _despawnBehindDistance
        );

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup != null &&
                pickup.gameObject.activeSelf &&
                pickup.transform.position.y > highestAllowedY)
            {
                pickup.Deactivate();
            }
        }
    }

    private void DeactivateAllPickups()
    {
        if (_pickupPool == null)
            return;

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup != null)
                pickup.Deactivate();
        }
    }

    private int CountActivePickups()
    {
        if (_pickupPool == null)
            return 0;

        int count = 0;

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup != null && pickup.gameObject.activeSelf)
                count++;
        }

        return count;
    }

    private int CreateRunSeed()
    {
        int runId = _runManager == null ? 0 : _runManager.CurrentRunId;

        unchecked
        {
            return _baseSeed * 397 ^ runId;
        }
    }

    private float RandomRange(float _minimum, float _maximum)
    {
        if (_random == null || _maximum <= _minimum)
            return _minimum;

        return _minimum + (float)_random.NextDouble() * (_maximum - _minimum);
    }

    private string GetConfigurationError()
    {
        if (_runManager == null)
            return "RunManager não foi encontrado.";

        if (_player == null)
            return "o Transform do jogador não foi encontrado.";

        if (_runStatistics == null)
            return "RunStatistics não foi encontrado.";

        if (_catalog == null)
            return "o MineralCatalog não está atribuído.";

        if (!_catalog.HasAnyValidMineral())
        {
            return "o MineralCatalog não possui nenhum MineralData válido. " +
                   "Preencha ID, nome, peso maior que zero e profundidades válidas.";
        }

        if (_pickupPool == null || _pickupPool.Length == 0)
            return "o pool de MineralPickup está vazio.";

        foreach (MineralPickup pickup in _pickupPool)
        {
            if (pickup != null)
                return null;
        }

        return "todas as posições do pool de MineralPickup estão vazias.";
    }

    private void LogConfigurationError(string _message)
    {
        if (!_logConfigurationErrors || _lastConfigurationError == _message)
            return;

        _lastConfigurationError = _message;
        Debug.LogError($"MineralSpawner não iniciou: {_message}", this);
    }

    private float SanitizeLuckMultiplier(float _multiplier)
    {
        if (float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
            return 1f;

        return Mathf.Max(1f, _multiplier);
    }

#if UNITY_EDITOR
    [ContextMenu("Development/Validate Mineral Spawner")]
    private void ValidateMineralSpawner()
    {
        ResolveReferences();
        PreparePool();
        string configurationError = GetConfigurationError();

        if (string.IsNullOrEmpty(configurationError))
        {
            Debug.Log(
                $"MineralSpawner válido. Catálogo: {_catalog.ValidCount} minérios válidos; " +
                $"pool: {_pickupPool.Length} itens.",
                this
            );
            return;
        }

        Debug.LogError($"MineralSpawner inválido: {configurationError}", this);
    }
#endif

    #endregion
}
