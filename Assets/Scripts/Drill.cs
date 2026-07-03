using UnityEngine;

public class Drill : MonoBehaviour
{
    [SerializeField] private Terrain terrain;

    [SerializeField] private float radius = .4f;

    void Update()
    {
            terrain.Dig(transform.position, radius);
    }
}