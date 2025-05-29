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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (pointA == null || pointB == null)
        {
            Debug.LogError($"{name}: pointA or pointB not assigned.");
            enabled = false;
            return;
        }

        currentPoint = pointB.transform;
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
                playerHealth.takeDamge(1);
        }
    }

    // Called via animation event
    public void EndAttack()
    {
        if (!isDead)
        {
            Debug.Log("EndAttack triggered");
            isAttacking = false;
        }
    }

    // Called from your health script when dying
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetTrigger("Die");
            anim.SetBool("isRunning", false);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Destroy(gameObject, 1.5f); // Wait for death animation
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
