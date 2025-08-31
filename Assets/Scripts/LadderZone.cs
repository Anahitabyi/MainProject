using UnityEngine;

public class LadderZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerControllerNew>();
            if (playerController != null)
            {
                playerController.SetOnLadder(true); // Call the new method in PlayerControllerNew
                Debug.Log("Player entered ladder zone.");
            }
            var playerController2 = other.GetComponent<PlayerControllerNew2>();
            if (playerController2 != null)
            {
                playerController2.SetOnLadder(true); // Call the new method in PlayerControllerNew
                Debug.Log("Player entered ladder zone.");
            }
            PlayerClimb climb = other.GetComponent<PlayerClimb>();
            if (climb != null)
                climb.SetOnLadder(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerControllerNew>();
            if (playerController != null)
            {
                playerController.SetOnLadder(false); // Call the new method in PlayerControllerNew
                Debug.Log("Player exited ladder zone.");
            }
            var playerController2 = other.GetComponent<PlayerControllerNew2>();
            if (playerController2 != null)
            {
                playerController2.SetOnLadder(false); // Call the new method in PlayerControllerNew
                Debug.Log("Player exited ladder zone.");
            }
            PlayerClimb climb = other.GetComponent<PlayerClimb>();
            if (climb != null)
                climb.SetOnLadder(false);
            
        }
    }
}
