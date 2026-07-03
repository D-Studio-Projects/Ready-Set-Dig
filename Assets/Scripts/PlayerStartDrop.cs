using UnityEngine;
using UnityEngine.UI;
public class PlayerStartDrop : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Force")]
    public float maxImpulse = 15f;

    [Header("Timing bar")]
    public float speed = 2f;

    private float charge;
    private float dir = 1f;
    private bool hasStarted;
    public Slider chargeBar;

    void Update()
    {
        if (!hasStarted)
        {
            HandleBar();

            chargeBar.value = charge;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartDrop();
            }
        }
    }

    void HandleBar()
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

    void StartDrop()
    {
        rb.gravityScale = 0.15f;
        hasStarted = true;

        float impulse = charge * maxImpulse;

        rb.AddForce(Vector2.down * impulse, ForceMode2D.Impulse);
        chargeBar.enabled = false;
    }

}