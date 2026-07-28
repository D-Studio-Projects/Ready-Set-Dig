using System;
using UnityEngine;

[Serializable]
public class EquipmentUpgradeProgress
{
    #region Fields

    [SerializeField]
    private string _equipmentId;

    [SerializeField]
    private int _level;

    #endregion

    #region Properties

    public string EquipmentId => _equipmentId;

    public int Level => _level;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public EquipmentUpgradeProgress()
    {
        _equipmentId = string.Empty;
        _level = 0;
    }

    public EquipmentUpgradeProgress(string _equipmentId, int _level)
    {
        this._equipmentId = _equipmentId;
        this._level = _level;
    }

    #endregion

    #region Private Methods

    #endregion
}
