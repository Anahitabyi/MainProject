using System.Collections.Generic;
using UnityEngine;

public class BossBulletHell : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float radius = 10f;
    public int spawnPointCount = 4;
    
    private List<Vector3> spawnPositions = new List<Vector3>();

    void OnEnable()
    {
        GenerateSpawnPositions();
    }

    // Call this from an Animation Event
    public void FireBulletWave()
    {
        foreach (Vector3 pos in spawnPositions)
        {
            GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
            BossBulletHellBullet bulletScript = bullet.GetComponent<BossBulletHellBullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize((pos - transform.position).normalized, null, null);
            }
        }
    }

    public void StopFiring()
    {
        // Placeholder for symmetry if needed in your logic
    }

    private void GenerateSpawnPositions()
    {
        spawnPositions.Clear();

        float step = 2 * Mathf.PI / spawnPointCount;

        for (int i = 0; i < spawnPointCount; i++)
        {
            float angle = step * i;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            Vector3 spawnPosition = transform.position + new Vector3(x, y, 0f);
            spawnPositions.Add(spawnPosition);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public List<Vector3> GetSpawnPositions()
    {
        return spawnPositions;
    }

    public Vector3 GetTheBulletDirection(Vector3 bulletPosition)
    {
        return (bulletPosition - transform.position).normalized;
    }
}
