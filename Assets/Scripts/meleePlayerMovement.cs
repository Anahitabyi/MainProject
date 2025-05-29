using UnityEngine;
using UnityEngine.InputSystem;

public class meleePlayerMovement : MonoBehaviour
{
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    float horizontalMovement;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 1;
    private int jumpsRemaining;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSizev = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);
        GroundCheck();

        animator.SetFloat("Yvelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

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
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            animator.SetTrigger("jump");
            jumpsRemaining--;
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void MeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetTrigger("meleeAttack"); // Start animation
        }
    }

    // Called by animation event
    public void PerformAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, 0f, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void flip()
    {
        if ((isFacingRight && horizontalMovement < 0) || (!isFacingRight && horizontalMovement > 0))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;

            // Flip attack point
            if (attackPoint != null)
            {
                Vector3 localPos = attackPoint.localPosition;
                localPos.x *= -1f;
                attackPoint.localPosition = localPos;
            }
        }
    }

    private bool isGrounded()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSizev, 0f, groundLayer);
        return hit != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSizev);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
        }
    }
}
