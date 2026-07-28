using System;
using UnityEngine;

[Serializable]
public class LauncherUpgradeLevel
{
    #region Fields

    [SerializeField]
    private long _price;

    [SerializeField]
    private float _launchForceMultiplier = 1f;

    #endregion

    #region Properties

    public long Price => _price;

    public float LaunchForceMultiplier => _launchForceMultiplier;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool HasValidConfiguration()
    {
        return _price >= 0 &&
               _launchForceMultiplier > 0f &&
               !float.IsNaN(_launchForceMultiplier) &&
               !float.IsInfinity(_launchForceMultiplier);
    }

    #endregion

    #region Private Methods

    #endregion
}
