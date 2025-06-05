using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    [Header("Input Blocking")]
    public bool isInputBlocked = false;

    public WeaponUIIndicator weaponUIIndicator;

    Rigidbody2D rb;
    private bool wasGroundedLastFrame = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (isInputBlocked)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetFloat("Yvelocity", rb.linearVelocity.y);
            animator.SetFloat("magnitude", 0);
            animator.SetBool("isJumping", false);
            return;
        }

        rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);

        GroundCheck();
        ApplyGravity();

        float yVel = rb.linearVelocity.y;
        bool groundedNow = isGrounded();

        animator.SetFloat("Yvelocity", yVel);
        animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

        // NEW: Update isJumping parameter
        animator.SetBool("isJumping", !groundedNow);

        flip();

        wasGroundedLastFrame = groundedNow;
    }

    private void ApplyGravity()
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
