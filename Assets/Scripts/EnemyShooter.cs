using Unity.Netcode;
using UnityEngine;

public class EnemyShooter : NetworkBehaviour, IPooledDeathHandler
{
    public GameObject pointA;
    public GameObject pointB;
    public GameObject bulletPrefab; // now we spawn this directly
    public Transform bulletSpawnPoint;
    public GameObject[] players;
    public Vector2 viewBoxSize = new Vector2(10f, 5f);
    public float patrolSpeed = 3f;
    public float shootCooldown = 2f;
    public float bombSpeed = 20f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    private Transform target;
    private float timer;
    private bool isDead = false;
    private GenerateID uniqueID;
    private EnemyHealth health;

    private void OnEnable()
    {
        PlayerSpawnerTest.OnPlayerUpdated += SetPlayers;
    }

    private void OnDisable()
    {
        PlayerSpawnerTest.OnPlayerUpdated -= SetPlayers;
    }

    private void Awake()
    {
        uniqueID = GetComponent<GenerateID>();
        health = GetComponent<EnemyHealth>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (IsServer)
        {
            currentPoint = pointB.transform;
        }
    }

    void Update()
    {
        if (!IsServer || isDead) return;

        target = FindClosestVisiblePlayer();
        if (IsValidTarget(target) && HasClearShot(target))
        {
            FaceTarget(target.position);
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);

            timer += Time.deltaTime;
            if (timer >= shootCooldown)
            {
                timer = 0f;
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
        Vector2 dir = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * patrolSpeed, rb.linearVelocity.y);
        anim.SetBool("isRunning", true);

        if (Mathf.Abs(transform.position.x - currentPoint.position.x) < 0.1f)
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
        scale.x = targetPos.x < transform.position.x ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void Shoot()
    {
        if (!IsServer || bulletPrefab == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        NetworkObject netObj = bulletObj.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn();

        // Initialize bullet movement on the server
        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            Vector2 dir = GetDirectionVector();
            bullet.Initialize(dir, bombSpeed);
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
        Vector2 half = viewBoxSize / 2f;
        Vector2 enemyPos = transform.position;

        return pos.x >= enemyPos.x - half.x && pos.x <= enemyPos.x + half.x &&
               pos.y >= enemyPos.y - half.y && pos.y <= enemyPos.y + half.y;
    }

    bool HasClearShot(Transform target)
    {
        Vector2 dir = (target.position - bulletSpawnPoint.position).normalized;
        float distance = Vector2.Distance(bulletSpawnPoint.position, target.position);
        RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPoint.position, dir, distance);
        return hit.collider != null && hit.collider.gameObject == target.gameObject;
    }

    Vector2 GetDirectionVector()
    {
        return target != null
            ? (target.position - bulletSpawnPoint.position).normalized
            : (currentPoint.position - transform.position).normalized;
    }

    public void SetPlayers(GameObject[] newPlayers) => players = newPlayers;

    public void OnDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (uniqueID != null && SaveTracker.Instance != null)
            SaveTracker.Instance.RecordEnemyState(uniqueID.Id, health?.CurrentHealth ?? 0, true);

        Destroy(gameObject, 1.5f);
    }
}
