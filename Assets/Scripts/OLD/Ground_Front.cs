using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Ground_Front : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Texture2D original = spriteRenderer.sprite.texture;

        Texture2D runtimeTexture = Instantiate(original);

        spriteRenderer.sprite = Sprite.Create(
            runtimeTexture,
            new Rect(0, 0, runtimeTexture.width, runtimeTexture.height),
            new Vector2(0.5f, 0.5f),
            spriteRenderer.sprite.pixelsPerUnit
        );
    }
}