using System;
using UnityEngine;

[Serializable]
public class TerrainDepthLayer
{
    #region Fields

    [SerializeField]
    private string _name = "Stone";

    [SerializeField]
    private TerrainBase.TerrainType _terrainType = TerrainBase.TerrainType.Stone;

    [SerializeField]
    private int _startDepth;

    [SerializeField]
    [Range(0f, 1f)]
    private float _baseChance = .3f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _chancePerDepth = .005f;

    [SerializeField]
    private float _noiseScale = .04f;

    [SerializeField]
    private float _noiseOffset;

    #endregion

    #region Properties

    public string Name => _name;

    public TerrainBase.TerrainType TerrainType => _terrainType;

    public int StartDepth => _startDepth;

    public float BaseChance => _baseChance;

    public float ChancePerDepth => _chancePerDepth;

    public float NoiseScale => _noiseScale;

    public float NoiseOffset => _noiseOffset;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool IsAvailableAtDepth(int _depth)
    {
        return _depth >= _startDepth && _terrainType != TerrainBase.TerrainType.Air;
    }

    public float GetDensityAtDepth(int _depth)
    {
        float depthProgress = Mathf.Max(0f, _depth - _startDepth);
        return Mathf.Clamp01(_baseChance + depthProgress * _chancePerDepth);
    }

    #endregion

    #region Private Methods

    #endregion
}

[CreateAssetMenu(fileName = "TerrainDepthProfile", menuName = "Digging Madness/Terrain/Depth Profile")]
public class TerrainDepthProfile : ScriptableObject
{
    #region Fields

    [SerializeField]
    private TerrainDepthLayer[] _layers;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public TerrainBase.TerrainType GetTerrainType(int _globalX, int _depth, int _seed)
    {
        if (_layers == null)
            return TerrainBase.TerrainType.Dirt;

        for (int index = _layers.Length - 1; index >= 0; index--)
        {
            TerrainDepthLayer layer = _layers[index];

            if (layer == null || !layer.IsAvailableAtDepth(_depth))
                continue;

            float density = layer.GetDensityAtDepth(_depth);

            if (density <= 0f)
                continue;

            float noise = GetNoise(_globalX, _depth, _seed, layer);

            if (noise >= 1f - density)
                return layer.TerrainType;
        }

        return TerrainBase.TerrainType.Dirt;
    }

    #endregion

    #region Private Methods

    private float GetNoise(int _globalX, int _depth, int _seed, TerrainDepthLayer _layer)
    {
        float seedX = _seed * .01337f + _layer.NoiseOffset;
        float seedY = _seed * .03171f + _layer.NoiseOffset * 1.7f;
        float scale = Mathf.Max(.0001f, _layer.NoiseScale);

        return Mathf.PerlinNoise(
            (_globalX + seedX) * scale,
            (_depth + seedY) * scale
        );
    }

    #endregion
}
