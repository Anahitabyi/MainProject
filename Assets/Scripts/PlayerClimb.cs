using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    public float climbSpeed = 4f;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private Rigidbody2D rb;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isOnLadder)
        {
            float vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(vertical) > 0.1f)
            {
                isClimbing = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * climbSpeed);
                rb.gravityScale = 0f;
            }
            else if (isClimbing)
            {
                // Stop movement when key released
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }
        else
        {
            if (isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = originalGravity;
            }
        }
    }

    public void SetOnLadder(bool value)
    {
        isOnLadder = value;

        if (!value && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = originalGravity;
        }
    }
}