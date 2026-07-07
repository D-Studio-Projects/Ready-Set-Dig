using UnityEngine;

public class EnergyObstacle : MonoBehaviour
{
    [SerializeField] private float energyDrainMultiplier = 2f;

    public float EnergyDrainMultiplier => Mathf.Max(1f, energyDrainMultiplier);
}
