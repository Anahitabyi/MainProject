using UnityEngine;
  public class LadderZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<PlayerControllerNew>();
            if (controller != null)
                controller.SetOnLadder(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<PlayerControllerNew>();
            if (controller != null)
                controller.SetOnLadder(false);
        }
    }
}
