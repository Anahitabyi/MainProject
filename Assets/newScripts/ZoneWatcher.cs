using UnityEngine;

public class ZoneWatcher : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public SceneFader sceneFader;

    private bool player1In = false;
    private bool player2In = false;

    private bool fadePlayed = false;  // Track if fade already played

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fadePlayed)
            return;  // Fade already played, do nothing

        if (other.gameObject == player1)
            player1In = true;
        else if (other.gameObject == player2)
            player2In = true;

        if (player1In || player2In)
        {
            sceneFader.PlayCutsceneFade();
            fadePlayed = true;  // Mark fade as played
            enabled = false;    // Disable this script to prevent further checks
        }
    }
}
