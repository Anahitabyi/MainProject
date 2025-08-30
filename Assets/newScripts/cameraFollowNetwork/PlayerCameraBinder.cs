using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerCameraBinder : NetworkBehaviour
{
    private PlayerIdentifier identifier;

    [SerializeField] private CinemachineTargetGroup group; // assign in inspector

    public override void OnNetworkSpawn()
    {
        identifier = GetComponent<PlayerIdentifier>();

        // --- Keep your existing camera assignment logic intact ---
        var cameras = FindObjectsByType<CameraAssignNetwork>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var cam in cameras)
        {
            if (cam.TargetType == identifier.playerType)
            {
                cam.SetTarget(transform); // existing logic stays the same
                break;
            }
        }
        // --- NEW: get the CinemachineTargetGroup using the new API ---
        if (group == null)
        {
            group = Object.FindFirstObjectByType<CinemachineTargetGroup>();
            // OR: group = Object.FindAnyObjectByType<CinemachineTargetGroup>();
        }

        // --- NEW: add this player to the target group if assigned ---
        if (group != null)
        {
            AddToTargetGroup(group);
        }
    }

    private void AddToTargetGroup(CinemachineTargetGroup group)
    {
        // Check if this player is already in the group
        foreach (var t in group.Targets)
        {
            if (t.Object == transform)
                return; // already added
        }

        // Add new player
        group.AddMember(transform, weight: 1f, radius: 0.5f);
    }



}
