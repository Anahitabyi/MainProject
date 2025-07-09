using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using System.Collections;
//using System.Numerics;

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
    public Transform[] spawnPoints;
    public int attacksPerMinionSpawn = 3;
    private int attackCount = 0;
    [Header("Close Attack")]
    public int attacksPerCloseAttacks = 10;
    public float currentSpeed = 10;
    public float maxSpeed = 10f;
    public float acceleration = 10f;
    public Transform clostAttackTarget;
    private Vector3 startPosition;
    public float inAttackWaitingTime = 5f;

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
        if (isDead || !canStartAttacking || isCloseAttacking)
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

        if (attackCount % attacksPerMinionSpawn == 0 && attackCount % attacksPerCloseAttacks != 0)
        {
            isSpawning = true;
            animator.SetTrigger(spawnAnimationName);
        }
        if (attackCount % attacksPerCloseAttacks == 0)
        {

            //StartCoroutine(DelayedCloseAttack(3));
            isCloseAttacking = true;
            CloseAttack();
            // animator.SetTrigger(closeAttackAnimationName);
        }
    }
    public void CloseAttack()
    {

        StartCoroutine(CloseAttackCoroutine());

    }
    private IEnumerator CloseAttackCoroutine()
    {
        isCloseAttacking = true;

        if (animator != null)
            animator.SetTrigger(closeAttackAnimationName);
        Vector3 targetPosition = clostAttackTarget.position;
        Vector3 direction = (targetPosition - startPosition).normalized;

        currentSpeed = 0;

        // Move forward
        while ((targetPosition - transform.position).magnitude >= 0.5f)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            transform.position += direction * currentSpeed * Time.deltaTime;
            yield return null; // wait one frame
        }

        // Optional: wait at target
        yield return new WaitForSeconds(0.5f);
        bulletHell.gameObject.SetActive(true); // This calls OnEnable → starts firing
        yield return new WaitForSeconds(inAttackWaitingTime);
        bulletHell.StopFiring();
        bulletHell.gameObject.SetActive(false); // Optional: to fully hide/deactivate it


        // Move back
        direction = (startPosition - transform.position).normalized;
        currentSpeed = 0;

        while ((startPosition - transform.position).magnitude >= 0.1f)
        {
            //Debug.Log((startPosition - transform.position).magnitude);
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            transform.position += direction * currentSpeed * Time.deltaTime;
            yield return null; // wait one frame
        }

        transform.position = startPosition; // snap back if needed
        currentSpeed = 0;
        isCloseAttacking = false;
        isAttacking = false;
        //Debug.Log("set the is attacking and is close atttacking sateto false");
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
    private IEnumerator DelayedCloseAttack(int seconds)
{
    yield return new WaitForSeconds(seconds);
    isCloseAttacking = true;
    animator.SetTrigger(closeAttackAnimationName);
}

}
