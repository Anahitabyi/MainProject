using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class RandomCollectibleSpawner : NetworkBehaviour
{
    [Header("Collectibles")]
    public NetworkObject[] collectibles; // Prefabs with NetworkObject component

    [Header("Spawn Settings")]
    public int collectiblesToSpawn = 3;
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        // Only the host should spawn
        if (IsServer)
        {
            SpawnCollectiblesRandomly();
            Debug.Log("called spawning");
        }
    }

    void SpawnCollectiblesRandomly()
    {
        if (collectibles.Length == 0 || spawnPoints.Length == 0) return;

        // Shuffle collectibles
        List<NetworkObject> shuffledCollectibles = new List<NetworkObject>(collectibles);
        Shuffle(shuffledCollectibles);

        // Shuffle spawn points
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        Shuffle(availablePoints);

        // Spawn collectibles
        for (int i = 0; i < collectiblesToSpawn && i < shuffledCollectibles.Count && i < availablePoints.Count; i++)
        {
            NetworkObject collectibleInstance = Instantiate(
                shuffledCollectibles[i],
                availablePoints[i].position,
                Quaternion.identity
            );

            collectibleInstance.Spawn(); // <-- This makes it networked!
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
