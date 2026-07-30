using System;
using UnityEngine;

[Serializable]
public class TerrainDepthLayer
{
    [SerializeField]
    private string _name = "Stone";

    [SerializeField]
    private TerrainBase.TerrainType _terrainType = TerrainBase.TerrainType.Stone;

    [SerializeField]
    private int _startDepth;

    [SerializeField]
    [Range(0f, 1f)]
    private float _baseChance = .15f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _chancePerDepth = .001f;

    [SerializeField]
    private float _noiseScale = .04f;

    [SerializeField]
    private float _noiseOffset;

    public string Name => _name;
    public TerrainBase.TerrainType TerrainType => _terrainType;
    public int StartDepth => _startDepth;
    public float BaseChance => _baseChance;
    public float ChancePerDepth => _chancePerDepth;
    public float NoiseScale => _noiseScale;
    public float NoiseOffset => _noiseOffset;

    public bool IsAvailableAtDepth(int _depth)
    {
        return _depth >= _startDepth && _terrainType != TerrainBase.TerrainType.Air;
    }

    public float GetDensityAtDepth(int _depth)
    {
        float depthProgress = Mathf.Max(0f, _depth - _startDepth);
        return Mathf.Clamp01(_baseChance + depthProgress * _chancePerDepth);
    }
}

[CreateAssetMenu(fileName = "TerrainDepthProfile", menuName = "Digging Madness/Terrain/Depth Profile")]
public class TerrainDepthProfile : ScriptableObject
{
    [SerializeField]
    private TerrainDepthLayer[] _layers;

    [SerializeField]
    [Range(0f, 1f)]
    private float _minimumDirtRatio = .65f;

    [Header("Resource Mix")]
    [SerializeField]
    [Range(0f, 1f)]
    private float _stoneWeight = .6f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _ironWeight = .32f;

    [SerializeField]
    [Range(0f, 1f)]
    private float _goldWeight = .08f;

    [Header("Fallback Start Depths")]
    [SerializeField]
    private int _fallbackStoneStartDepth;

    [SerializeField]
    private int _fallbackIronStartDepth = 32;

    [SerializeField]
    private int _fallbackGoldStartDepth = 96;

    public TerrainBase.TerrainType GetTerrainType(int _globalX, int _depth, int _seed)
    {
        return GetTerrainType(_globalX, _depth, _seed, 1f);
    }

    public TerrainBase.TerrainType GetTerrainType(
        int _globalX,
        int _depth,
        int _seed,
        float _luckMultiplier)
    {
        float luckMultiplier = SanitizeLuckMultiplier(_luckMultiplier);
        float dirtGate = GetNoise(_globalX, _depth, _seed, .017f, 53.71f);

        if (dirtGate < Mathf.Clamp01(_minimumDirtRatio))
            return TerrainBase.TerrainType.Dirt;

        float stoneWeight = GetResourceWeight(
            TerrainBase.TerrainType.Stone,
            _globalX,
            _depth,
            _seed,
            Mathf.Max(0, _fallbackStoneStartDepth),
            _stoneWeight,
            1f
        );
        float ironWeight = GetResourceWeight(
            TerrainBase.TerrainType.Iron,
            _globalX,
            _depth,
            _seed,
            Mathf.Max(0, _fallbackIronStartDepth),
            _ironWeight,
            luckMultiplier
        );
        float goldWeight = GetResourceWeight(
            TerrainBase.TerrainType.Gold,
            _globalX,
            _depth,
            _seed,
            Mathf.Max(0, _fallbackGoldStartDepth),
            _goldWeight,
            luckMultiplier
        );
        float totalWeight = stoneWeight + ironWeight + goldWeight;

        if (totalWeight <= 0f)
            return TerrainBase.TerrainType.Dirt;

        float resourceChoice =
            GetNoise(_globalX, _depth, _seed, .053f, 117.19f) *
            totalWeight;

        if (resourceChoice < stoneWeight)
            return TerrainBase.TerrainType.Stone;

        resourceChoice -= stoneWeight;

        if (resourceChoice < ironWeight)
            return TerrainBase.TerrainType.Iron;

        return TerrainBase.TerrainType.Gold;
    }

    private float GetResourceWeight(
        TerrainBase.TerrainType _terrainType,
        int _globalX,
        int _depth,
        int _seed,
        int _fallbackStartDepth,
        float _baseWeight,
        float _weightMultiplier)
    {
        bool hasConfiguredLayer = false;
        float strongestInfluence = 0f;

        if (_layers != null)
        {
            foreach (TerrainDepthLayer layer in _layers)
            {
                if (layer == null || layer.TerrainType != _terrainType)
                    continue;

                hasConfiguredLayer = true;

                if (layer.IsAvailableAtDepth(_depth))
                {
                    float density = layer.GetDensityAtDepth(_depth);
                    float layerNoise = GetNoise(
                        _globalX,
                        _depth,
                        _seed,
                        Mathf.Max(.0001f, layer.NoiseScale),
                        layer.NoiseOffset
                    );
                    float influence =
                        Mathf.Lerp(.5f, 1f, density) *
                        Mathf.Lerp(.25f, 1f, layerNoise);
                    strongestInfluence = Mathf.Max(strongestInfluence, influence);
                }
            }
        }

        if (hasConfiguredLayer)
            return Mathf.Max(0f, _baseWeight) *
                   strongestInfluence *
                   Mathf.Max(0f, _weightMultiplier);

        return _depth >= _fallbackStartDepth
            ? Mathf.Max(0f, _baseWeight) * Mathf.Max(0f, _weightMultiplier)
            : 0f;
    }

    private float SanitizeLuckMultiplier(float _multiplier)
    {
        if (float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
            return 1f;

        return Mathf.Max(1f, _multiplier);
    }

    private float GetNoise(
        int _globalX,
        int _depth,
        int _seed,
        float _scale,
        float _offset)
    {
        float seedX = _seed * .01337f + _offset;
        float seedY = _seed * .03171f + _offset * 1.7f;

        return Mathf.PerlinNoise(
            (_globalX + seedX) * _scale,
            (_depth + seedY) * _scale
        );
    }
}
