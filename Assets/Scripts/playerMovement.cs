using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour
{
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    float horizontalMovement;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    private int jumpsRemaining;

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

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float bulletSpawnDelay = 0.2f;

    public Camera hobbitCamera;
    public float shakeDuration;
    public float shakeMagnitude;

    [Header("Attack")]
    public int attackDamage = 1;

    public WeaponUIIndicator weaponUIIndicator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps; // ✅ Fix: Ensure jump is initialized
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * movementSpeed, rb.linearVelocity.y);

        GroundCheck();
        Gravity();

        float yVel = rb.linearVelocity.y;
        bool groundedNow = isGrounded();

        if (wasGroundedLastFrame && !groundedNow && yVel > 0.1f)
        {
            animator.SetTrigger("jump");
        }

        if (!wasGroundedLastFrame && groundedNow && wasFalling)
        {
            animator.SetTrigger("falling");
            Debug.Log("Landing triggered");
        }

        animator.SetFloat("Yvelocity", yVel);
        animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

        flip();

        wasGroundedLastFrame = groundedNow;
        wasFalling = yVel < -0.1f && !groundedNow;
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
            jumpsRemaining = maxJumps; // Reset jumps when grounded
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
            jumpsRemaining--;
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSizev);
    }

    private bool isGrounded()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSizev, 0f, groundLayer);
        return hit != null;
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

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        animator.SetTrigger("shoot");
        StartCoroutine(DelayedBulletSpawn());
    }

    private IEnumerator DelayedBulletSpawn()
    {
        yield return new WaitForSeconds(bulletSpawnDelay);

        Vector3 mousePosition = hobbitCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = attackDamage;
        }

        StartCoroutine(ShakeCamera());
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = hobbitCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            hobbitCamera.transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        hobbitCamera.transform.position = originalPos;
    }

    public void DebugRightClick(InputAction.CallbackContext context)
    {
        Debug.Log("Right click detected");
    }
}
