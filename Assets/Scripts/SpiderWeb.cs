using UnityEngine;
using UnityEngine.InputSystem;

public class SpiderWeb : MonoBehaviour
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
        if (hoodedPlayerInContact && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Hooded player pressed space near web — destroying web!");
            Destroy(gameObject);
        }
    }
}