using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;                 // Damage dealt to enemies
    public GameObject keyPrefab;          // Assign your key prefab in the Inspector
    public Vector2 keySpawnOffset = new Vector2(0, 2f);  // Offset above the GoalPoint

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
                Vector3 spawnPosition = other.transform.position + (Vector3)keySpawnOffset;
                Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
            }

            Destroy(other.gameObject); // Optionally remove the goal point object
            Destroy(gameObject);       // Destroy the bullet
            return;
        }

        // If it hit anything else (e.g., wall), destroy the bullet
        Destroy(gameObject);
    }
}
