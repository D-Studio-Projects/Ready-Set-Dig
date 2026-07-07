using UnityEngine;

public class Drill : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private float radius = .4f;

    private PlayerEnergy energy;
    private PlayerMovement movement;

    private void Awake()
    {
        energy = GetComponentInParent<PlayerEnergy>();
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        if (terrain == null || energy == null || movement == null)
            return;

        if (!movement.IsMoving || !energy.HasEnergy)
            return;

        int dugCells = terrain.Dig(transform.position, radius);

        if (dugCells > 0)
        {
            energy.ConsumeDigging(Time.deltaTime);
        }
    }
}
