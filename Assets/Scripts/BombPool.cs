using UnityEngine;
using System.Collections.Generic;

public class BombPool : MonoBehaviour
{
    public static BombPool Instance;

    public GameObject bombPrefab;
    public int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bomb = Instantiate(bombPrefab);
            bomb.SetActive(false);
            pool.Enqueue(bomb);
        }
    }

    public GameObject GetBomb()
    {
        GameObject bomb = pool.Count > 0 ? pool.Dequeue() : Instantiate(bombPrefab);
        bomb.SetActive(true);
        return bomb;
    }

    public void ReturnBomb(GameObject bomb)
    {
        bomb.SetActive(false);
        pool.Enqueue(bomb);
    }
}