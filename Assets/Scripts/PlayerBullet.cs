using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;                 // Damage dealt to enemies
    public GameObject keyPrefab;          // Assign your key prefab in the Inspector
    public Vector2 keySpawnOffset = new Vector2(0, 2f);  // Offset above the GoalPoint

    // Event to notify when the bullet is destroyed
    public delegate void BulletDestroyed();
    public event BulletDestroyed OnDestroyed;

    private void Start()
    {
    Destroy(gameObject, 3f); // Automatically destroy bullet after 2 seconds
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it hit an enemy
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            DestroyBullet();
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

            Destroy(other.gameObject);
            DestroyBullet();
            return;
        }

        // Hit something else
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        // Trigger the event before the bullet is destroyed
        if (OnDestroyed != null)
        {
            OnDestroyed.Invoke();
        }

        Destroy(gameObject);
    }
}
