using Unity.Netcode;
using UnityEngine;

public class EnemyBullet : NetworkBehaviour
{
    public int damage = 1;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private float speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called on the server when bullet is spawned
    public void Initialize(Vector2 direction, float moveSpeed)
    {
        moveDir = direction.normalized;
        speed = moveSpeed;

        // Rotate to face the movement direction
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (IsServer)
            rb.linearVelocity = moveDir * speed;
    }

    private void FixedUpdate()
    {
        if (IsServer)
            rb.linearVelocity = moveDir * speed; // keep movement consistent
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return; // only handle collisions on server

        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            NetworkObject.Despawn(); // networked destroy
        }
        else if (!collision.isTrigger)
        {
            NetworkObject.Despawn(); // destroy on hitting solid objects
        }
    }
}