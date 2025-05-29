using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1; // Set how much damage this bullet does

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object has an EnemyHealth component (or CatEnemyHealth)
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destroy the bullet after hitting
        }
        else
        {
            // Optional: destroy bullet if it hits a wall or anything else
            Destroy(gameObject);
        }
    }
}
