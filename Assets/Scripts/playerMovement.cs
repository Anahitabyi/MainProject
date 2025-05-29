using UnityEngine;
using UnityEngine.InputSystem;
public class playerMovement : MonoBehaviour {
    public Animator animator;
    bool isFacingRight = true;
    [Header("Movement")]
    float horizontalMovement;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2; // Total number of jumps (1 = single, 2 = double)
    private int jumpsRemaining; // Current jumps left

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSizev = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;
    Rigidbody2D rb;
    private bool wasGroundedLastFrame = true;
    private bool wasFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }
    void Update()
{
    // Apply horizontal movement
    rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);

    // Handle jump reset
    GroundCheck();

    // Apply gravity modifications
    Gravity();

    float yVel = rb.linearVelocity.y;
    bool groundedNow = isGrounded();

    // Set falling flag (can use a falling animation based on Y velocity)
    if (yVel < -0.1f)
    {
        animator.SetBool("falling", true);
    }
    else
    {
        animator.SetBool("falling", false);
    }

    // Pre-landing raycast check
    float preLandDistance = 0.3f; // How early you want to trigger "land"
    bool nearGround = false;

    if (!groundedNow && yVel < -0.5f)
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos.position, Vector2.down, preLandDistance, groundLayer);
        if (hit.collider != null)
        {
            nearGround = true;
        }
    }

    // Trigger "land" before hitting the ground
    if (nearGround && !wasFalling)
    {
        animator.SetTrigger("land");
        wasFalling = true;
    }

    // Reset fall flag once grounded
    if (groundedNow)
    {
        wasFalling = false;
    }

    // Update animator floats
    animator.SetFloat("Yvelocity", yVel);
    animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

    // Flip sprite
    flip();

    // Update last frame ground status
    wasGroundedLastFrame = groundedNow;
}


    private void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }
private void GroundCheck()
{
    if (isGrounded())
    {
        jumpsRemaining = maxJumps;
    }
}


    public void Move(InputAction.CallbackContext context)
    {
        //moveInput = contex.ReadValue<Vector2>();
        horizontalMovement = context.ReadValue<Vector2>().x;
        //Debug.Log("Move Called: " + horizontalMovement);
    }
    public void Jump(InputAction.CallbackContext contex)
{
    if (contex.performed && jumpsRemaining > 0)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        animator.SetTrigger("jump");
        jumpsRemaining--;
    }
    else if (contex.canceled)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }
}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        groundCheckSizev = new Vector2(0.5f, 0.1f);
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSizev);
    }
    private bool isGrounded()
{
    Vector2 pos = groundCheckPos.position;
    Vector2 size = groundCheckSizev;

    Collider2D hit = Physics2D.OverlapBox(pos, size, 0f, groundLayer);
    
    if (hit != null)
    {
        return true;
    }
    else
    {
        return false;
    }
}
    public void flip()
{
    if ((isFacingRight && horizontalMovement < 0) || (!isFacingRight && horizontalMovement > 0))
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }
}

}

