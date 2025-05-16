using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSetup : MonoBehaviour
{
    public PlayerInput player1;
    public PlayerInput player2;

    void Start()
    {
        if (player1 != null)
            player1.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);

        if (player2 != null)
            player2.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);
    }
}
