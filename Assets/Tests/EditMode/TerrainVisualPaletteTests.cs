using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DiggingMadness.Tests.EditMode
{
    public class TerrainVisualPaletteTests
    {
        [Test]
        public void ProjectPaletteHasTerrainAndFutureResourceSlots()
        {
            TerrainVisualPalette palette =
                AssetDatabase.LoadAssetAtPath<TerrainVisualPalette>(
                    "Assets/Data/Terrain/TerrainVisualPalette.asset"
                );

            Assert.That(palette, Is.Not.Null);
            Assert.That(palette.TileSizeInCells, Is.EqualTo(128));
            Assert.That(palette.PixelsPerCell, Is.EqualTo(1));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Dirt));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Stone));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Iron));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Gold));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Sapphire));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Ruby));
            Assert.That(palette.HasDefinition(TerrainBase.TerrainType.Diamond));
        }
        [Test]
        public void ChunkRendererUsesTileArtAndPreservesDigTransparency()
        {
            GameObject chunkObject = new GameObject("TerrainVisualTest");
            SpriteRenderer targetRenderer =
                chunkObject.AddComponent<SpriteRenderer>();
            TerrainChunk chunk = chunkObject.AddComponent<TerrainChunk>();
            TerrainChunkRenderer chunkRenderer =
                chunkObject.AddComponent<TerrainChunkRenderer>();
            Texture2D sourceTexture = CreateSourceTexture();
            Sprite sourceSprite = Sprite.Create(
                sourceTexture,
                new Rect(0f, 0f, 128f, 128f),
                new Vector2(.5f, .5f),
                128f
            );
            TerrainVisualPalette palette =
                ScriptableObject.CreateInstance<TerrainVisualPalette>();

            ConfigurePalette(palette, sourceSprite);
            ConfigureRenderer(
                chunk,
                chunkRenderer,
                targetRenderer,
                palette
            );

            chunk.Initialize(0, 32, 32, 32, 7, null);

            Texture2D renderedTexture = targetRenderer.sprite.texture;
            Assert.That(renderedTexture.width, Is.EqualTo(32));
            Assert.That(renderedTexture.height, Is.EqualTo(32));
            Assert.That(renderedTexture.GetPixel(16, 16).r, Is.EqualTo(1f));

            chunk.Dig(Vector2.zero, .1f);

            Assert.That(renderedTexture.GetPixel(16, 16).a, Is.Zero);

            Object.DestroyImmediate(chunkObject);
            Object.DestroyImmediate(sourceSprite);
            Object.DestroyImmediate(sourceTexture);
            Object.DestroyImmediate(palette);
        }

        private Texture2D CreateSourceTexture()
        {
            Texture2D texture = new Texture2D(
                128,
                128,
                TextureFormat.RGBA32,
                false
            );
            Color32[] pixels = new Color32[128 * 128];

            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = new Color32(255, 0, 0, 255);

            texture.SetPixels32(pixels);
            texture.Apply(false);
            return texture;
        }

        private void ConfigurePalette(
            TerrainVisualPalette _palette,
            Sprite _sourceSprite)
        {
            SerializedObject paletteObject = new SerializedObject(_palette);
            paletteObject.FindProperty("_tileSizeInCells").intValue = 128;
            paletteObject.FindProperty("_pixelsPerCell").intValue = 1;
            SerializedProperty definitions =
                paletteObject.FindProperty("_definitions");
            definitions.arraySize = 1;
            SerializedProperty definition =
                definitions.GetArrayElementAtIndex(0);
            definition.FindPropertyRelative("_terrainType").enumValueIndex =
                (int)TerrainBase.TerrainType.Dirt;
            SerializedProperty variations =
                definition.FindPropertyRelative("_variations");
            variations.arraySize = 1;
            variations.GetArrayElementAtIndex(0).objectReferenceValue =
                _sourceSprite;
            definition.FindPropertyRelative("_fallbackColor").colorValue =
                Color.red;
            paletteObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void ConfigureRenderer(
            TerrainChunk _chunk,
            TerrainChunkRenderer _chunkRenderer,
            SpriteRenderer _targetRenderer,
            TerrainVisualPalette _palette)
        {
            SerializedObject chunkObject = new SerializedObject(_chunk);
            chunkObject.FindProperty("_chunkRenderer").objectReferenceValue =
                _chunkRenderer;
            chunkObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject rendererObject =
                new SerializedObject(_chunkRenderer);
            rendererObject.FindProperty("_chunk").objectReferenceValue = _chunk;
            rendererObject.FindProperty("_targetRenderer").objectReferenceValue =
                _targetRenderer;
            rendererObject.FindProperty("_visualPalette").objectReferenceValue =
                _palette;
            rendererObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
