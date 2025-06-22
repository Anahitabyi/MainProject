using UnityEngine;

public class MinionEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2f;
    public int attackDamage = 1;
    public float attackRange = 1f;
    public float attackCooldown = 1f;

    [Header("Players")]
    public GameObject[] players;
    private GameObject targetPlayer;

    [Header("Animation")]
    public Animator animator;
    public string walkAnim = "Walk";
    public string attackAnim = "Attack";

    private float attackTimer = 0f;
    private bool isAttacking = false;

    private EnemyHealth healthScript;

    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (healthScript != null && healthScript.IsDead) return;
        if (targetPlayer == null) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);

        if (distance <= attackRange)
        {
            if (!isAttacking && attackTimer <= 0f)
            {
                isAttacking = true;
                animator.SetTrigger(attackAnim);
                attackTimer = attackCooldown;
            }

            animator.SetBool(walkAnim, false);
        }
        else
        {
            MoveTowardPlayer();
        }

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }

    void MoveTowardPlayer()
    {
        animator.SetBool(walkAnim, true);

        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void DealDamage() // Animation event
    {
        if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.transform.position) <= attackRange)
        {
            PlayerHealth playerHealth = targetPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void EndAttack() // Animation event
    {
        isAttacking = false;
    }

    public void SetPlayers(GameObject[] newPlayers, GameObject bossTarget)
{
    players = newPlayers;

    // Always choose the player that is NOT the one targeted by the boss
    foreach (GameObject player in players)
    {
        if (player != null && player != bossTarget)
        {
            targetPlayer = player;
            return;
        }
    }

    // If both players are null or bossTarget is the only valid one
    targetPlayer = null;
}



    // ✅ Optional trigger-based damage if you still want it
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
