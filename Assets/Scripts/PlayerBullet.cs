using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;                 // Damage dealt to enemies
    public GameObject keyPrefab;          // Assign your key prefab in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it hit an enemy
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destroy the bullet after hitting an enemy
            return;
        }

        // Check if it hit a GoalPoint
        if (other.CompareTag("GoalPoint"))
        {
            if (keyPrefab != null)
            {
                // Spawn the key at the goal point's position
                Instantiate(keyPrefab, other.transform.position, Quaternion.identity);
            }

            Destroy(other.gameObject); // Optionally remove the goal point object
            Destroy(gameObject);       // Destroy the bullet
            return;
        }

        // If it hit anything else (e.g., wall), destroy the bullet
        Destroy(gameObject);
    }
}
