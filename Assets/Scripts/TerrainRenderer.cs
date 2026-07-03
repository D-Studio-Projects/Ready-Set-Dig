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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        texture = new Texture2D(terrain.Width, terrain.Height);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        DrawEntireMap();
    }

    void Update()
    {
        DrawEntireMap();
    }

    void DrawEntireMap()
    {
        for (int x = 0; x < terrain.Width; x++)
        {
            for (int y = 0; y < terrain.Height; y++)
            {
                TerrainType cell = terrain.GetCell(x, y);

                switch (cell)
                {
                    case TerrainType.Air:
                        texture.SetPixel(x, y, Color.clear);
                        break;

                    case TerrainType.Dirt:
                        texture.SetPixel(x, y, dirtColor);
                        break;

                    case TerrainType.Stone:
                        texture.SetPixel(x, y, stoneColor);
                        break;

                    case TerrainType.Iron:
                        texture.SetPixel(x, y, ironColor);
                        break;

                    case TerrainType.Gold:
                        texture.SetPixel(x, y, goldColor);
                        break;
                }
            }
        }

        texture.Apply();

        spriteRenderer.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, terrain.Width, terrain.Height),
            new Vector2(.5f, .5f),
            terrain.PixelsPerUnit
        );
    }
}