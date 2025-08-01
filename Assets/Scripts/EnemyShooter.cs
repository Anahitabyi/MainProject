using UnityEngine;

public class EnemyShooter : MonoBehaviour, IPooledDeathHandler
{
    public GameObject pointA;
    public GameObject pointB;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public GameObject[] players;
    public Vector2 viewBoxSize = new Vector2(10f, 5f);
    public float patrolSpeed = 3f;
    public float shootCooldown = 2f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private Transform target;
    private float timer;
    private bool isDead = false;
    private GenerateID uniqueID;
    private EnemyHealth health;
    private void Awake()
    {
        uniqueID = GetComponent<GenerateID>();
        health = GetComponent<EnemyHealth>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
    }

    void Update()
    {
        if (isDead) return;

        target = FindClosestVisiblePlayer();
        if (IsValidTarget(target))
        {
            FaceTarget(target.position);
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);

            timer += Time.deltaTime;
            if (timer >= shootCooldown)
            {
                timer = 0;
                anim.SetTrigger("shoot");
                Shoot();
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);
        anim.SetBool("isRunning", true);

        float distanceX = Mathf.Abs(transform.position.x - currentPoint.position.x);
        if (distanceX < 0.1f)
        {
            Flip();
            currentPoint = currentPoint == pointA.transform ? pointB.transform : pointA.transform;
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 scale = transform.localScale;

        if (targetPos.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();

        if (rbBullet && target != null)
        {
            Vector2 dir = (target.position - bulletSpawnPoint.position).normalized;
            rbBullet.linearVelocity = dir * 7f;
        }
    }

    Transform FindClosestVisiblePlayer()
    {
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            if (player == null) continue;
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph == null || ph.IsDead() || ph.IsInvincible()) continue;

            if (IsInViewBox(player.transform.position))
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = player.transform;
                }
            }
        }
        return closest;
    }

    bool IsValidTarget(Transform t)
    {
        if (t == null) return false;
        PlayerHealth ph = t.GetComponent<PlayerHealth>();
        return ph != null && !ph.IsDead() && !ph.IsInvincible() && IsInViewBox(t.position);
    }

    bool IsInViewBox(Vector3 pos)
    {
        Vector2 enemyPos = transform.position;
        Vector2 half = viewBoxSize / 2f;

        return pos.x >= enemyPos.x - half.x && pos.x <= enemyPos.x + half.x &&
               pos.y >= enemyPos.y - half.y && pos.y <= enemyPos.y + half.y;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, viewBoxSize);

        if (pointA && pointB)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.transform.position, 0.2f);
            Gizmos.DrawSphere(pointB.transform.position, 0.2f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.15f);

        if (Application.isPlaying && currentPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(currentPoint.position, 0.25f);
        }
    }

    public void OnDeath()
{
    Debug.Log($"[EnemyShooter] OnDeath called for: {gameObject.name}");

    isDead = true;
    rb.linearVelocity = Vector2.zero;
    anim.SetTrigger("Die");
    GetComponent<Collider2D>().enabled = false;
    rb.constraints = RigidbodyConstraints2D.FreezeAll;
    foreach (var kvp in SaveTracker.Instance.enemyStates)
{
    Debug.Log($"[Save] Enemy ID: {kvp.Key}, HP: {kvp.Value.currentHealth}, Dead: {kvp.Value.isDead}");
}

    // ✅ Save death state
    if (uniqueID != null && SaveTracker.Instance != null)
    {
        Debug.Log($"[EnemyShooter] Recording state for: {uniqueID.Id}");

        SaveTracker.Instance.RecordEnemyState(
            uniqueID.Id,
            health != null ? health.CurrentHealth : 0,
            true
        );
    }

    Destroy(gameObject, 1.5f);
}


    public void SetPlayers(GameObject[] newPlayers)
    {
        players = newPlayers;
    }
}
