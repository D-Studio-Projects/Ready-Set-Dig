using System;
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

    private byte[,] _map;
    private TerrainDepthProfile _depthProfile;
    private int _chunkIndex;
    private int _startingDepth;
    private int _seed;
    private int _generation;
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

    #endregion

    #region Events

    public event Action<RectInt> CellsChanged;
    public event Action Initialized;

    #endregion

    #region Unity Methods

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
        this._chunkIndex = Mathf.Max(0, _chunkIndex);
        this._width = Mathf.Max(1, _width);
        this._height = Mathf.Max(1, _height);
        this._pixelsPerUnit = Mathf.Max(1, _pixelsPerUnit);
        this._seed = _seed;
        this._depthProfile = _depthProfile;
        _startingDepth = this._chunkIndex * this._height;
        _isInitialized = false;
        _isInPool = false;
        _generation++;
        EnsureMap();

        GenerateTerrain();
        ConfigureCollider();

        _isInitialized = true;
        Initialized?.Invoke();
    }

    public void MarkTakenFromPool()
    {
        _isInPool = false;
    }

    public void ResetForPool()
    {
        _isInitialized = false;
        _isInPool = true;
        _chunkIndex = -1;
        _startingDepth = 0;
        _seed = 0;
        _depthProfile = null;
        CellsChanged = null;
        Initialized = null;

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

    public Terrain.TerrainType GetCell(int _x, int _y)
    {
        if (!InsideMap(_x, _y))
            return Terrain.TerrainType.Air;

        return (Terrain.TerrainType)_map[_x, _y];
    }

    public void SetCell(int _x, int _y, Terrain.TerrainType _value)
    {
        if (!InsideMap(_x, _y))
            return;

        if (_map[_x, _y] == (byte)_value)
            return;

        _map[_x, _y] = (byte)_value;
        CellsChanged?.Invoke(new RectInt(_x, _y, 1, 1));
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

                if (!InsideMap(x, y) || _map[x, y] == (byte)Terrain.TerrainType.Air)
                    continue;

                Terrain.TerrainType terrainType = (Terrain.TerrainType)_map[x, y];
                _map[x, y] = (byte)Terrain.TerrainType.Air;

                switch (terrainType)
                {
                    case Terrain.TerrainType.Dirt:
                        dirtCells++;
                        break;
                    case Terrain.TerrainType.Stone:
                        stoneCells++;
                        break;
                    case Terrain.TerrainType.Iron:
                        ironCells++;
                        break;
                    case Terrain.TerrainType.Gold:
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
            CellsChanged?.Invoke(new RectInt(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1
            ));
        }

        return result;
    }

    #endregion

    #region Private Methods

    private void GenerateTerrain()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int globalDepth = GetGlobalDepth(y);
                Terrain.TerrainType terrainType = _depthProfile == null
                    ? Terrain.TerrainType.Dirt
                    : _depthProfile.GetTerrainType(x, globalDepth, _seed);

                _map[x, y] = (byte)terrainType;
            }
        }
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

    #endregion
}