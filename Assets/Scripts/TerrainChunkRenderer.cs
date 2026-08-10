using System.Collections.Generic;
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

    [SerializeField]
    private TerrainVisualPalette _visualPalette;

    [Header("Fallback Colors")]
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

    private static readonly Dictionary<Texture2D, Color32[]> _spritePixels =
        new Dictionary<Texture2D, Color32[]>();
    private static readonly HashSet<Texture2D> _unreadableTextures =
        new HashSet<Texture2D>();

    private readonly Dictionary<Vector3Int, Sprite> _visualSprites =
        new Dictionary<Vector3Int, Sprite>();
    private Texture2D _texture;
    private Sprite _sprite;
    private Texture2D _backgroundTexture;
    private Sprite _backgroundSprite;
    private Color32[] _texturePixels;
    private int _renderedGeneration = -1;
    private int _renderScale = 1;
    private bool _hasPendingTextureUpload;

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

        if (_backgroundRenderer == null)
        {
            Debug.LogError(
                "TerrainChunkRenderer needs a background SpriteRenderer reference.",
                this
            );
            enabled = false;
            return;
        }

        RefreshChunk();
    }

    private void LateUpdate()
    {
        if (_hasPendingTextureUpload)
            UploadTexture();
    }

    private void OnDisable()
    {
        _hasPendingTextureUpload = false;
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

        if (!ConfigureBackgroundRenderer())
            return;

        EnsureRendererResources();
        _targetRenderer.sprite = _sprite;
        _backgroundRenderer.sprite = _backgroundSprite;
        _targetRenderer.enabled = true;
        _backgroundRenderer.enabled = true;

        if (_renderedGeneration == _chunk.Generation)
            return;

        _visualSprites.Clear();
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
        _hasPendingTextureUpload = true;
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
        _renderScale = GetEffectiveRenderScale();
        int textureWidth = _chunk.Width * _renderScale;
        int textureHeight = _chunk.Height * _renderScale;

        if (_texture != null &&
            _texture.width == textureWidth &&
            _texture.height == textureHeight &&
            _backgroundTexture != null &&
            _backgroundTexture.width == _chunk.Width &&
            _backgroundTexture.height == _chunk.Height)
        {
            EnsureTexturePixelBuffer(textureWidth, textureHeight);
            return;
        }

        if (_sprite != null)
            Destroy(_sprite);

        if (_texture != null)
            Destroy(_texture);

        if (_backgroundSprite != null)
            Destroy(_backgroundSprite);

        if (_backgroundTexture != null)
            Destroy(_backgroundTexture);

        _texture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        )
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _texturePixels = new Color32[textureWidth * textureHeight];

        _sprite = Sprite.Create(
            _texture,
            new Rect(0f, 0f, textureWidth, textureHeight),
            new Vector2(.5f, .5f),
            _chunk.PixelsPerUnit * _renderScale
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

        Color32[] backgroundPixels = new Color32[_chunk.Width * _chunk.Height];

        for (int index = 0; index < backgroundPixels.Length; index++)
            backgroundPixels[index] = _backgroundColor;

        _backgroundTexture.SetPixels32(backgroundPixels);
        _backgroundTexture.Apply(false, false);
        _backgroundSprite = Sprite.Create(
            _backgroundTexture,
            new Rect(0f, 0f, _chunk.Width, _chunk.Height),
            new Vector2(.5f, .5f),
            _chunk.PixelsPerUnit
        );
    }

    private bool ConfigureBackgroundRenderer()
    {
        if (_backgroundRenderer == null)
        {
            Debug.LogError(
                "TerrainChunkRenderer needs a background SpriteRenderer reference.",
                this
            );
            enabled = false;
            return false;
        }

        _backgroundRenderer.sharedMaterial = _targetRenderer.sharedMaterial;
        _backgroundRenderer.sortingLayerID = _targetRenderer.sortingLayerID;
        _backgroundRenderer.sortingOrder = _targetRenderer.sortingOrder - 1;
        return true;
    }

    private void DrawEntireMap()
    {
        DrawCells(new RectInt(0, 0, _chunk.Width, _chunk.Height));
        UploadTexture();
    }

    private void EnsureTexturePixelBuffer(int _width, int _height)
    {
        int requiredLength = Mathf.Max(1, _width) * Mathf.Max(1, _height);

        if (_texturePixels == null || _texturePixels.Length != requiredLength)
            _texturePixels = new Color32[requiredLength];
    }

    private void UploadTexture()
    {
        if (_texture == null || _texturePixels == null)
            return;

        _texture.SetPixels32(_texturePixels);
        _texture.Apply(false, false);
        _hasPendingTextureUpload = false;
    }

    private void WriteTexturePixel(int _x, int _y, Color32 _color)
    {
        if (_texture == null ||
            _texturePixels == null ||
            _x < 0 ||
            _y < 0 ||
            _x >= _texture.width ||
            _y >= _texture.height)
        {
            return;
        }

        _texturePixels[_y * _texture.width + _x] = _color;
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
                DrawCell(x, y);
        }
    }

    private void DrawCell(int _x, int _y)
    {
        TerrainBase.TerrainType terrainType = _chunk.GetCell(_x, _y);
        int textureX = _x * _renderScale;
        int textureY = _y * _renderScale;

        if (terrainType == TerrainBase.TerrainType.Air)
        {
            FillCell(textureX, textureY, Color.clear);
            return;
        }

        Sprite visualSprite = GetVisualSprite(terrainType, _x, _y);

        if (visualSprite == null ||
            !TryGetSpritePixels(
                visualSprite,
                out Color32[] pixels,
                out RectInt spriteRect,
                out int textureWidth))
        {
            FillCell(textureX, textureY, GetFallbackColor(terrainType));
            return;
        }

        int tileSize = _visualPalette.TileSizeInCells;
        int globalDepth = _chunk.GetGlobalDepth(_y);
        int cellXInTile = PositiveModulo(_x, tileSize);
        int cellDepthInTile = PositiveModulo(globalDepth, tileSize);
        int cellYInTile = tileSize - 1 - cellDepthInTile;
        int destinationTileSize = tileSize * _renderScale;

        for (int subX = 0; subX < _renderScale; subX++)
        {
            for (int subY = 0; subY < _renderScale; subY++)
            {
                float normalizedX =
                    (cellXInTile * _renderScale + subX + .5f) /
                    destinationTileSize;
                float normalizedY =
                    (cellYInTile * _renderScale + subY + .5f) /
                    destinationTileSize;
                int sourceX = spriteRect.x + Mathf.Clamp(
                    Mathf.FloorToInt(normalizedX * spriteRect.width),
                    0,
                    spriteRect.width - 1
                );
                int sourceY = spriteRect.y + Mathf.Clamp(
                    Mathf.FloorToInt(normalizedY * spriteRect.height),
                    0,
                    spriteRect.height - 1
                );
                Color32 color = pixels[sourceY * textureWidth + sourceX];
                WriteTexturePixel(
                    textureX + subX,
                    textureY + subY,
                    color
                );
            }
        }
    }

    private void FillCell(int _textureX, int _textureY, Color _color)
    {
        Color32 color = _color;

        for (int subX = 0; subX < _renderScale; subX++)
        {
            for (int subY = 0; subY < _renderScale; subY++)
            {
                WriteTexturePixel(
                    _textureX + subX,
                    _textureY + subY,
                    color
                );
            }
        }
    }

    private Sprite GetVisualSprite(
        TerrainBase.TerrainType _terrainType,
        int _cellX,
        int _cellY)
    {
        if (_visualPalette == null)
            return null;

        int tileSize = _visualPalette.TileSizeInCells;
        int tileX = Mathf.FloorToInt(_cellX / (float)tileSize);
        int tileDepth = Mathf.FloorToInt(
            _chunk.GetGlobalDepth(_cellY) / (float)tileSize
        );

        Vector3Int visualKey = new Vector3Int(
            (int)_terrainType,
            tileX,
            tileDepth
        );

        if (_visualSprites.TryGetValue(visualKey, out Sprite cachedSprite))
            return cachedSprite;

        Sprite visualSprite = _visualPalette.GetSprite(
            _terrainType,
            tileX,
            tileDepth,
            _chunk.Seed
        );
        _visualSprites.Add(visualKey, visualSprite);
        return visualSprite;
    }

    private bool TryGetSpritePixels(
        Sprite _sprite,
        out Color32[] _pixels,
        out RectInt _spriteRect,
        out int _textureWidth)
    {
        _pixels = null;
        _spriteRect = default;
        _textureWidth = 0;

        if (_sprite == null || _sprite.texture == null)
            return false;

        Texture2D texture = _sprite.texture;

        if (_unreadableTextures.Contains(texture))
            return false;

        if (!_spritePixels.TryGetValue(texture, out _pixels))
        {
            try
            {
                _pixels = texture.GetPixels32();
                _spritePixels.Add(texture, _pixels);
            }
            catch (UnityException)
            {
                _unreadableTextures.Add(texture);
                Debug.LogWarning(
                    $"Terrain sprite texture '{texture.name}' must have Read/Write enabled.",
                    this
                );
                return false;
            }
        }

        Rect rect = _sprite.textureRect;
        _spriteRect = new RectInt(
            Mathf.RoundToInt(rect.x),
            Mathf.RoundToInt(rect.y),
            Mathf.Max(1, Mathf.RoundToInt(rect.width)),
            Mathf.Max(1, Mathf.RoundToInt(rect.height))
        );
        _textureWidth = texture.width;
        return true;
    }

    private int GetEffectiveRenderScale()
    {
        int requestedScale = _visualPalette == null
            ? 1
            : _visualPalette.PixelsPerCell;
        int largestChunkDimension = Mathf.Max(_chunk.Width, _chunk.Height);
        int maximumScale = largestChunkDimension <= 0
            ? 1
            : Mathf.Max(1, SystemInfo.maxTextureSize / largestChunkDimension);

        return Mathf.Clamp(requestedScale, 1, maximumScale);
    }

    private Color GetFallbackColor(TerrainBase.TerrainType _cell)
    {
        if (_visualPalette != null && _visualPalette.HasDefinition(_cell))
            return _visualPalette.GetFallbackColor(_cell);

        return GetLegacyColor(_cell);
    }

    private Color GetLegacyColor(TerrainBase.TerrainType _cell)
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

    private int PositiveModulo(int _value, int _divisor)
    {
        int remainder = _value % _divisor;
        return remainder < 0 ? remainder + _divisor : remainder;
    }

    #endregion
}
