using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerInput : NetworkBehaviour
{
    public PlayerInput playerInput;
    public PlayerIdentifier playerIdentifier;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
        if (playerIdentifier == null)
            playerIdentifier = GetComponent<PlayerIdentifier>();
    }

    public override void OnNetworkSpawn()
    {
        // Determine if this is the local owner (or offline player)
        bool isLocalOwner = !NetworkManager.Singleton.IsClient || IsOwner;

        // Enable input only for owner
        playerInput.enabled = isLocalOwner;

        if (isLocalOwner)
        {
            InputDevice device = Keyboard.current; // Only allow keyboard

            switch (playerIdentifier.playerType)
            {
                case PlayerIdentifier.PlayerType.Hooded:
                    playerInput.SwitchCurrentControlScheme("KeyboardRight", device);
                    break;
                case PlayerIdentifier.PlayerType.Hobbit:
                    playerInput.SwitchCurrentControlScheme("KeyboardLeft", device);
                    break;
            }

            // Prevent the Input System from switching devices automatically
            playerInput.neverAutoSwitchControlSchemes = true;
        }

    }
}
