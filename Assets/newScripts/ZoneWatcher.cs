using Unity.Netcode;
using UnityEngine;

public class ZoneWatcher : NetworkBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public SceneFader sceneFader;

    private bool player1In = false;
    private bool player2In = false;

    private bool fadePlayed = false;

    void Start()
    {
        // Load the saved state of cutscene playback
        if (SaveTracker.Instance != null)
        {
            fadePlayed = SaveTracker.Instance.level3CutscenePlayed;

            // If the cutscene was already played in a previous session, disable this script
            if (fadePlayed)
            {
                Debug.Log("Level 3 cutscene already played. Disabling ZoneWatcher.");
                enabled = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner)
            return;
        if (fadePlayed)
                return;

        if (other.gameObject == player1)
            player1In = true;
        else if (other.gameObject == player2)
            player2In = true;

        if (player1In || player2In)
        {
            sceneFader.PlayCutsceneFade();
            fadePlayed = true;

            if (SaveTracker.Instance != null)
            {
                SaveTracker.Instance.level3CutscenePlayed = true;
            }

            if (GameSaveController.Instance != null)
            {
                GameSaveController.Instance.SaveToFile();
            }

            enabled = false;
        }
    }
}
