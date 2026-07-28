using UnityEngine;

[CreateAssetMenu(
    fileName = "LauncherData",
    menuName = "Digging Madness/Equipment/Launcher Data"
)]
public class LauncherData : ScriptableObject
{
    #region Fields

    [Header("Identity")]
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _displayName;

    [SerializeField]
    private Sprite _icon;

    [Header("Launch")]
    [SerializeField]
    private float _baseForce = 1f;

    [SerializeField]
    private float _forceGainPerLevel = .05f;

    [SerializeField]
    private LauncherBehaviorId _behaviorId = LauncherBehaviorId.PowerBar;

    #endregion

    #region Properties

    public string Id => _id;

    public string DisplayName => _displayName;

    public Sprite Icon => _icon;

    public float BaseForce => _baseForce;

    public float ForceGainPerLevel => _forceGainPerLevel;

    public LauncherBehaviorId BehaviorId => _behaviorId;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public float GetForceMultiplier(int _level)
    {
        return _baseForce + _forceGainPerLevel * Mathf.Max(0, _level);
    }

    public bool HasValidConfiguration()
    {
        return !string.IsNullOrWhiteSpace(_id) &&
               !string.IsNullOrWhiteSpace(_displayName) &&
               IsFinitePositive(_baseForce) &&
               IsFiniteNonNegative(_forceGainPerLevel);
    }

    #endregion

    #region Private Methods

    private bool IsFinitePositive(float _value)
    {
        return _value > 0f && !float.IsNaN(_value) && !float.IsInfinity(_value);
    }

    private bool IsFiniteNonNegative(float _value)
    {
        return _value >= 0f && !float.IsNaN(_value) && !float.IsInfinity(_value);
    }

    #endregion
}
