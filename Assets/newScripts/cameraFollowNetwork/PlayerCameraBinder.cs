using UnityEngine;
using Unity.Netcode;

public class PlayerCameraBinder : NetworkBehaviour
{
    private PlayerIdentifier identifier;

    public override void OnNetworkSpawn()
    {
        //if (!IsOwner) return; // only bind the camera for the local player

        identifier = GetComponent<PlayerIdentifier>();

        var cameras = FindObjectsByType<CameraAssignNetwork>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var cam in cameras)
        {
            if (cam.TargetType == identifier.playerType)
            {
                cam.SetTarget(transform);
                break;
            }
        }
    }
}
