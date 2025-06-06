using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touched the key
        PlayerInventory player = other.GetComponent<PlayerInventory>();
        if (player != null)
        {
            player.hasKey = true; // Set the boolean
            Destroy(gameObject);  // Remove the key from the scene
        }
    }
}
