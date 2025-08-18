using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovement : NetworkBehaviour, IPlayerInputBlocker
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

    [Header("Jump Buffering")]
    private float groundTime = 0f;
    private float minGroundTime = 0.1f;

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
    private GameObject activeBullet = null;
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
        if(!IsOwner) return;
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

        animator.SetFloat("Yvelocity", yVel);
        animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));
        //animator.SetBool("isJumping", !groundedNow);
        SetFloatsServerRpc(yVel, groundedNow);

        flip();

        wasFalling = yVel < fallThreshold && !groundedNow;

        if (wasFalling)
            animator.SetBool("isJumping", false);
            SetIsJumpingServerRpc(groundedNow);

        // Update ground time
        if (groundedNow)
            groundTime += Time.deltaTime;
        else
            groundTime = 0f;
    }
    [ServerRpc]
    public void SetFloatsServerRpc(float yVel, bool groundedNow)
    {
        if (!IsOwner)
        {
            animator.SetFloat("Yvelocity", yVel);
            animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));
            //animator.SetBool("isJumping", !groundedNow);
            SetFloatsClientRpc(yVel, groundedNow);
        }
    }
     [ServerRpc]
    private void SetIsJumpingServerRpc(bool groundedNow)
    {
        if (!IsOwner)
        {
            animator.SetBool("isJumping", !groundedNow);
            SetIsJumpingClientRpc(groundedNow);
        }
    }
    [ClientRpc]
    public void SetFloatsClientRpc(float yVel, bool groundedNow)
    {
        if (!IsOwner && !IsServer)
        {
            animator.SetFloat("Yvelocity", yVel);
            animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));
            //animator.SetBool("isJumping", !groundedNow);
        }
    }
    [ClientRpc]
    private void SetIsJumpingClientRpc(bool groundedNow)
    {
        if (!IsOwner && !IsServer)
        {
            animator.SetBool("isJumping", !groundedNow);
        }
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
            animator.SetBool("isJumping", false);
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

        if (context.performed)
        {
            if (groundTime > minGroundTime)
            {
                // Grounded jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining = maxJumps - 1; // Use one jump now
                groundTime = 0f;

                animator.SetTrigger("jump");
                animator.SetBool("isJumping", true);
            }
            else if (jumpsRemaining > 0)
            {
                // Air jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining--;

                animator.SetTrigger("jump");
                animator.SetBool("isJumping", true);
            }
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private bool isGrounded()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(groundCheckPos.position, groundCheckSizev, 0f, groundLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.gameObject != this.gameObject && hit.transform.root != transform)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSizev);
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

        if (activeBullet != null) yield break;

        Vector3 mousePosition = hobbitCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        activeBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = activeBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        Bullet bulletScript = activeBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = attackDamage;
            bulletScript.OnDestroyed += HandleBulletDestroyed;
        }

        StartCoroutine(ShakeCamera());
    }

    private void HandleBulletDestroyed()
    {
        activeBullet = null;
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