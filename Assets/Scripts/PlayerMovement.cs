using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private bool canMoveHorizontal;

    private void Update()
    {
        float horizontal = 0f;

        if (canMoveHorizontal)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }

        transform.position += new Vector3(horizontal * moveSpeed * Time.deltaTime, 0f, 0f);
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