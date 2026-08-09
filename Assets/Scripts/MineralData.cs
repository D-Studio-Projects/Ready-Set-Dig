using UnityEngine;

[CreateAssetMenu(
    fileName = "MineralData",
    menuName = "Digging Madness/Minerals/Mineral Data"
)]
public class MineralData : ScriptableObject
{
    #region Fields

    [Header("Identity")]
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [Header("Visual")]
    [SerializeField]
    private Sprite _sprite;

    [SerializeField]
    private Color _color = Color.white;

    [Header("Reward")]
    [SerializeField]
    [Min(0f)]
    private float _moneyValue = 1f;

    [Header("Spawn")]
    [SerializeField]
    [Min(0f)]
    private float _spawnWeight = 1f;

    [SerializeField]
    [Min(0f)]
    private float _minimumDepth;

    [SerializeField]
    [Min(0f)]
    private float _maximumDepth;

    #endregion

    #region Properties

    public string Id => _id;

    public string DisplayName => _displayName;

    public Sprite Sprite => _sprite;

    public Color Color => _color;

    public float MoneyValue => Mathf.Max(0f, _moneyValue);

    public float SpawnWeight => Mathf.Max(0f, _spawnWeight);

    public float MinimumDepth => Mathf.Max(0f, _minimumDepth);

    public float MaximumDepth => Mathf.Max(0f, _maximumDepth);

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool HasValidConfiguration()
    {
        return !string.IsNullOrWhiteSpace(_id) &&
               !string.IsNullOrWhiteSpace(_displayName) &&
               IsFiniteNonNegative(_moneyValue) &&
               IsFiniteNonNegative(_spawnWeight) &&
               IsFiniteNonNegative(_minimumDepth) &&
               IsFiniteNonNegative(_maximumDepth) &&
               (_maximumDepth <= 0f || _maximumDepth >= _minimumDepth);
    }

    public bool IsAvailableAtDepth(float _depth)
    {
        if (!HasValidConfiguration() || _spawnWeight <= 0f)
            return false;

        float depth = Mathf.Max(0f, _depth);

        return depth >= MinimumDepth &&
               (MaximumDepth <= 0f || depth <= MaximumDepth);
    }

    #endregion

    #region Private Methods

    private bool IsFiniteNonNegative(float _value)
    {
        return _value >= 0f &&
               !float.IsNaN(_value) &&
               !float.IsInfinity(_value);
    }

    #endregion
}
