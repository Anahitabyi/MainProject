using UnityEngine;
using System.Collections.Generic;

public class BossEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Attack")]
    public float timeToSwitchTarget = 10f;
    public float attackRange = 5f;
    private bool isAttacking = false;
    private bool isSpawning = false;
    private bool minionsTargetPlayer1 = true;

    [Header("Attack Delay")]
    public float attackStartDelay = 3f; // Delay after alert animation
    private bool canStartAttacking = false;

    [Header("Minion Spawning")]
    public GameObject minionPrefab1;
    public GameObject minionPrefab2;
    public Transform[] spawnPoints;
    public int attacksPerMinionSpawn = 3;
    private int attackCount = 0;

    [Header("Animation")]
    public Animator animator;
    public string attackAnimationName = "Attack";
    public string spawnAnimationName = "SpawnMinions";
    public string deathAnimationName = "Death";

    [Header("Shooter Device")]
    public BossShooterDevice shooterDevice;

    private bool isDead = false;

    private HashSet<Transform> damagedPlayersThisWave = new HashSet<Transform>();

    void Start()
    {
        currentHealth = maxHealth;
        shooterDevice.bossRef = this; // Register boss in device
    }

    public void StartAttackWithDelay()
    {
        Invoke(nameof(EnableAttacking), attackStartDelay);
    }

    void EnableAttacking()
    {
        canStartAttacking = true;
    }

    void Update()
    {
        if (isDead || !canStartAttacking)
            return;

        HandleAttacking();
    }

    void HandleAttacking()
    {
        Transform bulletTarget = minionsTargetPlayer1 ? player2 : player1;
        float distance = Vector2.Distance(transform.position, bulletTarget.position);

        if (!isAttacking && !isSpawning && distance <= attackRange)
        {
            isAttacking = true;

            if (animator != null)
                animator.SetTrigger(attackAnimationName);

        }
    }

    // 🔔 Called in the middle of the boss attack animation
    public void TriggerShooter()
    {
        if (shooterDevice != null)
        {
            shooterDevice.currentTarget = minionsTargetPlayer1 ? player2 : player1;
            damagedPlayersThisWave.Clear();
            shooterDevice.TriggerAttack();

        }
    }

    // 🔔 Called during the boss attack animation
    public void SpawnMinions()
    {
        Transform minionTarget = minionsTargetPlayer1 ? player1 : player2;
        GameObject[] playerObjects = { player1.gameObject, player2.gameObject };

        bool useFirstMinion = true;
        foreach (Transform point in spawnPoints)
        {
            GameObject chosenPrefab = useFirstMinion ? minionPrefab1 : minionPrefab2;
            useFirstMinion = !useFirstMinion;

            GameObject minion = Instantiate(chosenPrefab, point.position, Quaternion.identity);
            MinionEnemy minionScript = minion.GetComponent<MinionEnemy>();
            if (minionScript != null)
            {
                minionScript.SetPlayers(playerObjects, minionTarget.gameObject);
            }
        }
    }

    // 🔔 Called at the end of boss attack animation
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        attackCount++;

        if (attackCount >= attacksPerMinionSpawn)
        {
            attackCount = 0;
            isSpawning = true;
            animator.SetTrigger(spawnAnimationName);
        }
    }

    public bool Registerdamage(Transform player)
    {
        if (!damagedPlayersThisWave.Contains(player))
        {
            damagedPlayersThisWave.Add(player);
            return true;
        }
        return false;
    }

    // 🔔 Called at the end of spawn animation
    public void OnSpawnAnimationEnd()
    {
        isSpawning = false;
        isAttacking = false;
        minionsTargetPlayer1 = !minionsTargetPlayer1;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        //Debug.Log("boss health : " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        isAttacking = false;
        isSpawning = false;
        if (animator != null)
            animator.SetTrigger(deathAnimationName);
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
