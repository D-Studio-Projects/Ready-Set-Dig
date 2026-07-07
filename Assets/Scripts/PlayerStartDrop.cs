using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStartDrop : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Force")]
    public float maxImpulse = 15f;

    [Header("Timing bar")]
    public float speed = 2f;

    private PlayerMovement movement;
    private float charge;
    private float dir = 1f;
    private bool hasStarted;

    public Slider chargeBar;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (hasStarted)
            return;

        HandleBar();

        if (chargeBar != null)
        {
            chargeBar.value = charge;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDrop();
        }
    }

    private void HandleBar()
    {
        charge += dir * speed * Time.deltaTime;

        if (charge >= 1f)
        {
            charge = 1f;
            dir = -1f;
        }
        else if (charge <= 0f)
        {
            charge = 0f;
            dir = 1f;
        }
    }

    private void StartDrop()
    {
        hasStarted = true;
        movement.StartRun(charge);

        if (chargeBar != null)
        {
            chargeBar.gameObject.SetActive(false);
        }
    }
}
