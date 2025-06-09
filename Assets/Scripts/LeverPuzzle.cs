using UnityEngine;

public class LeverPuzzle : MonoBehaviour
{ 
    //CODE NOT USED NOR WORKING
    
    public GameObject leverButtonUI; // UI button shown when player is near
    public Animator leverAnimator;   // Animator for the lever
    public GameObject bridge;        // Bridge to activate

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            leverButtonUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            leverButtonUI.SetActive(false);
        }
    }

    // This is called when the player presses the UI button
    public void PullLever()
    {
        if (activated) return;

        activated = true;
        leverAnimator.SetTrigger("Pull");
        Invoke(nameof(ActivateBridge), 1f); // delay to sync with animation
        leverButtonUI.SetActive(false);     // hide the button after pulling
    }

    private void ActivateBridge()
    {
        bridge.SetActive(true);
    }
}