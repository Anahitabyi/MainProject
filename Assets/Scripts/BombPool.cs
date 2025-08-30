using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class BombPool : NetworkBehaviour, INetworkPrefabInstanceHandler 
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

    // Called by Netcode when a prefab is spawned (server-owned)
    public NetworkObject Instantiate(Vector3 position, Quaternion rotation)
    {
        GameObject bombGO = GetBomb();
        bombGO.transform.position = position;
        bombGO.transform.rotation = rotation;

        NetworkObject netObj = bombGO.GetComponent<NetworkObject>();
        if (netObj == null)
            netObj = bombGO.AddComponent<NetworkObject>();
        bombGO.SetActive(true);
        netObj.Spawn(); // Server owned

        return netObj;
    }

    // Called by Netcode when a prefab is spawned with ownership (client-owned)
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        // For bombs just ignore ownerClientId and use server ownership
        return Instantiate(position, rotation);
    }

    // Called by Netcode when despawning
    public void Destroy(NetworkObject networkObject)
    {
        if (!IsServer) return;
        ReturnBomb(networkObject.gameObject);
    }
}