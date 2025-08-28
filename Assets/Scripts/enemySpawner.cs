using Unity.Netcode;
using UnityEngine;

public class enemySpawner : NetworkBehaviour
{
    [Tooltip("Assign each patrol pair object (with PointA and PointB inside)")]
    public GameObject[] patrolPairs;

    [Tooltip("The enemy prefab to spawn (must have NetworkObject)")]
    public NetworkObject enemyPrefab;

    void Start()
    {
        if (GameStateFlags.IsLoadingFromSave)
        {
            Debug.Log("[Spawner] Skipping enemy spawn because we're loading from save.");
            return;
        }
        if (!IsServer) return; // only server spawns enemies

        foreach (GameObject pair in patrolPairs)
        {
            UniqueID unique = pair.GetComponent<UniqueID>();
            if (unique == null)
            {
                Debug.LogWarning($"[Spawner] Patrol pair '{pair.name}' is missing UniqueID component.");
                continue;
            }

            if (SaveTracker.Instance.IsPatrolPairDisabled(unique.id))
            {
                Debug.Log($"[Spawner] Skipping patrol pair '{pair.name}' (ID: {unique.id}) because it's marked disabled.");
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

            // This is the critical line: make it visible on all clients
            enemyNetObj.Spawn();

            Debug.Log($"[Spawner] Spawned enemy at {spawnPos} for patrol pair '{pair.name}' (ID: {unique.id})");

            patrollingEnemy script = enemyNetObj.GetComponent<patrollingEnemy>();
            script.pointA = pointA.gameObject;
            script.pointB = pointB.gameObject;
            script.patrolPairID = unique.id;
        }
    }
}
