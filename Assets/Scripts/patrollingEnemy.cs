using UnityEngine;

public class patrollingEnemy : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;

    public float speed = 5f;
    private bool isAttacking = false;
    private bool isDead = false;

    [Tooltip("Unique ID of the patrol pair this enemy belongs to")]
    public string patrolPairID;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning($"{name}: pointA or pointB not assigned.");
            enabled = false;
            return;
        }

        currentPoint = pointB.transform;

        if (!string.IsNullOrEmpty(patrolPairID))
            Debug.Log($"[Enemy] {name} initialized with patrolPairID = {patrolPairID}");
        else
            Debug.LogWarning($"[Enemy] {name} has no patrolPairID assigned!");
    }

    void Update()
    {
        if (isDead) return;

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            return;
        }

        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        anim.SetBool("isRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.1f)
        {
            Flip();
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Player") && !isAttacking)
        {
            isAttacking = true;
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(1);
        }
    }

    public void EndAttack()
    {
        if (!isDead)
        {
            Debug.Log("EndAttack triggered");
            isAttacking = false;
        }
    }

    public void Die()
    {
        Debug.Log($"[Enemy] Die() called for {name}");

        if (isDead) return;

        isDead = true;
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;

        if (string.IsNullOrEmpty(patrolPairID))
        {
            Debug.LogWarning($"[Enemy] {name} has no patrolPairID! Cannot mark as disabled.");
        }
        else if (SaveTracker.Instance == null)
        {
            Debug.LogError("[Enemy] SaveTracker.Instance is null! Cannot mark patrol pair as disabled.");
        }
        else
        {
            SaveTracker.Instance.MarkPatrolPairDisabled(patrolPairID);
            Debug.Log($"[Enemy] {name} marked patrolPairID '{patrolPairID}' as disabled.");
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
            anim.SetBool("isRunning", false);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Destroy(gameObject, 1.5f);
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
}
