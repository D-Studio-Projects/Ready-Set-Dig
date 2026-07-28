using System;
using UnityEngine;

[Serializable]
public class DrillUpgradeLevel
{
    #region Fields

    [SerializeField]
    private long _price;

    [SerializeField]
    private float _digDamage = 1f;

    [SerializeField]
    private float _energyConsumption = 8f;

    [SerializeField]
    private float _digSpeed = 1f;

    #endregion

    #region Properties

    public long Price => _price;

    public float DigDamage => _digDamage;

    public float EnergyConsumption => _energyConsumption;

    public float DigSpeed => _digSpeed;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public bool HasValidConfiguration()
    {
        return _price >= 0 &&
               new DrillStats(_digDamage, _energyConsumption, _digSpeed).IsValid();
    }

    public DrillStats GetStats()
    {
        return new DrillStats(_digDamage, _energyConsumption, _digSpeed);
    }

    #endregion

    #region Private Methods

    #endregion
}
