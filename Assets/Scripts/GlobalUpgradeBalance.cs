using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GlobalUpgradeBalance
{
    #region Fields

    [SerializeField]
    private List<GlobalUpgradeDefinition> _upgrades =
        new List<GlobalUpgradeDefinition>();

    #endregion

    #region Properties

    public IReadOnlyList<GlobalUpgradeDefinition> Upgrades => _upgrades;

    #endregion

    #region Public Methods

    public bool EnsureDefaultDefinitions()
    {
        bool changed = false;

        if (_upgrades == null)
        {
            _upgrades = new List<GlobalUpgradeDefinition>();
            changed = true;
        }

        foreach (GlobalUpgradeType upgradeType in Enum.GetValues(typeof(GlobalUpgradeType)))
        {
            if (ContainsType(upgradeType))
                continue;

            _upgrades.Add(CreateDefault(upgradeType));
            changed = true;
        }

        return changed;
    }

    public bool TryGetDefinition(
        GlobalUpgradeType _upgradeType,
        out GlobalUpgradeDefinition _definition)
    {
        if (_upgrades != null)
        {
            foreach (GlobalUpgradeDefinition definition in _upgrades)
            {
                if (definition == null || definition.UpgradeType != _upgradeType)
                    continue;

                if (definition.HasValidConfiguration())
                {
                    _definition = definition;
                    return true;
                }

                break;
            }
        }

        _definition = CreateDefault(_upgradeType);
        return _definition != null;
    }

    #endregion

    #region Private Methods

    private bool ContainsType(GlobalUpgradeType _upgradeType)
    {
        if (_upgrades == null)
            return false;

        foreach (GlobalUpgradeDefinition definition in _upgrades)
        {
            if (definition != null && definition.UpgradeType == _upgradeType)
                return true;
        }

        return false;
    }

    private GlobalUpgradeDefinition CreateDefault(GlobalUpgradeType _upgradeType)
    {
        switch (_upgradeType)
        {
            case GlobalUpgradeType.SpeedLimit:
                return CreateMultiplierDefinition(
                    _upgradeType,
                    "Limite de velocidade",
                    5,
                    .1f
                );
            case GlobalUpgradeType.SteeringSpeed:
                return CreateMultiplierDefinition(
                    _upgradeType,
                    "Velocidade de curva",
                    5,
                    .1f
                );
            case GlobalUpgradeType.MaximumEnergy:
                return CreateMultiplierDefinition(
                    _upgradeType,
                    "Energia maxima",
                    5,
                    .1f
                );
            case GlobalUpgradeType.Luck:
                return CreateMultiplierDefinition(
                    _upgradeType,
                    "Sorte",
                    5,
                    .1f
                );
            case GlobalUpgradeType.MoneyMultiplier:
                return CreateMultiplierDefinition(
                    _upgradeType,
                    "Multiplicador de dinheiro",
                    10,
                    .1f
                );
            case GlobalUpgradeType.DashCount:
                return new GlobalUpgradeDefinition(
                    _upgradeType,
                    "Quantidade de dash",
                    5,
                    100,
                    10,
                    1f
                );
            default:
                return null;
        }
    }

    private GlobalUpgradeDefinition CreateMultiplierDefinition(
        GlobalUpgradeType _upgradeType,
        string _displayName,
        int _maximumLevel,
        float _effectPerLevel)
    {
        return new GlobalUpgradeDefinition(
            _upgradeType,
            _displayName,
            _maximumLevel,
            100,
            10,
            _effectPerLevel
        );
    }

    #endregion
}
