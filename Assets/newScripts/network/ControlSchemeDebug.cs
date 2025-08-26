using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemeDebug : MonoBehaviour
{
    public PlayerInput playerInput;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // Check if L key is pressed
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (playerInput != null)
            {
                Debug.Log("Current control scheme: " + playerInput.currentControlScheme);
            }
        }
    }
}
