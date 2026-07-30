using System;
using UnityEngine;

[Serializable]
public class TerrainObstacleSpawnDefinition
{
    #region Fields

    [SerializeField]
    private string _id = "resistant-rock";

    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    [Min(0)]
    private int _minimumDepth = 128;

    [SerializeField]
    private int _maximumDepth = -1;

    [SerializeField]
    [Range(0f, 1f)]
    private float _spawnChancePerSlot = .25f;

    [SerializeField]
    [Min(0)]
    private int _minimumCountPerChunk;

    [SerializeField]
    [Min(0)]
    private int _maximumCountPerChunk = 1;

    [SerializeField]
    [Min(0f)]
    private float _horizontalPadding = 2f;

    [SerializeField]
    [Min(0f)]
    private float _verticalPadding = .25f;

    [SerializeField]
    private float _localZ = -.1f;

    #endregion

    #region Properties

    public string Id => string.IsNullOrWhiteSpace(_id) ? "obstacle" : _id.Trim();

    public GameObject Prefab => _prefab;

    public int MinimumDepth => Mathf.Max(0, _minimumDepth);

    public int MaximumDepth => _maximumDepth < 0
        ? int.MaxValue
        : Mathf.Max(MinimumDepth, _maximumDepth);

    public float HorizontalPadding => Mathf.Max(0f, _horizontalPadding);

    public float VerticalPadding => Mathf.Max(0f, _verticalPadding);

    public float LocalZ => _localZ;

    #endregion

    #region Public Methods

    public bool IsAvailableInDepthRange(int _startDepth, int _endDepth)
    {
        if (_prefab == null)
            return false;

        int minimumDepth = MinimumDepth;
        int maximumDepth = MaximumDepth;

        return _endDepth >= minimumDepth && _startDepth <= maximumDepth;
    }

    public int GetSpawnCount(System.Random _random)
    {
        if (_prefab == null || _random == null)
            return 0;

        int maximumCount = Mathf.Max(0, _maximumCountPerChunk);
        int minimumCount = Mathf.Clamp(_minimumCountPerChunk, 0, maximumCount);
        int count = minimumCount;
        float chance = Mathf.Clamp01(_spawnChancePerSlot);

        for (int slot = minimumCount; slot < maximumCount; slot++)
        {
            if (_random.NextDouble() <= chance)
                count++;
        }

        return count;
    }

    #endregion
}
