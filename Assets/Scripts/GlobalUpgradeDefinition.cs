using System;
using UnityEngine;

[Serializable]
public class GlobalUpgradeDefinition
{
    #region Fields

    [SerializeField]
    private GlobalUpgradeType _upgradeType;

    [SerializeField]
    private string _displayName;

    [TextArea]
    [SerializeField]
    private string _description;

    [SerializeField]
    [Min(1)]
    private int _maximumLevel = 5;

    [SerializeField]
    [Min(0)]
    private long _basePrice = 100;

    [SerializeField]
    [Min(0)]
    private int _priceIncreasePercent = 10;

    [SerializeField]
    [Min(0f)]
    private float _effectPerLevel = .1f;

    #endregion

    #region Properties

    public GlobalUpgradeType UpgradeType => _upgradeType;

    public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
        ? _upgradeType.ToString()
        : _displayName;

    public string Description => string.IsNullOrWhiteSpace(_description)
        ? GetDefaultDescription()
        : _description;

    public int MaximumLevel => Mathf.Max(1, _maximumLevel);

    public long BasePrice => Math.Max(0, _basePrice);

    public int PriceIncreasePercent => Mathf.Max(0, _priceIncreasePercent);

    public float EffectPerLevel => Mathf.Max(0f, _effectPerLevel);

    #endregion

    #region Public Methods

    public GlobalUpgradeDefinition()
    {
    }

    public GlobalUpgradeDefinition(
        GlobalUpgradeType _upgradeType,
        string _displayName,
        int _maximumLevel,
        long _basePrice,
        int _priceIncreasePercent,
        float _effectPerLevel)
    {
        this._upgradeType = _upgradeType;
        this._displayName = _displayName;
        this._maximumLevel = _maximumLevel;
        this._basePrice = _basePrice;
        this._priceIncreasePercent = _priceIncreasePercent;
        this._effectPerLevel = _effectPerLevel;
    }

    public bool HasValidConfiguration()
    {
        return _maximumLevel > 0 &&
               _basePrice >= 0 &&
               _priceIncreasePercent >= 0 &&
               _effectPerLevel >= 0f &&
               !float.IsNaN(_effectPerLevel) &&
               !float.IsInfinity(_effectPerLevel);
    }

    public long GetPriceForLevel(int _level)
    {
        if (_level <= 0 || _level > MaximumLevel)
            return -1;

        decimal price = BasePrice;
        decimal multiplier = (100m + PriceIncreasePercent) / 100m;

        for (int level = 1; level < _level; level++)
        {
            price = decimal.Ceiling(price * multiplier);

            if (price >= long.MaxValue)
                return long.MaxValue;
        }

        return (long)price;
    }

    public float GetMultiplier(int _level)
    {
        int level = Mathf.Clamp(_level, 0, MaximumLevel);
        return 1f + EffectPerLevel * level;
    }

    public int GetFlatBonus(int _level)
    {
        int level = Mathf.Clamp(_level, 0, MaximumLevel);
        return Mathf.Max(0, Mathf.RoundToInt(EffectPerLevel * level));
    }

    #endregion

    #region Private Methods

    private string GetDefaultDescription()
    {
        switch (_upgradeType)
        {
            case GlobalUpgradeType.SpeedLimit:
                return "Aumenta a velocidade maxima que voce alcanca durante a descida.";
            case GlobalUpgradeType.SteeringSpeed:
                return "Deixa as mudancas de direcao mais rapidas durante a run.";
            case GlobalUpgradeType.MaximumEnergy:
                return "Aumenta a energia maxima disponivel em cada tentativa.";
            case GlobalUpgradeType.Luck:
                return "Aumenta a chance de encontrar minerios mais valiosos.";
            case GlobalUpgradeType.MoneyMultiplier:
                return "Aumenta todo o dinheiro recebido ao encerrar uma run.";
            case GlobalUpgradeType.DashCount:
                return "Adiciona um uso de dash para cada nivel comprado.";
            default:
                return "Melhoria permanente para as proximas runs.";
        }
    }

    #endregion
}
