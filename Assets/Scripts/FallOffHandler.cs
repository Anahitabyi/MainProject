using UnityEngine;
using UnityEngine.SceneManagement;

public class FallOffHandler : MonoBehaviour
{
    public float fallThresholdY = -30f;

    private PlayerHealth playerHealth;
    private ChunkGenerator chunkGenerator;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        chunkGenerator = FindFirstObjectByType<ChunkGenerator>();

        if (chunkGenerator == null)
        {
            Debug.Log("ChunkGenerator not found!");
        }
    }

    void Update()
    {
        if (transform.position.y < fallThresholdY)
        {
            HandleFall();
        }
    }

    void HandleFall()
    {
        if (playerHealth == null || chunkGenerator == null) return;

        playerHealth.TakeDamage(playerHealth.currentHealth); // Lose 1 life

        if (playerHealth.currentLives > 0)
        {
            Transform spawnPoint = chunkGenerator.FirstSpawnPoint;

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;

                if (playerHealth.rb != null)
                    playerHealth.rb.linearVelocity = Vector2.zero;
            }
            else
            {
                Debug.LogWarning("First chunk spawn point not set. Respawning at (0, 0).");
                transform.position = Vector3.zero;
            }
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
