using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float digDrainPerSecond = 8f;

    [Header("Obstacle Drain")]
    [SerializeField] private float defaultObstacleMultiplier = 2f;
    [SerializeField] private string obstacleTag = "Obstacle";
    [SerializeField] private LayerMask obstacleLayers;

    private readonly Dictionary<Collider2D, float> obstacleContacts = new Dictionary<Collider2D, float>();
    private float currentEnergy;

    public event Action<float, float> EnergyChanged;
    public event Action EnergyDepleted;

    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float Normalized => maxEnergy <= 0f ? 0f : currentEnergy / maxEnergy;
    public bool HasEnergy => currentEnergy > 0f;
    public bool IsTouchingObstacle => obstacleContacts.Count > 0;
    public float CurrentDrainMultiplier => GetCurrentObstacleMultiplier();

    private void Awake()
    {
        ResetEnergy();
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        EnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public bool ConsumeDigging(float deltaTime)
    {
        if (!HasEnergy)
            return false;

        Consume(digDrainPerSecond * GetCurrentObstacleMultiplier() * deltaTime);
        return HasEnergy;
    }

    public void Consume(float amount)
    {
        if (amount <= 0f || !HasEnergy)
            return;

        currentEnergy = Mathf.Max(0f, currentEnergy - amount);
        EnergyChanged?.Invoke(currentEnergy, maxEnergy);

        if (currentEnergy <= 0f)
        {
            EnergyDepleted?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryGetObstacleMultiplier(other, out float multiplier))
        {
            obstacleContacts[other] = Mathf.Max(1f, multiplier);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        obstacleContacts.Remove(other);
    }

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

        if (IsInLayerMask(other.gameObject.layer, obstacleLayers))
        {
            multiplier = defaultObstacleMultiplier;
            return true;
        }

        if (!string.IsNullOrEmpty(obstacleTag) && other.gameObject.tag == obstacleTag)
        {
            multiplier = defaultObstacleMultiplier;
            return true;
        }

        multiplier = 1f;
        return false;
    }

    private float GetCurrentObstacleMultiplier()
    {
        float multiplier = 1f;

        foreach (float contactMultiplier in obstacleContacts.Values)
        {
            multiplier = Mathf.Max(multiplier, contactMultiplier);
        }

        return multiplier;
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
