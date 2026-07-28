using UnityEngine;
using static TerrainBase;

public class TerrainRenderer : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private TerrainBase _terrain;

    [SerializeField]
    private SpriteRenderer _targetRenderer;

    [Header("Colors")]
    [SerializeField]
    private Color _dirtColor = new Color(0.35f, 0.22f, 0.12f);

    [SerializeField]
    private Color _stoneColor = Color.gray;

    [SerializeField]
    private Color _ironColor = Color.yellow;

    [SerializeField]
    private Color _goldColor = new Color(1f, 0.75f, 0f);

    private Texture2D _texture;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_terrain != null)
        {
            _terrain.CellsChanged += DrawDirtyRect;
        }
    }

    private void OnDisable()
    {
        if (_terrain != null)
        {
            _terrain.CellsChanged -= DrawDirtyRect;
        }
    }

    private void Start()
    {
        if (_terrain == null)
        {
            Debug.LogError("TerrainRenderer needs a TerrainBase reference.", this);
            enabled = false;
            return;
        }

        if (_targetRenderer == null)
        {
            Debug.LogError("TerrainRenderer needs a SpriteRenderer reference.", this);
            enabled = false;
            return;
        }

        _texture = new Texture2D(_terrain.Width, _terrain.Height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        _targetRenderer.sprite = Sprite.Create(
            _texture,
            new Rect(0, 0, _terrain.Width, _terrain.Height),
            new Vector2(.5f, .5f),
            _terrain.PixelsPerUnit
        );

        DrawEntireMap();
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void DrawEntireMap()
    {
        DrawCells(new RectInt(0, 0, _terrain.Width, _terrain.Height));
        _texture.Apply(false);
    }

    private void DrawDirtyRect(RectInt dirtyRect)
    {
        if (_texture == null)
            return;

        RectInt clampedRect = ClampToTerrain(dirtyRect);

        if (clampedRect.width <= 0 || clampedRect.height <= 0)
            return;

        DrawCells(clampedRect);
        _texture.Apply(false);
    }

    private RectInt ClampToTerrain(RectInt rect)
    {
        int xMin = Mathf.Clamp(rect.xMin, 0, _terrain.Width);
        int yMin = Mathf.Clamp(rect.yMin, 0, _terrain.Height);
        int xMax = Mathf.Clamp(rect.xMax, 0, _terrain.Width);
        int yMax = Mathf.Clamp(rect.yMax, 0, _terrain.Height);

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void DrawCells(RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                _texture.SetPixel(x, y, GetColor(_terrain.GetCell(x, y)));
            }
        }
    }

    private Color GetColor(TerrainType cell)
    {
        switch (cell)
        {
            case TerrainType.Air:
                return Color.clear;
            case TerrainType.Dirt:
                return _dirtColor;
            case TerrainType.Stone:
                return _stoneColor;
            case TerrainType.Iron:
                return _ironColor;
            case TerrainType.Gold:
                return _goldColor;
            default:
                return Color.magenta;
        }
    }

    #endregion
}

