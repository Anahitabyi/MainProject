using UnityEngine;
using Unity.Netcode;

public class AutoDestroy : NetworkBehaviour
{
    public void DestroySelf()
    {
        if (IsServer) // ✅ only the server is allowed to despawn network objects
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Destroy(gameObject); // fallback if it's not a networked object
            }
        }
    }
}
