using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using Unity.Netcode;

public class newMeleePlayerMovement : NetworkBehaviour, IPlayerInputBlocker
{
    public bool isInputBlocked { get; set; } = false;
    public NetworkVariable<bool> isInputBlockedNet { get; } = new NetworkVariable<bool>();

    public Animator animator;

    [Header("Movement")]
    Vector2 moveInput;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Attack")]
    public Transform attackPoint;
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    public WeaponUIIndicator weaponUIIndicator;
    public NetworkVariable<float> magnitude = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (audioSource != null && sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    void Update()
    {
        if (isInputBlocked)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("magnitude", 0);
            return;
        }

        // Update movement
        rb.linearVelocity = moveInput * movementSpeed;
        // Flip the sprite based on horizontal movement
        // Flip sprite without overriding original scale
        Vector3 localScale = transform.localScale;

        if (moveInput.x > 0.01f)
        {
            localScale.x = Mathf.Abs(localScale.x); // Ensure it's positive
            transform.localScale = localScale;
        }
        else if (moveInput.x < -0.01f)
        {
            localScale.x = -Mathf.Abs(localScale.x); // Flip X
            transform.localScale = localScale;
        }


        // Animator control for movement magnitude
        //animator.SetFloat("magnitude", moveInput.magnitude);
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            animator.SetFloat("magnitude", moveInput.magnitude);
            //Debug.Log("offline!");
        }
        else
        {
            if (IsOwner)
            {
                    magnitude.Value = moveInput.magnitude;
                    //Debug.Log("magnitude: " + magnitude1.Value);
                    animator.SetFloat("magnitude", magnitude.Value);
                
                // else if (playerIdentifier.playerType == PlayerIdentifier.PlayerType.Hooded)
                // {
                //     magnitude2.Value = localMag;
                //     Debug.Log("magnitude: " + magnitude2.Value);
                //     animator.SetFloat("magnitude", magnitude2.Value);
                // }

            }

        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isInputBlocked) return;
        moveInput = context.ReadValue<Vector2>();  // Get movement input
        //Debug.Log("Move Input: " + moveInput);
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
        if (!IsOwner) return;
        // Only the owner can trigger the attack
        if (NetworkManager.Singleton != null && !GetComponent<NetworkObject>().IsOwner) 
            return;

        AttackServerRpc();
    }
    [ServerRpc]
    private void AttackServerRpc(ServerRpcParams rpcParams = default)
    {
        // Detect enemies locally on the server
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, 0f, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();


            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }

            BossShooterDevice shooter = enemy.GetComponent<BossShooterDevice>();
            if (shooter != null && shooter.bossRef != null)
            {
                shooter.TakeDamage(attackDamage);
                return;
            }
        }

        // Notify all clients to play attack animation/effects
        //AttackClientRpc();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
    
}
