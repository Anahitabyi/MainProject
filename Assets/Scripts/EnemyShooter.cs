using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletPos;
    public GameObject[] players; // Assign both players in the Inspector
    public Vector2 viewBoxSize = new Vector2(10f, 5f); // Width x Height of view area

    private Transform target;
    private float timer;

    private Animator anim;

    void Start()
    {
        // Randomly pick one of the players at the start
        if (players.Length > 0)
        {
            int index = Random.Range(0, players.Length);
            target = players[index].transform;
        }
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (target == null) return;

        // Check if target is in view box
        if (IsInViewBox(target.position))
        {
            timer += Time.deltaTime;

            if (timer > 2f)
            {
                timer = 0;
                ShootAtTarget();
            }
        }
    }

    private void ShootAtTarget()
    {
        if (anim != null)
            anim.SetTrigger("shoot");

        GameObject b = Instantiate(bullet, bulletPos.position, Quaternion.identity);

        Vector2 direction = (target.position - bulletPos.position).normalized;

        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * 10f; // adjust bullet speed
        }
    }

    private bool IsInViewBox(Vector3 playerPos)
    {
        Vector2 enemyPos = transform.position;
        Vector2 boxHalfSize = viewBoxSize / 2;

        return
            playerPos.x >= enemyPos.x - boxHalfSize.x &&
            playerPos.x <= enemyPos.x + boxHalfSize.x &&
            playerPos.y >= enemyPos.y - boxHalfSize.y &&
            playerPos.y <= enemyPos.y + boxHalfSize.y;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the view box in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, viewBoxSize);
    }
}
