using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    #region Fields

    [Header("Fallback Configuration")]
    [SerializeField]
    private int _width = 64;

    [SerializeField]
    private int _height = 64;

    [SerializeField]
    private int _pixelsPerUnit = 32;

    [Header("References")]
    [SerializeField]
    private BoxCollider2D _groundCollider;

    [SerializeField]
    private TerrainChunkRenderer _chunkRenderer;

    private byte[,] _map;
    private readonly List<GameObject> _spawnedObstacles = new List<GameObject>();
    private TerrainDepthProfile _depthProfile;
    private TerrainObstacleSpawnDefinition[] _obstacleDefinitions;
    private int _chunkIndex;
    private int _startingDepth;
    private int _seed;
    private int _generation;
    private float _luckMultiplier = 1f;
    private bool _isInitialized;
    private bool _isInPool = true;

    #endregion

    #region Properties

    public int Width => _width;

    public int Height => _height;

    public int PixelsPerUnit => _pixelsPerUnit;

    public int ChunkIndex => _chunkIndex;

    public int StartingDepth => _startingDepth;

    public int Seed => _seed;

    public int Generation => _generation;

    public bool IsInitialized => _isInitialized;

    public bool IsInPool => _isInPool;

    public int SpawnedObstacleCount => _spawnedObstacles.Count;

    #endregion

    #region Events

    public event Action<RectInt> CellsChanged;
    public event Action Initialized;
    public event Action<TerrainObstacle> ObstacleSpawned;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        CacheRenderer();
    }

    #endregion

    #region Public Methods

    public void Initialize(
        int _chunkIndex,
        int _width,
        int _height,
        int _pixelsPerUnit,
        int _seed,
        TerrainDepthProfile _depthProfile)
    {
        Initialize(
            _chunkIndex,
            _width,
            _height,
            _pixelsPerUnit,
            _seed,
            _depthProfile,
            1f,
            null
        );
    }

    public void Initialize(
        int _chunkIndex,
        int _width,
        int _height,
        int _pixelsPerUnit,
        int _seed,
        TerrainDepthProfile _depthProfile,
        float _luckMultiplier)
    {
        Initialize(
            _chunkIndex,
            _width,
            _height,
            _pixelsPerUnit,
            _seed,
            _depthProfile,
            _luckMultiplier,
            null
        );
    }

    public void Initialize(
        int _chunkIndex,
        int _width,
        int _height,
        int _pixelsPerUnit,
        int _seed,
        TerrainDepthProfile _depthProfile,
        float _luckMultiplier,
        TerrainObstacleSpawnDefinition[] _obstacleDefinitions)
    {
        ClearSpawnedObstacles();
        this._chunkIndex = Mathf.Max(0, _chunkIndex);
        this._width = Mathf.Max(1, _width);
        this._height = Mathf.Max(1, _height);
        this._pixelsPerUnit = Mathf.Max(1, _pixelsPerUnit);
        this._seed = _seed;
        this._depthProfile = _depthProfile;
        this._luckMultiplier = SanitizeLuckMultiplier(_luckMultiplier);
        this._obstacleDefinitions = _obstacleDefinitions;
        _startingDepth = this._chunkIndex * this._height;
        _isInitialized = false;
        _isInPool = false;
        _generation++;
        CacheRenderer();
        EnsureMap();

        GenerateTerrain();
        ConfigureCollider();
        GenerateObstacles();

        _isInitialized = true;
        Initialized?.Invoke();

        if (_chunkRenderer != null)
            _chunkRenderer.RefreshChunk();
    }

    public void MarkTakenFromPool()
    {
        _isInPool = false;
    }

    public void ResetForPool()
    {
        ClearSpawnedObstacles();
        _isInitialized = false;
        _isInPool = true;
        _chunkIndex = -1;
        _startingDepth = 0;
        _seed = 0;
        _luckMultiplier = 1f;
        _depthProfile = null;
        _obstacleDefinitions = null;
        CellsChanged = null;
        Initialized = null;
        ObstacleSpawned = null;

        if (_groundCollider != null)
            _groundCollider.enabled = false;
    }

    public bool InsideMap(int _x, int _y)
    {
        return _isInitialized &&
               _x >= 0 &&
               _y >= 0 &&
               _x < _width &&
               _y < _height;
    }

    public TerrainBase.TerrainType GetCell(int _x, int _y)
    {
        if (!InsideMap(_x, _y))
            return TerrainBase.TerrainType.Air;

        return (TerrainBase.TerrainType)_map[_x, _y];
    }

    public void SetCell(int _x, int _y, TerrainBase.TerrainType _value)
    {
        if (!InsideMap(_x, _y))
            return;

        if (_map[_x, _y] == (byte)_value)
            return;

        _map[_x, _y] = (byte)_value;
        NotifyCellsChanged(new RectInt(_x, _y, 1, 1));
    }

    public Vector2Int WorldToCell(Vector2 _worldPosition)
    {
        Vector2 localPosition = transform.InverseTransformPoint(_worldPosition);

        int x = Mathf.RoundToInt(localPosition.x * _pixelsPerUnit + _width * .5f);
        int y = Mathf.RoundToInt(localPosition.y * _pixelsPerUnit + _height * .5f);

        return new Vector2Int(x, y);
    }

    public Vector2 CellToWorld(int _x, int _y)
    {
        float localX = (_x - _width * .5f) / _pixelsPerUnit;
        float localY = (_y - _height * .5f) / _pixelsPerUnit;

        return transform.TransformPoint(new Vector2(localX, localY));
    }

    public DigResult Dig(Vector2 _worldPosition, float _radius)
    {
        if (!_isInitialized)
            return new DigResult(_worldPosition, _radius, 0, 0, 0, 0);

        Vector2Int center = WorldToCell(_worldPosition);
        int pixelRadius = Mathf.Max(0, Mathf.RoundToInt(_radius * _pixelsPerUnit));
        int dirtCells = 0;
        int stoneCells = 0;
        int ironCells = 0;
        int goldCells = 0;
        int minX = _width;
        int minY = _height;
        int maxX = -1;
        int maxY = -1;

        for (int offsetX = -pixelRadius; offsetX <= pixelRadius; offsetX++)
        {
            for (int offsetY = -pixelRadius; offsetY <= pixelRadius; offsetY++)
            {
                if (offsetX * offsetX + offsetY * offsetY > pixelRadius * pixelRadius)
                    continue;

                int x = center.x + offsetX;
                int y = center.y + offsetY;

                if (!InsideMap(x, y) || _map[x, y] == (byte)TerrainBase.TerrainType.Air)
                    continue;

                TerrainBase.TerrainType terrainType = (TerrainBase.TerrainType)_map[x, y];
                _map[x, y] = (byte)TerrainBase.TerrainType.Air;

                switch (terrainType)
                {
                    case TerrainBase.TerrainType.Dirt:
                        dirtCells++;
                        break;
                    case TerrainBase.TerrainType.Stone:
                        stoneCells++;
                        break;
                    case TerrainBase.TerrainType.Iron:
                        ironCells++;
                        break;
                    case TerrainBase.TerrainType.Gold:
                        goldCells++;
                        break;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        DigResult result = new DigResult(
            _worldPosition,
            _radius,
            dirtCells,
            stoneCells,
            ironCells,
            goldCells
        );

        if (result.HasChanges)
        {
            NotifyCellsChanged(new RectInt(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1
            ));
        }

        return result;
    }

    public void NotifyObstacleRemoved(TerrainObstacle _obstacle)
    {
        if (_obstacle == null)
            return;

        _spawnedObstacles.Remove(_obstacle.gameObject);
    }

    #endregion

    #region Private Methods

    private void CacheRenderer()
    {
        if (_chunkRenderer == null)
            _chunkRenderer = GetComponentInChildren<TerrainChunkRenderer>(true);
    }

    private void NotifyCellsChanged(RectInt _dirtyRect)
    {
        CellsChanged?.Invoke(_dirtyRect);

        if (_chunkRenderer != null)
            _chunkRenderer.RedrawChangedCells(_dirtyRect);
    }

    private void GenerateTerrain()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int globalDepth = GetGlobalDepth(y);
                TerrainBase.TerrainType terrainType = _depthProfile == null
                    ? TerrainBase.TerrainType.Dirt
                    : _depthProfile.GetTerrainType(
                        x,
                        globalDepth,
                        _seed,
                        _luckMultiplier
                    );

                _map[x, y] = (byte)terrainType;
            }
        }
    }

    private void GenerateObstacles()
    {
        if (_obstacleDefinitions == null || _obstacleDefinitions.Length == 0)
            return;

        int chunkEndDepth = _startingDepth + _height - 1;

        for (int definitionIndex = 0;
             definitionIndex < _obstacleDefinitions.Length;
             definitionIndex++)
        {
            TerrainObstacleSpawnDefinition definition =
                _obstacleDefinitions[definitionIndex];

            if (definition == null ||
                !definition.IsAvailableInDepthRange(_startingDepth, chunkEndDepth))
            {
                continue;
            }

            System.Random random = new System.Random(
                GetObstacleSeed(definitionIndex, definition.Id)
            );
            int spawnCount = definition.GetSpawnCount(random);

            for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
                SpawnObstacle(definition, random, chunkEndDepth);
        }
    }

    private void SpawnObstacle(
        TerrainObstacleSpawnDefinition _definition,
        System.Random _random,
        int _chunkEndDepth)
    {
        if (_definition == null || _definition.Prefab == null || _random == null)
            return;

        GameObject obstacleObject = Instantiate(_definition.Prefab, transform);
        obstacleObject.name =
            $"{_definition.Prefab.name}_Chunk{_chunkIndex}_{_spawnedObstacles.Count}";
        obstacleObject.transform.localPosition = GetObstacleLocalPosition(
            _definition,
            _random,
            _chunkEndDepth
        );
        obstacleObject.SetActive(true);
        _spawnedObstacles.Add(obstacleObject);

        TerrainObstacle obstacle = obstacleObject.GetComponent<TerrainObstacle>();

        if (obstacle != null)
        {
            obstacle.InitializeOwner(this);
            ObstacleSpawned?.Invoke(obstacle);
        }
    }

    private Vector3 GetObstacleLocalPosition(
        TerrainObstacleSpawnDefinition _definition,
        System.Random _random,
        int _chunkEndDepth)
    {
        float halfWidth = _width / (float)_pixelsPerUnit * .5f;
        float minimumX = -halfWidth + _definition.HorizontalPadding;
        float maximumX = halfWidth - _definition.HorizontalPadding;

        if (minimumX > maximumX)
            minimumX = maximumX = 0f;

        float localX = Mathf.Lerp(
            minimumX,
            maximumX,
            (float)_random.NextDouble()
        );
        int minimumDepth = Mathf.Max(_startingDepth, _definition.MinimumDepth);
        int maximumDepth = Mathf.Min(_chunkEndDepth, _definition.MaximumDepth);
        int obstacleDepth = minimumDepth;

        if (maximumDepth > minimumDepth)
            obstacleDepth = _random.Next(minimumDepth, maximumDepth + 1);

        int localCellY = _height - 1 - (obstacleDepth - _startingDepth);
        float halfHeight = _height / (float)_pixelsPerUnit * .5f;
        float localY = (localCellY - _height * .5f) / _pixelsPerUnit;
        float verticalPadding = Mathf.Min(
            _definition.VerticalPadding,
            halfHeight
        );
        localY = Mathf.Clamp(
            localY,
            -halfHeight + verticalPadding,
            halfHeight - verticalPadding
        );

        return new Vector3(localX, localY, _definition.LocalZ);
    }

    private int GetObstacleSeed(int _definitionIndex, string _definitionId)
    {
        unchecked
        {
            int hash = 17;
            string definitionId = _definitionId ?? string.Empty;

            for (int index = 0; index < definitionId.Length; index++)
                hash = hash * 31 + definitionId[index];

            hash = hash * 31 + _seed;
            hash = hash * 31 + _chunkIndex;
            hash = hash * 31 + _definitionIndex;
            return hash;
        }
    }

    private void ClearSpawnedObstacles()
    {
        for (int index = _spawnedObstacles.Count - 1; index >= 0; index--)
        {
            GameObject obstacle = _spawnedObstacles[index];

            if (obstacle == null)
                continue;

            obstacle.SetActive(false);
            Destroy(obstacle);
        }

        _spawnedObstacles.Clear();
    }

    private int GetGlobalDepth(int _localY)
    {
        return _startingDepth + (_height - 1 - _localY);
    }

    private void EnsureMap()
    {
        if (_map == null ||
            _map.GetLength(0) != _width ||
            _map.GetLength(1) != _height)
        {
            _map = new byte[_width, _height];
        }
    }

    private void ConfigureCollider()
    {
        if (_groundCollider == null)
            return;

        _groundCollider.size = new Vector2(
            _width / (float)_pixelsPerUnit,
            _height / (float)_pixelsPerUnit
        );
        _groundCollider.offset = Vector2.zero;
        _groundCollider.enabled = true;
    }

    private float SanitizeLuckMultiplier(float _multiplier)
    {
        if (float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
            return 1f;

        return Mathf.Max(1f, _multiplier);
    }

    #endregion
}
