using UnityEngine;

public class patrollingEnemy : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    public float speed = 5f;

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

        if (anim != null)
            anim.SetBool("isRunning", true);
    }

    void Update()
    {
        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, 0);

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
