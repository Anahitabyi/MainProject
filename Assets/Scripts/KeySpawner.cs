using UnityEngine;
using Unity.Netcode;

public class KeySpawner : MonoBehaviour
{
    [Header("Key Prefabs (must have NetworkObject)")]
    public NetworkObject[] keyPrefabs; // One prefab per spawn point

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return; // Only server spawns keys

        SpawnKeys();
    }

    private void SpawnKeys()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        Debug.Log("Im spawning keys!");
        if (keyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        int count = Mathf.Min(keyPrefabs.Length, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            NetworkObject keyInstance = Instantiate(keyPrefabs[i], spawnPoints[i].position, Quaternion.identity);

            // Assign unique index for tracking
            CollectKey collectKeyScript = keyInstance.GetComponent<CollectKey>();
            if (collectKeyScript != null)
            {
                collectKeyScript.keyIndex = i;
            }

            keyInstance.Spawn(); // Make it networked
            Debug.Log($"Spawned key {i} ({keyPrefabs[i].name}) at {spawnPoints[i].position}");
        }
    }
}
