using UnityEngine;
using TMPro;

public class SaveFeedback : MonoBehaviour
{
    public GameObject panel;         // Assign: SaveFeedbackPanel
    public TMP_Text messageText;     // Assign: child text component
    public float displayTime = 2f;   // How long to show the message

    public void Show(string message)
    {
        StopAllCoroutines();         // If a message is already showing
        StartCoroutine(ShowRoutine(message));
    }

    private System.Collections.IEnumerator ShowRoutine(string msg)
    {
        messageText.text = msg;
        panel.SetActive(true);
        yield return new WaitForSecondsRealtime(displayTime); // ✅ Works during pause
        panel.SetActive(false);
    }
}
