using UnityEngine;
using System.Collections.Generic;

public class RandomCollectibleSpawner : MonoBehaviour
{
    [Header("Collectibles")]
    public GameObject[] collectibles; // Just drag your 4 collectible prefabs here

    [Header("Spawn Settings")]
    public int collectiblesToSpawn = 3;
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnCollectiblesRandomly();
    }

    void SpawnCollectiblesRandomly()
    {
        if (collectibles.Length == 0 || spawnPoints.Length == 0) return;

        // Shuffle collectibles
        List<GameObject> shuffledCollectibles = new List<GameObject>(collectibles);
        Shuffle(shuffledCollectibles);

        // Shuffle spawn points
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        Shuffle(availablePoints);

        // Spawn collectibles
        for (int i = 0; i < collectiblesToSpawn && i < shuffledCollectibles.Count && i < availablePoints.Count; i++)
        {
            Instantiate(shuffledCollectibles[i], availablePoints[i].position, Quaternion.identity);
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
