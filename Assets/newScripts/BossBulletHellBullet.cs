using UnityEngine;

public class BossBulletHellBullet : MonoBehaviour
{
    private Vector3 direction;

    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 1;

    [Header("Collision Layers")]
    public LayerMask playerLayer;
    public LayerMask wallLayer;

    // References to be assigned externally
    private BossBulletHell bossBulletHell;
    private BossEnemy bossEnemy;

    // Called from the BossBulletHell script when the bullet is spawned
    public void Initialize(Vector3 direction, BossBulletHell boss, BossEnemy enemy)
    {
        this.direction = direction.normalized;
        this.bossBulletHell = boss;
        this.bossEnemy = enemy;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If it hits the player
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null && bossEnemy != null)
            {
                if (bossEnemy.Registerdamage(collision.transform))
                {
                    player.TakeDamage(damage);
                }
            }
            Destroy(gameObject);
        }

        // If it hits a wall
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
