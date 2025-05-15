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
    
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }
    void Update()
{
    // Apply horizontal movement
    rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);

    // Handle jump reset in a separate method
    GroundCheck();

    // Update animator parameters
    animator.SetFloat("Yvelocity", rb.linearVelocity.y);
    animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x)); // Use Abs to avoid negative magnitude

    // Flip character direction
    flip();
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

