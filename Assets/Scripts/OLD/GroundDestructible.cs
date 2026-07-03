using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GroundDestructible : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D texture;
    private Sprite runtimeSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Texture2D original = spriteRenderer.sprite.texture;

        texture = Instantiate(original);

        runtimeSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            spriteRenderer.sprite.pixelsPerUnit
        );

        spriteRenderer.sprite = runtimeSprite;
    }

    public void Dig(Vector2 worldPosition, float radius)
    {
        Debug.Log("Escavando!");
        Vector2 localPos = transform.InverseTransformPoint(worldPosition);

        float pixelsPerUnit = runtimeSprite.pixelsPerUnit;

        int centerX = Mathf.RoundToInt(localPos.x * pixelsPerUnit + texture.width * 0.5f);
        int centerY = Mathf.RoundToInt(localPos.y * pixelsPerUnit + texture.height * 0.5f);

        int pixelRadius = Mathf.RoundToInt(radius * pixelsPerUnit);

        for (int x = -pixelRadius; x <= pixelRadius; x++)
        {
            for (int y = -pixelRadius; y <= pixelRadius; y++)
            {
                if (x * x + y * y > pixelRadius * pixelRadius)
                    continue;

                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || py < 0 || px >= texture.width || py >= texture.height)
                    continue;

                Color color = texture.GetPixel(px, py);
                color.a = 0f;
                texture.SetPixel(px, py, color);
            }
        }

        texture.Apply();
    }
}