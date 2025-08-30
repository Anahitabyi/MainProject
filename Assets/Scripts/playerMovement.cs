using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using NUnit.Framework;

public class playerMovement : NetworkBehaviour, IPlayerInputBlocker
{
    public bool isInputBlocked { get; set; } = false;
    public Animator animator;
    bool isFacingRight = true;
    private PlayerIdentifier playerIdentifier;

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
    [Header("Network")]
    public NetworkVariable<float> magnitude1 = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // Find the local Cinemachine/Camera on THIS client
            hobbitCamera = Camera.main; 
            // Or if you use multiple CinemachineCameras:
            // hobbitCamera = FindObjectOfType<CinemachineCamera>().GetComponent<Camera>();
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
        playerIdentifier = GetComponent<PlayerIdentifier>();
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

        animator.SetFloat("Yvelocity", yVel);

         // -------------------- MAGNITUDE HANDLING --------------------
        float localMag = Mathf.Abs(rb.linearVelocity.x);
        // Offline mode → just update directly

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            animator.SetFloat("magnitude", localMag);
            //Debug.Log("offline!");
        }
        else
        {
            if (IsOwner)
            {
                if (playerIdentifier.playerType == PlayerIdentifier.PlayerType.Hobbit)
                {
                    magnitude1.Value = localMag;
                    Debug.Log("magnitude: " + magnitude1.Value);
                    animator.SetFloat("magnitude", magnitude1.Value);
                }
                // else if (playerIdentifier.playerType == PlayerIdentifier.PlayerType.Hooded)
                // {
                //     magnitude2.Value = localMag;
                //     Debug.Log("magnitude: " + magnitude2.Value);
                //     animator.SetFloat("magnitude", magnitude2.Value);
                // }

            }

        }
        // ------------------------------------------------------------

        flip();

        wasFalling = yVel < fallThreshold && !groundedNow;

        if (wasFalling)
            animator.SetBool("isJumping", false);

        // Update ground time
        if (groundedNow)
            groundTime += Time.deltaTime;
        else
            groundTime = 0f;
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