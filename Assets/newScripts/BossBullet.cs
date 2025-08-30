using UnityEngine;
using Unity.Netcode;

public class BossBullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    [Header("Layers")]
    public LayerMask playerLayer;

    [HideInInspector]
    public BossEnemy bossEnemy;

    private float spawnTime;

    public override void OnNetworkSpawn()
    {
        spawnTime = Time.time;

        if (!IsServer)
        {
            // Only server controls bullet movement/collision
            enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        transform.position += transform.up * speed * Time.deltaTime;

        // Auto-destroy after lifetime
        if (Time.time - spawnTime >= lifeTime)
        {
            NetworkObject.Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null && bossEnemy != null)
            {
                if (bossEnemy.Registerdamage(collision.transform))
                {
                    player.TakeDamage(damage); // Make sure PlayerHealth has ServerRpc for damage
                }
            }
            NetworkObject.Despawn();
        }
        else if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Hit wall or other environment
            NetworkObject.Despawn();
        }
    }
}
