using UnityEngine;

public class TerrainChunkRenderer : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private TerrainChunk _chunk;

    [SerializeField]
    private SpriteRenderer _targetRenderer;

    [Header("Colors")]
    [SerializeField]
    private Color _dirtColor = new Color(.35f, .22f, .12f);

    [SerializeField]
    private Color _stoneColor = Color.gray;

    [SerializeField]
    private Color _ironColor = Color.yellow;

    [SerializeField]
    private Color _goldColor = new Color(1f, .75f, 0f);

    private Texture2D _texture;
    private Sprite _sprite;
    private int _renderedGeneration = -1;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_chunk == null)
            return;

        _chunk.CellsChanged += DrawDirtyRect;
        _chunk.Initialized += HandleChunkInitialized;
        Refresh();
    }

    private void OnDisable()
    {
        if (_chunk == null)
            return;

        _chunk.CellsChanged -= DrawDirtyRect;
        _chunk.Initialized -= HandleChunkInitialized;
    }

    private void Start()
    {
        if (_chunk == null)
        {
            Debug.LogError("TerrainChunkRenderer needs a TerrainChunk reference.", this);
            enabled = false;
            return;
        }

        if (_targetRenderer == null)
        {
            Debug.LogError("TerrainChunkRenderer needs a SpriteRenderer reference.", this);
            enabled = false;
            return;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (_targetRenderer != null)
            _targetRenderer.sprite = null;

        if (_sprite != null)
            Destroy(_sprite);

        if (_texture != null)
            Destroy(_texture);
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void HandleChunkInitialized()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_chunk == null ||
            _targetRenderer == null ||
            !_chunk.IsInitialized)
        {
            return;
        }

        EnsureRendererResources();
        _targetRenderer.sprite = _sprite;

        if (_renderedGeneration == _chunk.Generation)
            return;

        DrawEntireMap();
        _renderedGeneration = _chunk.Generation;
    }

    private void EnsureRendererResources()
    {
        if (_texture != null &&
            _texture.width == _chunk.Width &&
            _texture.height == _chunk.Height)
        {
            return;
        }

        if (_sprite != null)
            Destroy(_sprite);

        if (_texture != null)
            Destroy(_texture);

        _texture = new Texture2D(
            _chunk.Width,
            _chunk.Height,
            TextureFormat.RGBA32,
            false
        )
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _sprite = Sprite.Create(
            _texture,
            new Rect(0f, 0f, _chunk.Width, _chunk.Height),
            new Vector2(.5f, .5f),
            _chunk.PixelsPerUnit
        );
    }

    private void DrawEntireMap()
    {
        DrawCells(new RectInt(0, 0, _chunk.Width, _chunk.Height));
        _texture.Apply(false);
    }

    private void DrawDirtyRect(RectInt _dirtyRect)
    {
        if (_texture == null)
            return;

        if (_renderedGeneration != _chunk.Generation)
        {
            Refresh();
            return;
        }

        RectInt clampedRect = ClampToChunk(_dirtyRect);

        if (clampedRect.width <= 0 || clampedRect.height <= 0)
            return;

        DrawCells(clampedRect);
        _texture.Apply(false);
    }

    private RectInt ClampToChunk(RectInt _rect)
    {
        int xMin = Mathf.Clamp(_rect.xMin, 0, _chunk.Width);
        int yMin = Mathf.Clamp(_rect.yMin, 0, _chunk.Height);
        int xMax = Mathf.Clamp(_rect.xMax, 0, _chunk.Width);
        int yMax = Mathf.Clamp(_rect.yMax, 0, _chunk.Height);

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void DrawCells(RectInt _rect)
    {
        for (int x = _rect.xMin; x < _rect.xMax; x++)
        {
            for (int y = _rect.yMin; y < _rect.yMax; y++)
                _texture.SetPixel(x, y, GetColor(_chunk.GetCell(x, y)));
        }
    }

    private Color GetColor(Terrain.TerrainType _cell)
    {
        switch (_cell)
        {
            case Terrain.TerrainType.Air:
                return Color.clear;
            case Terrain.TerrainType.Dirt:
                return _dirtColor;
            case Terrain.TerrainType.Stone:
                return _stoneColor;
            case Terrain.TerrainType.Iron:
                return _ironColor;
            case Terrain.TerrainType.Gold:
                return _goldColor;
            default:
                return Color.magenta;
        }
    }

    #endregion
}