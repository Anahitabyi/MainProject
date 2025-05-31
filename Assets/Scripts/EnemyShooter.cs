using UnityEngine;
using System.Collections.Generic;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletPos;
    public GameObject[] players; // Will be assigned dynamically
    public Vector2 viewBoxSize = new Vector2(10f, 5f); // Width x Height of view area

    private Transform target;
    private float timer;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
{
    if (players == null || players.Length == 0) return;

    // Stop everything if enemy is dead
    EnemyHealth health = GetComponent<EnemyHealth>();
    if (health != null && health.IsDead) return;

    // Target logic...
    if (target == null || !IsInViewBox(target.position))
    {
        target = FindRandomVisiblePlayer();
    }

    if (target != null && IsInViewBox(target.position))
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
        List<Transform> visiblePlayers = new List<Transform>();

        foreach (GameObject player in players)
        {
            if (player != null && IsInViewBox(player.transform.position))
            {
                visiblePlayers.Add(player.transform);
            }
        }

        if (visiblePlayers.Count > 0)
        {
            int index = Random.Range(0, visiblePlayers.Count);
            return visiblePlayers[index];
        }

        return null;
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

    // New method for assigning players
    public void SetPlayers(GameObject[] players)
    {
        this.players = players;
    }
}
