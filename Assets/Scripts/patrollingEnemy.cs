using UnityEngine;
using Unity.Netcode;

public class patrollingEnemy : NetworkBehaviour, IPooledDeathHandler
{
    [Header("Patrol Points")]
    public GameObject pointA;
    public GameObject pointB;

    [Header("Movement & Combat")]
    public float speed = 5f;
    public string patrolPairID; // Unique ID for saving

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private bool isDead = false;

    // Network-synced attack state
    public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>(false);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        Debug.Log($"[patrollingEnemy] Start called for {name}");

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning($"{name}: pointA or pointB not assigned.");
            enabled = false;
            return;
        }

        currentPoint = pointB.transform;

        if (!string.IsNullOrEmpty(patrolPairID))
            Debug.Log($"[patrollingEnemy] {name} initialized with patrolPairID = {patrolPairID}");
        else
            Debug.LogWarning($"[patrollingEnemy] {name} has no patrolPairID assigned!");

        Debug.Log($"[patrollingEnemy] Starting position: {transform.position}");
    }

    void Update()
    {
        if (!IsServer) return; // Only server controls movement/logic
        if (isDead) return;

        if (IsAttacking.Value)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            return;
        }

        Patrol();
    }

    private void Patrol()
    {
        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
        anim.SetBool("isRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.1f)
        {
            Flip();
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
            Debug.Log($"[patrollingEnemy] {name} switched patrol point to {currentPoint.name}");
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        Debug.Log($"[patrollingEnemy] {name} flipped. New scale: {transform.localScale}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || isDead) return;

        if (collision.CompareTag("Player") && !IsAttacking.Value)
        {
            IsAttacking.Value = true;
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
                anim.SetTrigger("Attack");

            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);

            Debug.Log($"[patrollingEnemy] {name} started attacking player {collision.name}");
        }
    }

    public void EndAttack()
    {
        if (!IsServer || isDead) return;
        IsAttacking.Value = false;
        Debug.Log($"[patrollingEnemy] {name} ended attack");
    }

    public void Die()
    {
        if (!IsServer || isDead) return;

        isDead = true;
        IsAttacking.Value = false;

        // Save patrol pair state
        if (!string.IsNullOrEmpty(patrolPairID) && SaveTracker.Instance != null)
        {
            SaveTracker.Instance.MarkPatrolPairDisabled(patrolPairID);
            Debug.Log($"[patrollingEnemy] Marked patrolPairID '{patrolPairID}' as disabled.");
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
            anim.SetBool("isRunning", false);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (rb) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Despawn();
            Debug.Log($"[patrollingEnemy] {name} despawned network object.");
        }
        else
        {
            Destroy(gameObject, 1.5f);
            Debug.Log($"[patrollingEnemy] {name} destroyed locally.");
        }
    }

    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointA.transform.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.transform.position, 0.2f);
            Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
        }
    }

    public void OnDeath()
    {
        Debug.Log($"[patrollingEnemy] OnDeath called for {name}");
        Die(); // Called by EnemyHealth or other death systems
    }
}
