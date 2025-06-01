using UnityEngine;
using System.Collections.Generic;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletPos;
    public GameObject[] players;
    public Vector2 viewBoxSize = new Vector2(10f, 5f);

    private Transform target;
    private float timer;
    private Animator anim;

    // Track last validity state for logging
    private Dictionary<GameObject, bool> lastValidity = new Dictionary<GameObject, bool>();

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (players == null || players.Length == 0) return;

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.IsDead) return;

        // Log status changes
        foreach (GameObject player in players)
        {
            if (player == null) continue;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null) continue;

            bool isNowValid = !playerHealth.IsDead() && !playerHealth.IsInvincible() && IsInViewBox(player.transform.position);

            if (lastValidity.ContainsKey(player))
            {
                if (!lastValidity[player] && isNowValid)
                {
                    Debug.Log($"EnemyShooter: Player {player.name} became a valid target again.");
                }
            }
            lastValidity[player] = isNowValid;
        }

        // If target is null or invalid, re-acquire
        if (target == null || !IsValidTarget(target))
        {
            target = FindRandomVisiblePlayer();
        }

        // Shoot if target is valid
        if (target != null && IsValidTarget(target))
        {
            timer += Time.deltaTime;
            if (timer > 2f)
            {
                timer = 0;
                ShootAtTarget();
            }
        }
    }

    private Transform FindRandomVisiblePlayer()
    {
        List<Transform> validTargets = new List<Transform>();

        foreach (GameObject player in players)
        {
            if (player == null) continue;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead() || health.IsInvincible()) continue;

            if (IsInViewBox(player.transform.position))
            {
                validTargets.Add(player.transform);
            }
        }

        if (validTargets.Count > 0)
        {
            int index = Random.Range(0, validTargets.Count);
            return validTargets[index];
        }

        return null;
    }

    private bool IsValidTarget(Transform t)
    {
        if (t == null) return false;

        PlayerHealth health = t.GetComponent<PlayerHealth>();
        if (health == null || health.IsDead() || health.IsInvincible()) return false;

        return IsInViewBox(t.position);
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
            rb.linearVelocity = direction * 7f;
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, viewBoxSize);
    }

    public void SetPlayers(GameObject[] players)
    {
        this.players = players;
    }
}
