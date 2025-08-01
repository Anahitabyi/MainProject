using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using System.Collections;
//using System.Numerics;
    #if UNITY_EDITOR
using UnityEditor;
#endif
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

    private bool isCloseAttacking = false;
    private bool minionsTargetPlayer1 = true;

    [Header("Attack Delay")]
    public float attackStartDelay = 3f; // Delay after alert animation
    private bool canStartAttacking = false;

    [Header("Minion Spawning")]
    public GameObject minionPrefab1;
    public GameObject minionPrefab2;
    //public Transform[] spawnPoints;
    public int attacksPerMinionSpawn = 3;

    public enum SpawnShape { Line, Circle, Zigzag }
    [Header("Minion Spawn Settings")]
    public SpawnShape spawnShape = SpawnShape.Line;
    public int minionCount = 6;
    public float spawnRadius = 5f; // used for circle and zigzag
    public Vector3 spawnCenterOffset = new Vector3(0, 0, 0); // from boss position

    private int attackCount = 0;
    [Header("Close Attack")]
    public int attacksPerCloseAttacks = 10;
    public float currentSpeed = 10;
    public float maxSpeed = 10f;
    public float acceleration = 10f;
    //public Transform clostAttackTarget;
    private Vector3 startPosition;
    public float inAttackWaitingTime = 5f;
    public List<Transform> bossCloseAttackPoints = new List<Transform>();
    public GameObject warningPrefab;


    [Header("Animation")]
    public Animator animator;
    public string attackAnimationName = "Attack";
    public string spawnAnimationName = "SpawnMinions";
    public string deathAnimationName = "Death";
    public string closeAttackAnimationName = "CloseAttack";
    public string hurtAnimationName = "Hurt";

    [Header("Shooter Device")]
    public BossShooterDevice shooterDevice;

    private bool isDead = false;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup;
    public BossSFX sfx;
    [Header("Timer")]
    public float timer = 0f;
    //sbool isWaiting = false;


    private HashSet<Transform> damagedPlayersThisWave = new HashSet<Transform>();
    public BossBulletHell bulletHell;


    void Start()
    {
        currentHealth = maxHealth;
        shooterDevice.bossRef = this; // Register boss in device
        audioSource = GetComponent<AudioSource>();
        sfx = GetComponent<BossSFX>();
        startPosition = transform.position;
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
        if (isDead || !canStartAttacking) //|| //isCloseAttacking)
        return; // ⛔ Block anything during close attack

        HandleAttacking();
        // if (isCloseAttacking)
        // {
        //     CloseAttack();
        // }
    }

    void HandleAttacking()
    {
        Transform bulletTarget = minionsTargetPlayer1 ? player2 : player1;
        float distance = Vector2.Distance(transform.position, bulletTarget.position);

        if (!isAttacking && !isSpawning && !isCloseAttacking && distance <= attackRange)
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

        sfx.PlaySpawnSound();
        Transform minionTarget = minionsTargetPlayer1 ? player1 : player2;
        GameObject[] playerObjects = { player1.gameObject, player2.gameObject };

        bool useFirstMinion = true;
        List<Vector3> spawnPositions = GenerateSpawnPositions();

        foreach (Vector3 pos in spawnPositions)
        {
            GameObject chosenPrefab = useFirstMinion ? minionPrefab1 : minionPrefab2;
            useFirstMinion = !useFirstMinion;

            GameObject minion = Instantiate(chosenPrefab, pos, Quaternion.identity);
            MinionEnemy minionScript = minion.GetComponent<MinionEnemy>();
            if (minionScript != null)
            {
                minionScript.SetPlayers(new[] { player1.gameObject, player2.gameObject }, minionsTargetPlayer1 ? player1.gameObject : player2.gameObject);
            }
        }

    }

    // 🔔 Called at the end of boss attack animation
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        attackCount++;

        if (attackCount % attacksPerMinionSpawn == 0 && attackCount % attacksPerCloseAttacks != 0)
        {
            isSpawning = true;
            StartCoroutine(DelayedMinionSpawn(3f)); // 2 seconds delay before triggering spawn animation
        }

        //if (attackCount % attacksPerCloseAttacks == 0)
        // {

        //     //StartCoroutine(DelayedCloseAttack(3));
        //     isCloseAttacking = true;
        //     CloseAttack();
        //     // animator.SetTrigger(closeAttackAnimationName);
        // }
    }
    private IEnumerator DelayedMinionSpawn(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            animator.SetTrigger(spawnAnimationName); // This will call SpawnMinions() via animation event
        }

    // public void CloseAttack()
    // {

    //     StartCoroutine(CloseAttackCoroutine());

    // }
    // private IEnumerator CloseAttackCoroutine()
    // {
    //     isCloseAttacking = true;
    //     int index = UnityEngine.Random.Range(0, bossCloseAttackPoints.Count);
    //     Transform randomElement = bossCloseAttackPoints[index];
    //     Vector3 targetPosition = randomElement.position;
    //      // Spawn danger warning BEFORE moving
    //     GameObject warning = Instantiate(warningPrefab, targetPosition, Quaternion.identity);
    //     Destroy(warning, 4f); // Automatically destroy after 2 seconds (optional)

    // // Wait before boss starts moving — this gives players time to react
    //     yield return new WaitForSeconds(2f);

    //     if (animator != null)
    //         animator.SetTrigger(closeAttackAnimationName);
    //     Vector3 direction = (targetPosition - startPosition).normalized;

    //     currentSpeed = 0;

    //     // Move forward
    //     while ((targetPosition - transform.position).magnitude >= 0.5f)
    //     {
    //         currentSpeed += acceleration * Time.deltaTime;
    //         currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    //         transform.position += direction * currentSpeed * Time.deltaTime;
    //         yield return null; // wait one frame
    //     }

    //     // Optional: wait at target
    //     yield return new WaitForSeconds(0.5f);
    //     bulletHell.gameObject.SetActive(true); // This calls OnEnable → starts firing
    //     bulletHell.FireBulletWave();
    //     yield return new WaitForSeconds(inAttackWaitingTime);
    //     bulletHell.StopFiring();
    //     bulletHell.gameObject.SetActive(false); // Optional: to fully hide/deactivate it


    //     // Move back
    //     direction = (startPosition - transform.position).normalized;
    //     currentSpeed = 0;

    //     while ((startPosition - transform.position).magnitude >= 0.5f)
    //     {
    //         //Debug.Log((startPosition - transform.position).magnitude);
    //         currentSpeed += acceleration * Time.deltaTime;
    //         currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    //         transform.position += direction * currentSpeed * Time.deltaTime;
    //         yield return null; // wait one frame
    //     }

    //     transform.position = startPosition; // snap back if needed
    //     currentSpeed = 0;
    //     isCloseAttacking = false;
    //     isAttacking = false;
    //     //Debug.Log("set the is attacking and is close atttacking sateto false");
    // }

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
        animator.SetLayerWeight(1, 1f);  // Enable Hurt layer
        animator.SetTrigger(hurtAnimationName);
        StartCoroutine(DisableHurtLayerAfterTime());


        //Debug.Log("boss health : " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private IEnumerator DisableHurtLayerAfterTime()
    {
            yield return new WaitForSeconds(0.5f); // Adjust based on animation
            animator.SetLayerWeight(1, 0f); // Turn off Hurt layer
    }

    void Die()
    {
        isDead = true;
        isAttacking = false;
        isSpawning = false;
        if (shooterDevice.deviceAnimator != null)
        {
            sfx.PlayDeathSound();
            shooterDevice.deviceAnimator.SetTrigger(shooterDevice.deathTrigger);
            
        }
        Destroy(gameObject, 3f);
            
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
    private IEnumerator DelayedCloseAttack(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        isCloseAttacking = true;
        animator.SetTrigger(closeAttackAnimationName);
    }
    private List<Vector3> GenerateSpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 center = transform.position + spawnCenterOffset;

        switch (spawnShape)
        {
            case SpawnShape.Line:
                float spacing = 2f;
                float startX = center.x - (minionCount - 1) * spacing * 0.5f;
                for (int i = 0; i < minionCount; i++)
                    positions.Add(new Vector3(startX + i * spacing, center.y, center.z));
                break;

            case SpawnShape.Circle:
                for (int i = 0; i < minionCount; i++)
                {
                    float angle = 2 * Mathf.PI * i / minionCount;
                    float x = center.x + Mathf.Cos(angle) * spawnRadius;
                    float y = center.y + Mathf.Sin(angle) * spawnRadius;
                    positions.Add(new Vector3(x, y, center.z));
                }
                break;

            case SpawnShape.Zigzag:
                float zigSpacing = 2f;
                float zigHeight = 1.5f;
                for (int i = 0; i < minionCount; i++)
                {
                    float x = center.x - (minionCount - 1) * zigSpacing * 0.5f + i * zigSpacing;
                    float y = center.y + ((i % 2 == 0) ? zigHeight : -zigHeight);
                    positions.Add(new Vector3(x, y, center.z));
                }
                break;
        }

        return positions;
    }
    public void SetCurrentHealth(float value)
{
    currentHealth = Mathf.Clamp(value, 0, maxHealth);

    if (currentHealth <= 0f)
    {
        Die(); // Trigger death if health is 0 or less
    }
}

void OnDrawGizmos()
{
    // Draw attack range
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRange);

    // Draw minion spawn radius
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(transform.position, spawnRadius);

#if UNITY_EDITOR
    // Simulate spawn positions in edit mode
    if (!Application.isPlaying)
    {
        List<Vector3> preview = GetPreviewSpawnPositions();

        foreach (var pos in preview)
        {
            Gizmos.DrawSphere(pos, 0.2f);
            Handles.Label(pos + Vector3.up * 0.3f, "Spawn");
        }
    }
#endif
}
List<Vector3> GetPreviewSpawnPositions()
{
    List<Vector3> positions = new List<Vector3>();
    Vector3 center = transform.position;

    for (int i = 0; i < minionCount; i++)
    {
        Vector3 offset = Vector3.zero;

        switch (spawnShape)
        {
            case SpawnShape.Line:
                offset = new Vector3(-spawnRadius + (2f * spawnRadius / Mathf.Max(minionCount - 1, 1)) * i, 0, 0);
                break;

            case SpawnShape.Circle:
                float angle = i * Mathf.PI * 2f / minionCount;
                offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnRadius;
                break;

            case SpawnShape.Zigzag:
                float step = 2f * spawnRadius / Mathf.Max(minionCount - 1, 1);
                offset = new Vector3(-spawnRadius + step * i, 0, Mathf.Sin(i * 0.5f * Mathf.PI) * 2f);
                break;
        }

        positions.Add(center + offset);
    }

    return positions;
}




}
