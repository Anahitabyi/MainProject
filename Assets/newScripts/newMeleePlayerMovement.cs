using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class newMeleePlayerMovement : MonoBehaviour, IPlayerInputBlocker
{
    public bool isInputBlocked { get; set; } = false;

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
        animator.SetFloat("magnitude", moveInput.magnitude);
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
        if (isInputBlocked) return;

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
                shooter.bossRef.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
    
}
