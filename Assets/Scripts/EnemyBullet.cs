using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // New method: set bullet direction and speed
    public void SetDirection(Vector2 dir, float speed)
    {
        dir.Normalize();
        rb.linearVelocity = dir * speed;

        // Rotate to face the movement direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Bullet hit player!");
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!collision.isTrigger) // Optional: destroy on hitting anything solid
        {
            Destroy(gameObject);
        }
    }
}
