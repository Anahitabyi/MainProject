using Unity.Netcode;
using UnityEngine;

public class ShooterEnemySpawner : NetworkBehaviour
{
    [Tooltip("Enemy shooter prefab (must have NetworkObject & EnemyShooter)")]
    public NetworkObject catEnemyPrefab;

    [Tooltip("Markers for spawning cat enemies (MarkerCat_1, MarkerCat_2, etc.)")]
    public GameObject[] catMarkers;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only spawn on server/host
        if (!IsServer)
        {
            Debug.Log("[CatSpawner] Not server, skipping spawn.");
            return;
        }

        // Skip if loading from save
        if (GameStateFlags.IsLoadingFromSave)
        {
            Debug.Log("[CatSpawner] Skipping spawn because we're loading from save.");
            return;
        }

        SpawnCats();
    }

    private void SpawnCats()
    {
        foreach (GameObject marker in catMarkers)
        {
            if (marker == null) continue;

            string idSuffix = marker.name.Replace("MarkerCat_", "");
            GameObject pointA = GameObject.Find("PointAcat" + idSuffix);
            GameObject pointB = GameObject.Find("PointBcat" + idSuffix);

            if (pointA == null || pointB == null)
            {
                Debug.LogWarning($"[CatSpawner] Missing PointA/PointB for cat {idSuffix}");
                continue;
            }

            Vector3 spawnPos = marker.transform.position;
            NetworkObject enemyNetObj = Instantiate(catEnemyPrefab, spawnPos, Quaternion.identity);
            enemyNetObj.transform.localScale = new Vector3(-1.5f, 1.5f, 1f);
            enemyNetObj.Spawn();

            EnemyShooter shooter = enemyNetObj.GetComponent<EnemyShooter>();
            if (shooter != null)
            {
                shooter.pointA = pointA;
                shooter.pointB = pointB;
                shooter.SetPlayers(GetAllPlayers());
            }

            Debug.Log($"[CatSpawner] Spawned cat enemy {idSuffix} at {spawnPos}");
        }
    }

    private GameObject[] GetAllPlayers()
    {
        if (NetworkManager.Singleton == null) return new GameObject[0];

        var players = new System.Collections.Generic.List<GameObject>();
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            if (kvp.Value.PlayerObject != null)
                players.Add(kvp.Value.PlayerObject.gameObject);
        }
        return players.ToArray();
    }
}
