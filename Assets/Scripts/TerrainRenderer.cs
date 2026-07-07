using UnityEngine;
using static Terrain;

[RequireComponent(typeof(SpriteRenderer))]
public class TerrainRenderer : MonoBehaviour
{
    [SerializeField] private Terrain terrain;

    [SerializeField]
    private Color dirtColor = new Color(0.35f, 0.22f, 0.12f);

    [SerializeField]
    private Color stoneColor = Color.gray;

    [SerializeField]
    private Color ironColor = Color.yellow;

    [SerializeField]
    private Color goldColor = new Color(1f, 0.75f, 0f);

    private Texture2D texture;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (terrain != null)
        {
            terrain.CellsChanged += DrawDirtyRect;
        }
    }

    private void OnDisable()
    {
        if (terrain != null)
        {
            terrain.CellsChanged -= DrawDirtyRect;
        }
    }

    private void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("TerrainRenderer needs a Terrain reference.", this);
            enabled = false;
            return;
        }

        texture = new Texture2D(terrain.Width, terrain.Height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        spriteRenderer.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, terrain.Width, terrain.Height),
            new Vector2(.5f, .5f),
            terrain.PixelsPerUnit
        );

        DrawEntireMap();
    }

    private void DrawEntireMap()
    {
        DrawCells(new RectInt(0, 0, terrain.Width, terrain.Height));
        texture.Apply(false);
    }

    private void DrawDirtyRect(RectInt dirtyRect)
    {
        if (texture == null)
            return;

        RectInt clampedRect = ClampToTerrain(dirtyRect);

        if (clampedRect.width <= 0 || clampedRect.height <= 0)
            return;

        DrawCells(clampedRect);
        texture.Apply(false);
    }

    private RectInt ClampToTerrain(RectInt rect)
    {
        int xMin = Mathf.Clamp(rect.xMin, 0, terrain.Width);
        int yMin = Mathf.Clamp(rect.yMin, 0, terrain.Height);
        int xMax = Mathf.Clamp(rect.xMax, 0, terrain.Width);
        int yMax = Mathf.Clamp(rect.yMax, 0, terrain.Height);

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void DrawCells(RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                texture.SetPixel(x, y, GetColor(terrain.GetCell(x, y)));
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
                return dirtColor;
            case TerrainType.Stone:
                return stoneColor;
            case TerrainType.Iron:
                return ironColor;
            case TerrainType.Gold:
                return goldColor;
            default:
                return Color.magenta;
        }
    }
}
