using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShooterEnemySpawner : MonoBehaviour
{
    [Tooltip("Enemy shooter prefab (must have NetworkObject & EnemyShooter)")]
    public NetworkObject catEnemyPrefab;

    [Tooltip("Markers for spawning cat enemies (MarkerCat_1, MarkerCat_2, etc.)")]
    public GameObject[] catMarkers;

    void Start()
    {
        Debug.Log("[CatSpawner] Start called.");

        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[CatSpawner] No NetworkManager found, skipping spawn.");
            return;
        }

        // Only run on server/host
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[CatSpawner] Not server, skipping spawn.");
            return;
        }

        if (GameStateFlags.IsLoadingFromSave)
        {
            Debug.Log("[CatSpawner] Skipping spawn because loading from save.");
            return;
        }

        SpawnCats();
    }

    private void SpawnCats()
    {
        Debug.Log($"[CatSpawner] Starting spawn for {catMarkers.Length} markers.");

        foreach (GameObject marker in catMarkers)
        {
            if (marker == null)
            {
                Debug.LogWarning("[CatSpawner] Found null marker in array, skipping.");
                continue;
            }

            string idSuffix = marker.name.Replace("MarkerCat_", "");
            GameObject pointA = GameObject.Find("PointAcat" + idSuffix);
            GameObject pointB = GameObject.Find("PointBcat" + idSuffix);

            if (pointA == null || pointB == null)
            {
                Debug.LogWarning($"[CatSpawner] Missing PointA or PointB for cat {idSuffix}, skipping.");
                continue;
            }

            Vector3 spawnPos = marker.transform.position;
            NetworkObject enemyNetObj = Instantiate(catEnemyPrefab, spawnPos, Quaternion.identity);

            if (enemyNetObj == null)
            {
                Debug.LogError($"[CatSpawner] Failed to instantiate NetworkObject for marker {idSuffix}.");
                continue;
            }

            enemyNetObj.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
            enemyNetObj.Spawn();
            Debug.Log($"[CatSpawner] Spawned NetworkObject enemy at {spawnPos} for marker {idSuffix}.");

            EnemyShooter shooter = enemyNetObj.GetComponent<EnemyShooter>();
            if (shooter == null)
            {
                Debug.LogWarning($"[CatSpawner] Spawned enemy missing EnemyShooter component for marker {idSuffix}.");
                continue;
            }

            shooter.pointA = pointA;
            shooter.pointB = pointB;
            shooter.SetPlayers(GetAllPlayers());
        }

        Debug.Log("[CatSpawner] Finished spawning all enemies.");
    }

    private GameObject[] GetAllPlayers()
    {
        var players = new List<GameObject>();
        if (NetworkManager.Singleton == null) return players.ToArray();

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            if (kvp.Value.PlayerObject != null)
                players.Add(kvp.Value.PlayerObject.gameObject);
        }

        Debug.Log($"[CatSpawner] Found {players.Count} connected players to assign as targets.");
        return players.ToArray();
    }
}
