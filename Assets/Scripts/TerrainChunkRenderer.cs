using UnityEngine;

public class TerrainChunkRenderer : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private TerrainChunk _chunk;

    [SerializeField]
    private SpriteRenderer _targetRenderer;

    [SerializeField]
    private SpriteRenderer _backgroundRenderer;

    [Header("Colors")]
    [SerializeField]
    private Color _dirtColor = new Color(.55f, .34f, .17f);

    [SerializeField]
    private Color _backgroundColor = new Color(.18f, .10f, .05f);

    [SerializeField]
    private Color _stoneColor = Color.gray;

    [SerializeField]
    private Color _ironColor = Color.yellow;

    [SerializeField]
    private Color _goldColor = new Color(1f, .75f, 0f);

    private Texture2D _texture;
    private Sprite _sprite;
    private Texture2D _backgroundTexture;
    private Sprite _backgroundSprite;
    private int _renderedGeneration = -1;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
        RefreshChunk();
    }

    private void Start()
    {
        CacheReferences();

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

        RefreshChunk();
    }

    private void OnDestroy()
    {
        if (_targetRenderer != null)
            _targetRenderer.sprite = null;

        if (_backgroundRenderer != null)
            _backgroundRenderer.sprite = null;

        if (_sprite != null)
            Destroy(_sprite);

        if (_texture != null)
            Destroy(_texture);

        if (_backgroundSprite != null)
            Destroy(_backgroundSprite);

        if (_backgroundTexture != null)
            Destroy(_backgroundTexture);
    }

    #endregion

    #region Public Methods

    public void RefreshChunk()
    {
        CacheReferences();

        if (_chunk == null ||
            _targetRenderer == null ||
            !_chunk.IsInitialized)
        {
            return;
        }

        EnsureRendererResources();
        EnsureBackgroundRenderer();
        _targetRenderer.sprite = _sprite;
        _backgroundRenderer.sprite = _backgroundSprite;
        _targetRenderer.enabled = true;
        _backgroundRenderer.enabled = true;

        if (_renderedGeneration == _chunk.Generation)
            return;

        DrawEntireMap();
        _renderedGeneration = _chunk.Generation;
    }

    public void RedrawChangedCells(RectInt _dirtyRect)
    {
        if (_chunk == null || !_chunk.IsInitialized)
            return;

        if (_texture == null || _renderedGeneration != _chunk.Generation)
        {
            _renderedGeneration = -1;
            RefreshChunk();
            return;
        }

        RectInt clampedRect = ClampToChunk(_dirtyRect);

        if (clampedRect.width <= 0 || clampedRect.height <= 0)
            return;

        DrawCells(clampedRect);
        _texture.Apply(false);
    }

    #endregion

    #region Private Methods

    private void CacheReferences()
    {
        if (_chunk == null)
            _chunk = GetComponentInParent<TerrainChunk>();

        if (_targetRenderer == null)
            _targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void EnsureRendererResources()
    {
        EnsureBackgroundRenderer();

        if (_texture != null &&
            _texture.width == _chunk.Width &&
            _texture.height == _chunk.Height &&
            _backgroundTexture != null &&
            _backgroundTexture.width == _chunk.Width &&
            _backgroundTexture.height == _chunk.Height)
        {
            return;
        }

        EnsureBackgroundRenderer();

        if (_sprite != null)
            Destroy(_sprite);

        if (_texture != null)
            Destroy(_texture);

        if (_backgroundSprite != null)
            Destroy(_backgroundSprite);

        if (_backgroundTexture != null)
            Destroy(_backgroundTexture);

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

        _backgroundTexture = new Texture2D(
            _chunk.Width,
            _chunk.Height,
            TextureFormat.RGBA32,
            false
        )
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] backgroundPixels = new Color[_chunk.Width * _chunk.Height];

        for (int index = 0; index < backgroundPixels.Length; index++)
            backgroundPixels[index] = _backgroundColor;

        _backgroundTexture.SetPixels(backgroundPixels);
        _backgroundTexture.Apply(false);
        _backgroundSprite = Sprite.Create(
            _backgroundTexture,
            new Rect(0f, 0f, _chunk.Width, _chunk.Height),
            new Vector2(.5f, .5f),
            _chunk.PixelsPerUnit
        );
    }

    private void EnsureBackgroundRenderer()
    {
        if (_backgroundRenderer == null)
        {
            GameObject backgroundObject = new GameObject("TerrainBackground");
            backgroundObject.transform.SetParent(transform, false);
            _backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        }

        _backgroundRenderer.sharedMaterial = _targetRenderer.sharedMaterial;
        _backgroundRenderer.sortingLayerID = _targetRenderer.sortingLayerID;
        _backgroundRenderer.sortingOrder = _targetRenderer.sortingOrder - 1;
    }

    private void DrawEntireMap()
    {
        DrawCells(new RectInt(0, 0, _chunk.Width, _chunk.Height));
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

    private Color GetColor(TerrainBase.TerrainType _cell)
    {
        switch (_cell)
        {
            case TerrainBase.TerrainType.Air:
                return Color.clear;
            case TerrainBase.TerrainType.Dirt:
                return _dirtColor;
            case TerrainBase.TerrainType.Stone:
                return _stoneColor;
            case TerrainBase.TerrainType.Iron:
                return _ironColor;
            case TerrainBase.TerrainType.Gold:
                return _goldColor;
            default:
                return Color.magenta;
        }
    }

    #endregion
}
