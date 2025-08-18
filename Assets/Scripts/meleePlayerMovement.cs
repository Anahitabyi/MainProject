using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Audio;
using Unity.Netcode;
using NUnit.Framework;

public class meleePlayerMovement : NetworkBehaviour, IPlayerInputBlocker
{
    public bool isInputBlocked { get; set; } = false;
    public Animator animator;
    bool isFacingRight = true;

    [Header("Movement")]
    float horizontalMovement;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 1;
    private int jumpsRemaining;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioMixerGroup sfxMixerGroup;


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

    public WeaponUIIndicator weaponUIIndicator;

    Rigidbody2D rb;
    private bool wasGroundedLastFrame = true;

    // ✳️ New jump buffer timer
    private float groundedTime = 0f;
    private float groundedResetThreshold = 0.04f;
    public NetworkVariable<float> magnitude2 = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
        if (audioSource != null && sfxMixerGroup != null)
    {
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }
    }

    void Update()
    {
        //if (!IsOwner) return;
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
            
                    magnitude2.Value = localMag;
                    Debug.Log("magnitude: " + magnitude2.Value);
                    animator.SetFloat("magnitude", magnitude2.Value);
                // else if (playerIdentifier.playerType == PlayerIdentifier.PlayerType.Hooded)
                // {
                //     magnitude2.Value = localMag;
                //     Debug.Log("magnitude: " + magnitude2.Value);
                //     animator.SetFloat("magnitude", magnitude2.Value);
                // }

            }

        }
        // ------------------------------------------------------------
        animator.SetBool("isJumping", !groundedNow);
        //SetVariablesServerRpc(yVel, groundedNow);

        flip();

        wasGroundedLastFrame = groundedNow;
    }
    // [ServerRpc]
    // public void SetVariablesServerRpc(float yVel, bool groundedNow)
    // {
    //     if (!IsOwner)
    //     {
    //         animator.SetFloat("Yvelocity", yVel);
    //         animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));
    //         animator.SetBool("isJumping", !groundedNow);
    //         SetVariablesClientRpc(yVel, groundedNow);
    //     }
    // }
    // [ClientRpc]
    // public void SetVariablesClientRpc(float yVel, bool groundedNow)
    // {
    //     if (!IsOwner && !IsServer)
    //     {
    //         animator.SetFloat("Yvelocity", yVel);
    //         animator.SetFloat("magnitude", Mathf.Abs(rb.linearVelocity.x));
    //         animator.SetBool("isJumping", !groundedNow);
    //     }
    // }

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
            groundedTime += Time.deltaTime;

            if (groundedTime > groundedResetThreshold)
            {
                jumpsRemaining = maxJumps;
            }
        }
        else
        {
            groundedTime = 0f;
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
            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
            // Reset grounded timer on successful jump
            groundedTime = 0f;
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

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
        }
    }
}
