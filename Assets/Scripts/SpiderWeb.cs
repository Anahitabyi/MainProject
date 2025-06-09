using UnityEngine;
using UnityEngine.InputSystem;

public class SpiderWeb : MonoBehaviour
{
    private bool hoodedPlayerInContact = false; // Only hooded can destroy it

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerIdentifier playerId = collision.collider.GetComponentInParent<PlayerIdentifier>(); // Player identifier to check if the player is hooded
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
        if (hoodedPlayerInContact && Keyboard.current.spaceKey.wasPressedThisFrame) // If hooded attacks the web, it gets destroyed
        {
            Debug.Log("Hooded player pressed space near web — destroying web!");
            Destroy(gameObject);
        }
    }
}