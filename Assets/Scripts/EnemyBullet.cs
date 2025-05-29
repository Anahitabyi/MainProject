using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Rotate to face the direction of movement
        Vector2 dir = rb.linearVelocity.normalized;
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
                health.takeDamge(damage);
            }
            Destroy(gameObject);
        }
        else if (!collision.isTrigger) // Optional: destroy on hitting anything solid
        {
            Destroy(gameObject);
        }
    }
}
