using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Terrain : MonoBehaviour
{
    public enum TerrainType : byte
    {
        Air = 0,
        Dirt = 1,
        Stone = 2,
        Iron = 3,
        Gold = 4
    }
    [Header("Terrain Size")]

    [SerializeField] private int width = 512;
    [SerializeField] private int height = 256;

    [Header("Pixels Per Unit")]

    [SerializeField] private int pixelsPerUnit = 32;

    public int Width => width;
    public int Height => height;

    public int PixelsPerUnit => pixelsPerUnit;

    private byte[,] map;

    public byte[,] Map => map;

    private void Awake()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        map = new byte[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (byte)TerrainType.Dirt;
            }
        }
    }

    public bool InsideMap(int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < width &&
               y < height;
    }

    public TerrainType GetCell(int x, int y)
    {
        if (!InsideMap(x, y))
            return TerrainType.Air;

        return (TerrainType)map[x, y];
    }

    public void SetCell(int x, int y, TerrainType value)
    {
        if (!InsideMap(x, y))
            return;

        map[x, y] = (byte)value;
    }

    public Vector2Int WorldToCell(Vector2 worldPosition)
    {
        Vector2 local = transform.InverseTransformPoint(worldPosition);

        int x = Mathf.RoundToInt(local.x * pixelsPerUnit + width * .5f);
        int y = Mathf.RoundToInt(local.y * pixelsPerUnit + height * .5f);

        return new Vector2Int(x, y);
    }

    public Vector2 CellToWorld(int x, int y)
    {
        float worldX = (x - width * .5f) / pixelsPerUnit;
        float worldY = (y - height * .5f) / pixelsPerUnit;

        return transform.TransformPoint(new Vector2(worldX, worldY));
    }

    public void Dig(Vector2 worldPosition, float radius)
    {
        Vector2Int center = WorldToCell(worldPosition);

        int pixelRadius = Mathf.RoundToInt(radius * pixelsPerUnit);

        for (int x = -pixelRadius; x <= pixelRadius; x++)
        {
            for (int y = -pixelRadius; y <= pixelRadius; y++)
            {
                if (x * x + y * y > pixelRadius * pixelRadius)
                    continue;

                int px = center.x + x;
                int py = center.y + y;

                SetCell(px, py, TerrainType.Air);
            }
        }
    }
}