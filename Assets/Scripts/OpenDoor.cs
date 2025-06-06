using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpenDoor : MonoBehaviour
{
    private TMP_Text _text;
    KeyTracker keyTracker;
    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            int currentKey = keyTracker.getCurrentKey();
            UpdateButtonText(currentKey);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            
        }
    }
    void UpdateButtonText(int currentKey) {
        _text.text = $"Keys: {currentKey}/4";
    }

    public void OpenLockedDoor()
    {
        int currentKey = keyTracker.getCurrentKey();
        Debug.Log("Button clicked. Trying to open door with " + currentKey);
        if (currentKey == 1)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
