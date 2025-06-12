using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Players")]
    public Transform player1;
    public Transform player2;
    private Transform currentTarget;

    [Header("Attack")]
    public float timeToSwitchTarget = 10f;
    public float attackRange = 5f;
    public int attackDamage = 10;

    [Header("Minion Spawning")]
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 20f;

    [Header("Animation")]
    public Animator animator;
    public string attackAnimationName = "Attack";
    public string spawnAnimationName = "SpawnMinions";
    public string deathAnimationName = "Death";

    [Header("UI")]
    //public BossHealthBar healthBar; // ← Hook for health bar UI

    private float switchTimer;
    private float spawnTimer;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentTarget = player1;
        switchTimer = timeToSwitchTarget;
        spawnTimer = spawnInterval;

        //healthBar?.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        HandleTargetSwitching();
        HandleMinionSpawning();
        HandleAttacking();
    }

    void HandleTargetSwitching()
    {
        switchTimer -= Time.deltaTime;
        if (switchTimer <= 0f)
        {
            currentTarget = (currentTarget == player1) ? player2 : player1;
            switchTimer = timeToSwitchTarget;
        }
    }

    void HandleMinionSpawning()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            animator.SetTrigger(spawnAnimationName); // Play spawn animation
            foreach (Transform point in spawnPoints)
            {
                Instantiate(minionPrefab, point.position, Quaternion.identity);
            }
            spawnTimer = spawnInterval;
        }
    }

    void HandleAttacking()
    {
        if (Vector2.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            animator.SetTrigger(attackAnimationName); // Play attack animation
            // Damage logic can go here or be triggered by animation event
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        //healthBar?.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger(deathAnimationName);
        // Optional: destroy after animation
        Destroy(gameObject, 2f);
    }
}
