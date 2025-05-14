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
        rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed,  rb.linearVelocity.y);

        animator.SetFloat("Yvelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        flip();
        
    }
    public void Move(InputAction.CallbackContext context)
    {
        //moveInput = contex.ReadValue<Vector2>();
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
    public void Jump(InputAction.CallbackContext contex)
    {
        if(isGrounded()) {
            if(contex.performed) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                animator.SetTrigger("jump");
            }
            else if(contex.canceled){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                animator.SetTrigger("jump");
            }
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

