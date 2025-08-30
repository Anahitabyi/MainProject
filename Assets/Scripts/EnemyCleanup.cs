using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyCleanup : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true); // true = destroy the object
            }
        }
    }
}