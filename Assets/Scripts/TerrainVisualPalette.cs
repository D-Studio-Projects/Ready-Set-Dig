using System;
using UnityEngine;

[Serializable]
public class TerrainVisualDefinition
{
    #region Fields

    [SerializeField]
    private TerrainBase.TerrainType _terrainType = TerrainBase.TerrainType.Dirt;

    [SerializeField]
    private Sprite[] _variations;

    [SerializeField]
    private Color _fallbackColor = Color.white;

    #endregion

    #region Properties

    public TerrainBase.TerrainType TerrainType => _terrainType;

    public Color FallbackColor => _fallbackColor;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public Sprite GetVariation(int _selection)
    {
        int validCount = CountValidVariations();

        if (validCount == 0)
            return null;

        int selectedIndex = PositiveModulo(_selection, validCount);

        foreach (Sprite variation in _variations)
        {
            if (variation == null)
                continue;

            if (selectedIndex == 0)
                return variation;

            selectedIndex--;
        }

        return null;
    }

    #endregion

    #region Private Methods

    private int CountValidVariations()
    {
        if (_variations == null)
            return 0;

        int count = 0;

        foreach (Sprite variation in _variations)
        {
            if (variation != null)
                count++;
        }

        return count;
    }

    private int PositiveModulo(int _value, int _divisor)
    {
        int remainder = _value % _divisor;
        return remainder < 0 ? remainder + _divisor : remainder;
    }

    #endregion
}

[CreateAssetMenu(
    fileName = "TerrainVisualPalette",
    menuName = "Digging Madness/Terrain/Visual Palette"
)]
public class TerrainVisualPalette : ScriptableObject
{
    #region Fields

    [Header("Tile Layout")]
    [SerializeField]
    [Min(1)]
    private int _tileSizeInCells = 128;

    [SerializeField]
    [Range(1, 8)]
    private int _pixelsPerCell = 1;

    [Header("Terrain Visuals")]
    [SerializeField]
    private TerrainVisualDefinition[] _definitions;

    [SerializeField]
    private Color _missingVisualColor = Color.magenta;

    #endregion

    #region Properties

    public int TileSizeInCells => Mathf.Max(1, _tileSizeInCells);

    public int PixelsPerCell => Mathf.Clamp(_pixelsPerCell, 1, 8);

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public Sprite GetSprite(
        TerrainBase.TerrainType _terrainType,
        int _tileX,
        int _tileDepth,
        int _seed)
    {
        TerrainVisualDefinition definition = FindDefinition(_terrainType);

        return definition == null
            ? null
            : definition.GetVariation(CreateSelection(
                _terrainType,
                _tileX,
                _tileDepth,
                _seed
            ));
    }

    public Color GetFallbackColor(TerrainBase.TerrainType _terrainType)
    {
        TerrainVisualDefinition definition = FindDefinition(_terrainType);
        return definition == null
            ? _missingVisualColor
            : definition.FallbackColor;
    }

    public bool HasDefinition(TerrainBase.TerrainType _terrainType)
    {
        return FindDefinition(_terrainType) != null;
    }

    #endregion

    #region Private Methods

    private TerrainVisualDefinition FindDefinition(
        TerrainBase.TerrainType _terrainType)
    {
        if (_definitions == null)
            return null;

        foreach (TerrainVisualDefinition definition in _definitions)
        {
            if (definition != null && definition.TerrainType == _terrainType)
                return definition;
        }

        return null;
    }

    private int CreateSelection(
        TerrainBase.TerrainType _terrainType,
        int _tileX,
        int _tileDepth,
        int _seed)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _seed;
            hash = hash * 31 + (int)_terrainType;
            hash = hash * 31 + _tileX;
            hash = hash * 31 + _tileDepth;
            hash ^= hash >> 16;
            hash *= 0x45d9f3b;
            hash ^= hash >> 16;
            return hash;
        }
    }

    #endregion
}
