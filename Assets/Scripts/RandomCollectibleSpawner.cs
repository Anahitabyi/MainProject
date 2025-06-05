using UnityEngine;
using System.Collections.Generic;

public class RandomCollectibleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Range(1, 10)]
        public int weight = 1;
    }

    [Header("Collectible Settings")]
    public List<SpawnEntry> collectibleEntries;
    public int collectiblesToSpawn = 3;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnCollectiblesRandomly();
    }

    void SpawnCollectiblesRandomly()
    {
        if (spawnPoints.Length == 0 || collectibleEntries.Count == 0) return;

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < collectiblesToSpawn && availablePoints.Count > 0; i++)
        {
            Transform spawnPoint = GetRandomSpawnPoint(ref availablePoints);
            GameObject collectible = GetRandomCollectible();

            Instantiate(collectible, spawnPoint.position, Quaternion.identity);
        }
    }

    Transform GetRandomSpawnPoint(ref List<Transform> points)
    {
        int index = Random.Range(0, points.Count);
        Transform point = points[index];
        points.RemoveAt(index);
        return point;
    }

    GameObject GetRandomCollectible()
    {
        int totalWeight = 0;
        foreach (var entry in collectibleEntries)
            totalWeight += entry.weight;

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var entry in collectibleEntries)
        {
            current += entry.weight;
            if (roll < current)
                return entry.prefab;
        }

        // fallback
        return collectibleEntries[0].prefab;
    }
}
