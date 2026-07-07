using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Vertical Movement")]
    [SerializeField] private float baseFallSpeed = 6f;
    [SerializeField] private float maxFallSpeed = 14f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float stopDeceleration = 18f;

    [Header("Curved Horizontal Movement")]
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float horizontalAcceleration = 10f;
    [SerializeField] private float curveTiltDegrees = 20f;
    [SerializeField] private float rotationSmoothing = 10f;

    private Rigidbody2D rb;
    private PlayerEnergy energy;
    private bool hasStarted;
    private bool canMoveHorizontal;
    private float currentFallSpeed;
    private float currentHorizontalSpeed;
    private float input;

    public float CurrentSpeed => rb == null ? 0f : rb.linearVelocity.magnitude;
    public bool IsMoving => hasStarted && energy != null && energy.HasEnergy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        energy = GetComponent<PlayerEnergy>();
        rb.gravityScale = 0f;
    }

    private void OnEnable()
    {
        if (energy != null)
        {
            energy.EnergyDepleted += StopMovement;
        }
    }

    private void OnDisable()
    {
        if (energy != null)
        {
            energy.EnergyDepleted -= StopMovement;
        }
    }

    private void Update()
    {
        input = 0f;

        if (IsMoving && canMoveHorizontal)
        {
            input = Input.GetAxisRaw("Horizontal");
        }
    }

    private void FixedUpdate()
    {
        if (!hasStarted)
            return;

        if (energy == null || !energy.HasEnergy)
        {
            DecelerateToStop();
            return;
        }

        currentFallSpeed = Mathf.MoveTowards(
            currentFallSpeed,
            maxFallSpeed,
            acceleration * Time.fixedDeltaTime
        );

        currentHorizontalSpeed = Mathf.MoveTowards(
            currentHorizontalSpeed,
            input * horizontalSpeed,
            horizontalAcceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(currentHorizontalSpeed, -currentFallSpeed);
        ApplyCurvedRotation();
    }

    public void StartRun(float launchStrength)
    {
        hasStarted = true;
        canMoveHorizontal = true;
        currentFallSpeed = Mathf.Lerp(baseFallSpeed, maxFallSpeed, Mathf.Clamp01(launchStrength));
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * currentFallSpeed;
    }

    private void DecelerateToStop()
    {
        currentFallSpeed = Mathf.MoveTowards(currentFallSpeed, 0f, stopDeceleration * Time.fixedDeltaTime);
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0f, stopDeceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentHorizontalSpeed, -currentFallSpeed);
        ApplyCurvedRotation();
    }

    private void StopMovement()
    {
        currentFallSpeed = 0f;
        currentHorizontalSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void ApplyCurvedRotation()
    {
        float targetZ = -Mathf.Sign(currentHorizontalSpeed) * curveTiltDegrees;

        if (Mathf.Abs(currentHorizontalSpeed) < 0.05f)
        {
            targetZ = 0f;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSmoothing * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            canMoveHorizontal = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            canMoveHorizontal = false;
        }
    }
}
