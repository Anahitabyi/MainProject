using UnityEngine;
using System.Collections.Generic;

public class RandomCollectibleSpawner : MonoBehaviour
{
    [Header("Collectible Settings")]
    public GameObject[] collectiblePrefabs; // You can assign Food, Coin, Powerup, etc.
    public int collectiblesToSpawn = 3;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnCollectiblesRandomly();
    }

    void SpawnCollectiblesRandomly()
    {
        if (spawnPoints.Length == 0 || collectiblePrefabs.Length == 0) return;

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < collectiblesToSpawn && availablePoints.Count > 0; i++)
        {
            // Choose random spawn point
            int pointIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[pointIndex];

            // Choose random collectible
            int collectibleIndex = Random.Range(0, collectiblePrefabs.Length);
            GameObject collectible = collectiblePrefabs[collectibleIndex];

            Instantiate(collectible, spawnPoint.position, Quaternion.identity);

            availablePoints.RemoveAt(pointIndex); // prevent duplicate use
        }
    }
}
