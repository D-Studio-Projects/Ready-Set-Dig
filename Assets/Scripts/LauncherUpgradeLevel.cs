using System;
using UnityEngine;

[Serializable]
public class LauncherUpgradeLevel
{
    #region Fields

    [SerializeField]
    private long _price;

    #endregion

    #region Properties

    public long Price => _price;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool HasValidConfiguration()
    {
        return _price >= 0;
    }

    #endregion

    #region Private Methods

    #endregion
}
