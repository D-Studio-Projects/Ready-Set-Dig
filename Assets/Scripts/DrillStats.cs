using UnityEngine;

public readonly struct DrillStats
{
    #region Fields

    private readonly float _digDamage;
    private readonly float _energyConsumption;
    private readonly float _digSpeed;

    #endregion

    #region Properties

    public float DigDamage => _digDamage;

    public float EnergyConsumption => _energyConsumption;

    public float DigSpeed => _digSpeed;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public DrillStats(float _digDamage, float _energyConsumption, float _digSpeed)
    {
        this._digDamage = _digDamage;
        this._energyConsumption = _energyConsumption;
        this._digSpeed = _digSpeed;
    }

    public bool IsValid()
    {
        return _digDamage >= 0f &&
               _energyConsumption >= 0f &&
               _digSpeed > 0f &&
               !float.IsNaN(_digDamage) &&
               !float.IsNaN(_energyConsumption) &&
               !float.IsNaN(_digSpeed) &&
               !float.IsInfinity(_digDamage) &&
               !float.IsInfinity(_energyConsumption) &&
               !float.IsInfinity(_digSpeed);
    }

    public static DrillStats FromToolData(ToolData _toolData)
    {
        if (_toolData == null)
            return default;

        return new DrillStats(
            _toolData.DigDamage,
            _toolData.EnergyConsumption,
            _toolData.DigSpeed
        );
    }

    #endregion

    #region Private Methods

    #endregion
}
