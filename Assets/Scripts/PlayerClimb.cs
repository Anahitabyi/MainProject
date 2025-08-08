using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    public float climbSpeed = 4f;

    private bool isOnLadder = false;
    private bool isClimbing = false;

    private Rigidbody2D rb;
    private float originalGravity;
    private MonoBehaviour controller;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerControllerNew>() as MonoBehaviour;
        if (controller == null)
        {
            controller = GetComponent<PlayerControllerNew2>() as MonoBehaviour;
        }

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

            SetIsClimbing(false);
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
        SetIsClimbing(true);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = originalGravity;
        SetIsClimbing(false);
    }

    private void SetIsClimbing(bool value)
    {
        if (controller == null) return;

        if (controller is PlayerControllerNew c1)
            c1.IsClimbing = value;
        else if (controller is PlayerControllerNew2 c2)
            c2.IsClimbing = value;
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