using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TerrainChunkManager : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private TerrainChunk _chunkPrefab;

    [SerializeField]
    private Transform _chunksRoot;

    [SerializeField]
    private Transform _poolContainer;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private TerrainDepthProfile _depthProfile;

    [Header("Chunk Configuration")]
    [SerializeField]
    private int _chunkWidth = 64;

    [SerializeField]
    private int _chunkHeight = 64;

    [SerializeField]
    private int _pixelsPerUnit = 32;

    [SerializeField]
    private bool _fitWidthToCamera = true;

    [SerializeField]
    private float _horizontalMargin = 2f;

    [SerializeField]
    private float _horizontalCoverageBuffer = 4f;

    [SerializeField]
    private bool _followPlayerPathHorizontally = true;

    [SerializeField]
    private int _chunksAhead = 6;

    [SerializeField]
    private float _cameraPreloadMargin = 4f;

    [SerializeField]
    private float _unloadMargin = 4f;

    [Header("Chunk Pool")]
    [SerializeField]
    private int _initialPoolCapacity = 7;

    [SerializeField]
    private int _maxPoolSize = 10;

    [SerializeField]
    private bool _collectionCheck = true;

    [Header("World Origin")]
    [SerializeField]
    private bool _usePlayerPositionAsSurface = true;

    [SerializeField]
    private float _surfaceWorldY;

    [SerializeField]
    private float _surfaceDistanceBelowPlayer = 4f;

    [Header("Seed")]
    [SerializeField]
    private int _seed = 12345;

    private readonly Dictionary<int, TerrainChunk> _activeChunks = new Dictionary<int, TerrainChunk>();
    private readonly List<int> _chunksToRemove = new List<int>();
    private ObjectPool<TerrainChunk> _chunkPool;
    private float _worldSurfaceY;
    private float _worldCenterX;
    private float _chunkWorldHeight;
    private bool _isInitialized;
    private bool _hasLoggedPoolLimit;

    #endregion

    #region Properties

    public int ActiveChunkCount => _activeChunks.Count;

    public int PooledChunkCount => _chunkPool == null ? 0 : _chunkPool.CountAll;

    public float ChunkWorldHeight => _chunkWorldHeight;

    public int Seed => _seed;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        ConfigureChunkDimensions();
        _worldSurfaceY = _usePlayerPositionAsSurface
            ? _player.position.y - Mathf.Max(.5f, _surfaceDistanceBelowPlayer)
            : _surfaceWorldY;
        _worldCenterX = _player.position.x;
        CreateChunkPool();
        _isInitialized = true;

        EnsureChunksAroundPlayer();
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        if (_runManager != null && _runManager.IsFinished)
            return;

        RemoveChunksAboveCamera();
        EnsureChunksAroundPlayer();
        AlignUpcomingChunksWithPlayer();
    }

    private void OnDestroy()
    {
        if (_chunkPool == null)
            return;

        ReleaseAllActiveChunks();
        _chunkPool.Clear();
        _chunkPool = null;
    }

    #endregion

    #region Public Methods

    public DigResult Dig(Vector2 _worldPosition, float _radius)
    {
        DigResult result = new DigResult(_worldPosition, _radius, 0, 0, 0, 0);

        if (!_isInitialized || _radius < 0f)
            return result;

        float radius = Mathf.Max(0f, _radius);
        int minChunkIndex = GetChunkIndex(_worldPosition.y + radius);
        int maxChunkIndex = GetChunkIndex(_worldPosition.y - radius);

        for (int chunkIndex = minChunkIndex; chunkIndex <= maxChunkIndex; chunkIndex++)
        {
            TerrainChunk chunk = EnsureChunk(chunkIndex);

            if (chunk != null)
                result = result.Add(chunk.Dig(_worldPosition, radius));
        }

        return result;
    }

    public DigResult DigPath(Vector2 _startWorldPosition, Vector2 _endWorldPosition, float _radius)
    {
        float radius = Mathf.Max(0f, _radius);
        float distance = Vector2.Distance(_startWorldPosition, _endWorldPosition);
        float minimumStep = 1f / Mathf.Max(1, _pixelsPerUnit);
        float stepDistance = Mathf.Max(minimumStep, radius * .5f);
        int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / stepDistance));
        DigResult result = new DigResult(_endWorldPosition, radius, 0, 0, 0, 0);

        for (int step = 0; step <= stepCount; step++)
        {
            float progress = step / (float)stepCount;
            Vector2 samplePosition = Vector2.Lerp(
                _startWorldPosition,
                _endWorldPosition,
                progress
            );
            result = result.Add(Dig(samplePosition, radius));
        }

        return result;
    }

    public bool TryGetChunk(int _chunkIndex, out TerrainChunk _chunk)
    {
        return _activeChunks.TryGetValue(_chunkIndex, out _chunk);
    }

    public void ResetTerrain()
    {
        ResetTerrain(_seed);
    }

    public void ResetTerrain(int _newSeed)
    {
        _seed = _newSeed;

        if (!_isInitialized)
            return;

        ReleaseAllActiveChunks();
        _hasLoggedPoolLimit = false;
        _worldCenterX = _player.position.x;
        EnsureChunksAroundPlayer();
        AlignUpcomingChunksWithPlayer();
    }

    #endregion

    #region Private Methods

    private bool ValidateReferences()
    {
        if (_chunkPrefab == null)
        {
            Debug.LogError("TerrainChunkManager needs a chunk prefab reference.", this);
            return false;
        }

        if (_chunksRoot == null)
        {
            Debug.LogError("TerrainChunkManager needs a chunks root reference.", this);
            return false;
        }

        if (_poolContainer == null)
            _poolContainer = _chunksRoot;

        if (_player == null)
        {
            Debug.LogError("TerrainChunkManager needs a player reference.", this);
            return false;
        }

        if (_camera == null)
        {
            Debug.LogError("TerrainChunkManager needs a camera reference.", this);
            return false;
        }

        if (_chunkWidth <= 0 || _chunkHeight <= 0 || _pixelsPerUnit <= 0)
        {
            Debug.LogError("TerrainChunkManager chunk dimensions must be greater than zero.", this);
            return false;
        }

        _chunksAhead = Mathf.Max(0, _chunksAhead);
        _cameraPreloadMargin = Mathf.Max(0f, _cameraPreloadMargin);
        _unloadMargin = Mathf.Max(0f, _unloadMargin);
        _horizontalMargin = Mathf.Max(0f, _horizontalMargin);
        _horizontalCoverageBuffer = Mathf.Max(0f, _horizontalCoverageBuffer);
        _surfaceDistanceBelowPlayer = Mathf.Max(.5f, _surfaceDistanceBelowPlayer);
        _maxPoolSize = Mathf.Max(1, _maxPoolSize);
        _initialPoolCapacity = Mathf.Clamp(_initialPoolCapacity, 0, _maxPoolSize);

        if (_maxPoolSize < _chunksAhead + 1)
        {
            Debug.LogWarning(
                "TerrainChunkManager Max Pool Size is smaller than the configured active range.",
                this
            );
        }

        return true;
    }

    private void ConfigureChunkDimensions()
    {
        if (_fitWidthToCamera && _camera.orthographic)
        {
            float visibleWorldWidth = _camera.orthographicSize * 2f * _camera.aspect;
            float sideCoverage = _horizontalMargin + _horizontalCoverageBuffer;
            float requiredWorldWidth = visibleWorldWidth + sideCoverage * 2f;
            int requiredPixelWidth = Mathf.CeilToInt(requiredWorldWidth * _pixelsPerUnit);
            _chunkWidth = Mathf.Max(_chunkWidth, requiredPixelWidth);
        }

        _chunkWorldHeight = _chunkHeight / (float)_pixelsPerUnit;

        if (_camera.orthographic)
        {
            float retainedWorldHeight =
                _camera.orthographicSize * 2f +
                _cameraPreloadMargin +
                _unloadMargin;
            int minimumPoolSize =
                Mathf.CeilToInt(retainedWorldHeight / _chunkWorldHeight) + 2;
            _maxPoolSize = Mathf.Max(_maxPoolSize, minimumPoolSize);
            _initialPoolCapacity = Mathf.Clamp(
                Mathf.Max(_initialPoolCapacity, minimumPoolSize),
                0,
                _maxPoolSize
            );
        }
    }

    private void CreateChunkPool()
    {
        _chunkPool = new ObjectPool<TerrainChunk>(
            CreatePooledChunk,
            OnTakeChunk,
            OnReleaseChunk,
            OnDestroyChunk,
            _collectionCheck,
            _initialPoolCapacity,
            _maxPoolSize
        );

        List<TerrainChunk> prewarmedChunks = new List<TerrainChunk>(_initialPoolCapacity);

        for (int index = 0; index < _initialPoolCapacity; index++)
        {
            TerrainChunk chunk = _chunkPool.Get();

            if (chunk != null)
                prewarmedChunks.Add(chunk);
        }

        foreach (TerrainChunk chunk in prewarmedChunks)
            _chunkPool.Release(chunk);
    }

    private TerrainChunk CreatePooledChunk()
    {
        TerrainChunk chunk = Instantiate(_chunkPrefab, _poolContainer);
        chunk.name = "PooledTerrainChunk";
        chunk.gameObject.SetActive(false);
        chunk.ResetForPool();
        return chunk;
    }

    private void OnTakeChunk(TerrainChunk _chunk)
    {
        if (_chunk != null)
            _chunk.MarkTakenFromPool();
    }

    private void OnReleaseChunk(TerrainChunk _chunk)
    {
        if (_chunk == null)
            return;

        _chunk.gameObject.SetActive(false);
        _chunk.ResetForPool();
        _chunk.transform.SetParent(_poolContainer, false);
    }

    private void OnDestroyChunk(TerrainChunk _chunk)
    {
        if (_chunk != null)
            Destroy(_chunk.gameObject);
    }

    private void EnsureChunksAroundPlayer()
    {
        int playerChunkIndex = GetChunkIndex(_player.position.y);
        float cameraBottom = _camera.transform.position.y - _camera.orthographicSize;
        int cameraBottomChunkIndex = GetChunkIndex(cameraBottom - _cameraPreloadMargin);
        int lastRequiredChunkIndex = Mathf.Max(
            playerChunkIndex + _chunksAhead,
            cameraBottomChunkIndex
        );

        for (int chunkIndex = playerChunkIndex; chunkIndex <= lastRequiredChunkIndex; chunkIndex++)
            EnsureChunk(chunkIndex);
    }

    private TerrainChunk EnsureChunk(int _chunkIndex)
    {
        if (_chunkIndex < 0)
            return null;

        if (_activeChunks.TryGetValue(_chunkIndex, out TerrainChunk existingChunk))
            return existingChunk;

        if (!CanAcquireChunk())
            return null;

        TerrainChunk chunk = _chunkPool.Get();

        if (chunk == null)
            return null;

        chunk.transform.SetParent(_chunksRoot, false);
        chunk.transform.position = new Vector3(
            GetNewChunkCenterX(),
            GetChunkWorldY(_chunkIndex),
            _chunksRoot.position.z
        );
        chunk.name = $"TerrainChunk_{_chunkIndex}";
        chunk.gameObject.SetActive(true);
        chunk.Initialize(
            _chunkIndex,
            _chunkWidth,
            _chunkHeight,
            _pixelsPerUnit,
            _seed,
            _depthProfile
        );
        _activeChunks.Add(_chunkIndex, chunk);
        return chunk;
    }

    private void AlignUpcomingChunksWithPlayer()
    {
        if (!_followPlayerPathHorizontally || _player == null)
            return;

        int playerChunkIndex = GetChunkIndex(_player.position.y);
        bool playerIsAboveSurface = _player.position.y > _worldSurfaceY;

        foreach (KeyValuePair<int, TerrainChunk> pair in _activeChunks)
        {
            bool isUpcomingChunk = playerIsAboveSurface
                ? pair.Key >= playerChunkIndex
                : pair.Key > playerChunkIndex;

            if (!isUpcomingChunk || pair.Value == null)
                continue;

            Vector3 chunkPosition = pair.Value.transform.position;
            chunkPosition.x = _player.position.x;
            pair.Value.transform.position = chunkPosition;
        }
    }

    private void RemoveChunksAboveCamera()
    {
        float cameraTop = _camera.transform.position.y + _camera.orthographicSize;
        int firstChunkToKeep = GetChunkIndex(cameraTop + _unloadMargin);

        _chunksToRemove.Clear();

        foreach (KeyValuePair<int, TerrainChunk> pair in _activeChunks)
        {
            if (pair.Key < firstChunkToKeep)
                _chunksToRemove.Add(pair.Key);
        }

        foreach (int chunkIndex in _chunksToRemove)
            ReleaseChunk(chunkIndex);
    }

    private void ReleaseAllActiveChunks()
    {
        _chunksToRemove.Clear();

        foreach (int chunkIndex in _activeChunks.Keys)
            _chunksToRemove.Add(chunkIndex);

        foreach (int chunkIndex in _chunksToRemove)
            ReleaseChunk(chunkIndex);
    }

    private bool ReleaseChunk(int _chunkIndex)
    {
        if (!_activeChunks.TryGetValue(_chunkIndex, out TerrainChunk chunk))
            return false;

        _activeChunks.Remove(_chunkIndex);

        if (chunk == null || chunk.IsInPool)
            return false;

        _chunkPool.Release(chunk);
        return true;
    }

    private bool CanAcquireChunk()
    {
        if (_chunkPool == null)
            return false;

        if (_chunkPool.CountInactive > 0 || _chunkPool.CountAll < _maxPoolSize)
            return true;

        if (!_hasLoggedPoolLimit)
        {
            Debug.LogWarning(
                "TerrainChunkManager reached its maximum chunk pool size. Increase Max Pool Size if this is unexpected.",
                this
            );
            _hasLoggedPoolLimit = true;
        }

        return false;
    }

    private int GetChunkIndex(float _worldY)
    {
        if (_chunkWorldHeight <= 0f)
            return 0;

        float depth = _worldSurfaceY - _worldY;
        return Mathf.Max(0, Mathf.FloorToInt(depth / _chunkWorldHeight));
    }

    private float GetChunkWorldY(int _chunkIndex)
    {
        return _worldSurfaceY - (_chunkIndex + .5f) * _chunkWorldHeight;
    }

    private float GetNewChunkCenterX()
    {
        if (_followPlayerPathHorizontally && _player != null)
            return _player.position.x;

        return _worldCenterX;
    }

    #endregion
}
