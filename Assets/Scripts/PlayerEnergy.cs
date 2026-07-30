using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private PlayerMovement _playerMovement;

    [Header("Energy")]
    [SerializeField]
    private float _maxEnergy = 100f;

    [SerializeField]
    private float _digDrainPerSecond = 8f;

    [Header("Obstacle Drain")]
    [SerializeField]
    private float _defaultObstacleMultiplier = 2f;

    [SerializeField]
    private string _obstacleTag = "Obstacle";

    [SerializeField]
    private LayerMask _obstacleLayers;

    private readonly Dictionary<Collider2D, float> _obstacleContacts = new Dictionary<Collider2D, float>();
    private float _currentEnergy;
    private float _maximumEnergyMultiplier = 1f;

    #endregion

    #region Properties

    public float MaxEnergy => Mathf.Max(0f, _maxEnergy) * _maximumEnergyMultiplier;

    public float CurrentEnergy => _currentEnergy;

    public float Normalized => MaxEnergy <= 0f ? 0f : _currentEnergy / MaxEnergy;

    public bool HasEnergy => _currentEnergy > 0f;

    public bool IsTouchingObstacle => _obstacleContacts.Count > 0;

    public float CurrentDrainMultiplier => GetCurrentObstacleMultiplier();

    #endregion

    #region Events

    public event Action<float, float> EnergyChanged;
    public event Action EnergyDepleted;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (_playerMovement == null)
            _playerMovement = GetComponentInParent<PlayerMovement>();

        ResetEnergy();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryGetObstacleMultiplier(other, out float multiplier))
        {
            _obstacleContacts[other] = Mathf.Max(1f, multiplier);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _obstacleContacts.Remove(other);
    }

    #endregion

    #region Public Methods

    public void ResetForRun()
    {
        _obstacleContacts.Clear();
        ResetEnergy();
    }

    public void ResetEnergy()
    {
        _currentEnergy = MaxEnergy;
        EnergyChanged?.Invoke(_currentEnergy, MaxEnergy);
    }

    public void ApplyMaximumEnergyMultiplier(
        float _multiplier,
        bool _refillEnergy)
    {
        float previousNormalized = Normalized;
        _maximumEnergyMultiplier = SanitizeMultiplier(_multiplier);

        if (_refillEnergy)
        {
            ResetEnergy();
            return;
        }

        _currentEnergy = Mathf.Clamp(previousNormalized * MaxEnergy, 0f, MaxEnergy);
        EnergyChanged?.Invoke(_currentEnergy, MaxEnergy);
    }

    public bool ConsumeDigging(float deltaTime)
    {
        return ConsumeToolEnergy(_digDrainPerSecond, deltaTime);
    }

    public bool ConsumeToolEnergy(float energyConsumption, float deltaTime)
    {
        if (!HasEnergy)
            return false;

        if (energyConsumption <= 0f || deltaTime <= 0f)
            return true;

        float obstacleMultiplier = _playerMovement != null && _playerMovement.IsDashing
            ? 1f
            : GetCurrentObstacleMultiplier();
        Consume(energyConsumption * obstacleMultiplier * deltaTime);
        return HasEnergy;
    }

    public void Consume(float amount)
    {
        if (amount <= 0f || !HasEnergy)
            return;

        _currentEnergy = Mathf.Max(0f, _currentEnergy - amount);
        EnergyChanged?.Invoke(_currentEnergy, MaxEnergy);

        if (_currentEnergy <= 0f)
        {
            EnergyDepleted?.Invoke();
        }
    }

    #endregion

    #region Private Methods

    private bool TryGetObstacleMultiplier(Collider2D other, out float multiplier)
    {
        EnergyObstacle obstacle = other.GetComponent<EnergyObstacle>();

        if (obstacle == null && other.attachedRigidbody != null)
        {
            obstacle = other.attachedRigidbody.GetComponent<EnergyObstacle>();
        }

        if (obstacle != null)
        {
            multiplier = obstacle.EnergyDrainMultiplier;
            return true;
        }

        if (IsInLayerMask(other.gameObject.layer, _obstacleLayers))
        {
            multiplier = _defaultObstacleMultiplier;
            return true;
        }

        if (!string.IsNullOrEmpty(_obstacleTag) && other.gameObject.CompareTag(_obstacleTag))
        {
            multiplier = _defaultObstacleMultiplier;
            return true;
        }

        multiplier = 1f;
        return false;
    }

    private float GetCurrentObstacleMultiplier()
    {
        float multiplier = 1f;

        foreach (float contactMultiplier in _obstacleContacts.Values)
        {
            multiplier = Mathf.Max(multiplier, contactMultiplier);
        }

        return multiplier;
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private float SanitizeMultiplier(float _multiplier)
    {
        if (float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
            return 1f;

        return Mathf.Max(1f, _multiplier);
    }

    #endregion
}
