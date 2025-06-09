using UnityEngine;

public class LadderZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerClimb climb = other.GetComponent<PlayerClimb>();
            if (climb != null)
                climb.SetOnLadder(true); // Calls the method form the PlayerClimb so player can start climbing after the setter.
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerClimb climb = other.GetComponent<PlayerClimb>();
            if (climb != null)
                climb.SetOnLadder(false); // No longer climbing
        }
    }
}