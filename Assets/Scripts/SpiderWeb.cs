using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpiderWeb : NetworkBehaviour
{
    private bool hoodedPlayerInContact = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerIdentifier playerId = collision.collider.GetComponentInParent<PlayerIdentifier>();
        if (playerId == null) return;

        if (playerId.playerType == PlayerIdentifier.PlayerType.Hooded)
        {
            hoodedPlayerInContact = true;
            Debug.Log("Hooded player started colliding with web");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        PlayerIdentifier playerId = collision.collider.GetComponentInParent<PlayerIdentifier>();
        if (playerId == null) return;

        if (playerId.playerType == PlayerIdentifier.PlayerType.Hooded)
        {
            hoodedPlayerInContact = false;
            Debug.Log("Hooded player stopped colliding with web");
        }
    }

    private void Update()
    {
        // Any client whose Hooded player is in contact can request destruction
        if (hoodedPlayerInContact && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            DestroyWebServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyWebServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Web destroyed on server by Hooded player");

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Despawn(); // synced destruction across all clients
        }
        else
        {
            Destroy(gameObject); // fallback
        }
    }
}