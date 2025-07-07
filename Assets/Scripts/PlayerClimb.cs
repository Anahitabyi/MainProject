using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    public float climbSpeed = 4f;

    private bool isOnLadder = false;
    private bool isClimbing = false;

    private Rigidbody2D rb;
    private float originalGravity;
    private PlayerControllerNew controller;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerControllerNew>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (!isOnLadder)
        {
            if (isClimbing)
            {
                StopClimbing();
            }
            if(controller != null){controller.IsClimbing = false;}
            
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(vertical) > 0.1f)
        {
            if (!isClimbing)
            {
                StartClimbing();
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * climbSpeed);
        }
        else if (isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        if(controller!= null)
        controller.IsClimbing = true;
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = originalGravity;
        if(controller!=null)
        controller.IsClimbing = false;
    }

    public void SetOnLadder(bool value)
    {
        isOnLadder = value;

        if (!value && isClimbing)
        {
            StopClimbing();
        }
    }
}
