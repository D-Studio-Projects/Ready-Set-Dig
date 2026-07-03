using UnityEngine;

public class TerrainData
{
    public int width;
    public int height;

    public bool[,] pixels;

    public TerrainData(int width, int height)
    {
        this.width = width;
        this.height = height;

        pixels = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = true;
            }
        }
    }
}