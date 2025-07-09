//using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBulletHell : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float rotateSpeed = 10f;
    public int spawnPointCount = 4;
    public float ShooterWaitTime = 1f;
    public float radius = 10f;
    private List<Vector3> spawnPositions = new List<Vector3>();
    public bool shouldFire = true;
    public BossEnemy bossEnemy;
    void OnEnable()
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
            //Debug.Log("Spawn Point " + i + ": " + spawnPosition);
            // You can use this spawnPosition to instantiate a bullet/spawner/etc.
            // Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        }
        StartCoroutine(SpawnBulletWaves());

    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    public Vector3 GetTheBulletDirection(Vector3 bulletPosition)
    {
        Vector3 direction = (bulletPosition - transform.position).normalized;
        return direction;
    }
    public List<Vector3> GetSpawnPositions()
    {
        return spawnPositions;
    }
    private IEnumerator SpawnBulletWaves()
    {
        shouldFire = true;
        while (shouldFire)
        {
            bossEnemy.animator.SetTrigger(bossEnemy.closeAttackAnimationName);
            foreach (Vector3 pos in spawnPositions)
            {
                GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
                BossBulletHellBullet bulletScript = bullet.GetComponent<BossBulletHellBullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize((pos - transform.position).normalized, null, null);
                }
            }

            yield return new WaitForSeconds(ShooterWaitTime);
        }

    }
public void StopFiring()
{
    shouldFire = false;
}



}
