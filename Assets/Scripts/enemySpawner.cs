using Unity.Netcode;
using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    [Tooltip("Assign each patrol pair object (with PointA and PointB inside)")]
    public GameObject[] patrolPairs;

    [Tooltip("The enemy prefab to spawn (must have NetworkObject)")]
    public NetworkObject enemyPrefab;

    void Start()
    {
        Debug.Log("[Spawner] Start called.");

        // Only spawn on the server
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[Spawner] Not server, skipping spawn.");
            return;
        }

        // Skip if loading from save
        if (GameStateFlags.IsLoadingFromSave)
        {
            Debug.Log("[Spawner] Skipping enemy spawn because we're loading from save.");
            return;
        }

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        Debug.Log($"[Spawner] Starting spawn for {patrolPairs.Length} patrol pairs.");

        foreach (GameObject pair in patrolPairs)
        {
            if (pair == null)
            {
                Debug.LogWarning("[Spawner] Encountered a null patrol pair in the array.");
                continue;
            }

            UniqueID unique = pair.GetComponent<UniqueID>();
            if (unique == null)
            {
                Debug.LogWarning($"[Spawner] Patrol pair '{pair.name}' is missing UniqueID component.");
                continue;
            }

            if (SaveTracker.Instance.IsPatrolPairDisabled(unique.id))
            {
                Debug.Log($"[Spawner] Skipping patrol pair '{pair.name}' because it's marked disabled.");
                continue;
            }

            Transform pointA = pair.transform.Find("PointA");
            Transform pointB = pair.transform.Find("PointB");

            if (pointA == null || pointB == null)
            {
                Debug.LogWarning($"[Spawner] Patrol pair '{pair.name}' is missing PointA or PointB.");
                continue;
            }

            Vector3 spawnPos = (pointA.position + pointB.position) / 2f;

            NetworkObject enemyNetObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            patrollingEnemy script = enemyNetObj.GetComponent<patrollingEnemy>();
            if (script == null)
            {
                Debug.LogError($"[Spawner] Enemy prefab does not have a patrollingEnemy component.");
                Destroy(enemyNetObj.gameObject);
                continue;
            }

            script.pointA = pointA.gameObject;
            script.pointB = pointB.gameObject;
            script.patrolPairID = unique.id;

            enemyNetObj.Spawn();
            Debug.Log($"[Spawner] Spawned enemy for patrol pair '{pair.name}' with ID {unique.id}");
        }

        Debug.Log("[Spawner] Finished spawning patrol pair enemies.");
    }
}
