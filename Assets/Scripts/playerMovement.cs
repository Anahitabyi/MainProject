using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : MonoBehaviour, IPlayerInputBlocker
{
    public bool isInputBlocked { get; set; } = false;
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    float horizontalMovement;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    private int jumpsRemaining;
    private const float fallThreshold = -0.2f;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSizev = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    Rigidbody2D rb;
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
        jumpsRemaining = maxJumps;
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

        GroundCheck();
        Gravity();

        float yVel = rb.linearVelocity.y;
        bool groundedNow = isGrounded();

        // if (groundedNow && wasFalling)
        // {
        //     animator.SetTrigger("falling");
        // }

        animator.SetFloat("Yvelocity", yVel);
        animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));

        flip();

        wasFalling = yVel < fallThreshold && !groundedNow;

        // Reset jump bool when falling starts
        if (wasFalling)
            animator.SetBool("isJumping", false);
        
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
        bool groundedNow = isGrounded();

        if (groundedNow)
        {
            if (wasFalling)
            {
                animator.SetTrigger("falling");
            }
            animator.SetBool("isJumping", false); // Reset jump state
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
            jumpsRemaining--;

            animator.SetTrigger("jump"); // ✅ Animation only on actual jump input
            animator.SetBool("isJumping", true);
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
        if (isInputBlocked) return;

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
        if (isInputBlocked || !context.performed) return;

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
