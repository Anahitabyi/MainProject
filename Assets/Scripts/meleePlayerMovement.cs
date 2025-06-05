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

    [Header("Input Blocking")]
    public bool isInputBlocked = false;

    public WeaponUIIndicator weaponUIIndicator;
    private bool wasGroundedLastFrame = true;
    private bool wasFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
{
    if (isInputBlocked)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetFloat("Yvelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", 0);
        return;
    }

    rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);

    float yVel = rb.linearVelocity.y;
    bool groundedNow = isGrounded();

    // Set jump animation when leaving ground and moving upward
    if (wasGroundedLastFrame && !groundedNow && yVel > 0.1f)
    {
        animator.SetTrigger("jump");
    }

    // Trigger "falling" animation as soon as falling begins
    if (!groundedNow && yVel < -0.1f && !wasFalling)
    {
        animator.SetTrigger("land");
    }

    // Update animator parameters
    animator.SetFloat("Yvelocity", yVel);
    animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

    flip();

    // Update states
    wasGroundedLastFrame = groundedNow;
    wasFalling = yVel < -0.1f && !groundedNow;

    // Reset jumps if grounded
    if (groundedNow)
    {
        jumpsRemaining = maxJumps;
    }
}


    public void Move(InputAction.CallbackContext context)
    {
        if (isInputBlocked) return;
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isInputBlocked) return;

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
        if (isInputBlocked) return;

        if (context.performed)
        {
            animator.SetTrigger("meleeAttack");
        }
    }

    public void PerformAttack()
    {
        if (isInputBlocked) return;

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
        if (isInputBlocked) return;

        if ((isFacingRight && horizontalMovement < 0) || (!isFacingRight && horizontalMovement > 0))
        {
            isFacingRight = !isFacingRight;

            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;

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
