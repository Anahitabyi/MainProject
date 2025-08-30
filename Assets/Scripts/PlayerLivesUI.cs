using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    public int playerId = 1;            // Match the chosen character or your mapping
    public Image[] heartImages;         // Assign in Inspector
    public Sprite fullHeart;            // Assign in Inspector
    public Sprite emptyHeart;           // Assign in Inspector

    private PlayerHealth playerHealth;

    // cache to avoid unnecessary redraws
    private int lastLives   = int.MinValue;
    private int lastMaxLives = int.MinValue;

    private void Start()
    {
        // Try to bind immediately (works if objects already exist)
        TryBind();
        // Force one draw if we already found the player
        if (playerHealth != null)
            ForceRefresh();
    }

    private void Update()
    {
        // If we haven’t bound yet (spawn order), keep trying
        if (playerHealth == null)
        {
            TryBind();
            if (playerHealth == null) return; // still not found this frame
            ForceRefresh(); // just bound — draw once
        }

        // Poll the networked values; only redraw if something changed
        int lives = playerHealth.currentLives.Value;
        int maxLives = playerHealth.maxLives;

        if (lives != lastLives || maxLives != lastMaxLives)
        {
            UpdateHearts(lives, maxLives);
            lastLives = lives;
            lastMaxLives = maxLives;
        }
    }

    private void TryBind()
    {
        var players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (var ph in players)
        {
            if (ph.playerId == playerId)
            {
                playerHealth = ph;
                break;
            }
        }

        if (playerHealth == null)
        { 
            Debug.LogWarning($"PlayerLivesUI: No PlayerHealth found with ID: {playerId}");
        }
    }

    private void ForceRefresh()
    {
        lastLives = int.MinValue; // force UpdateHearts to run
        lastMaxLives = int.MinValue;
    }

    private void UpdateHearts(int lives, int maxLives)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // show/hide slots based on max lives (if you want to hide extras)
            heartImages[i].enabled = i < maxLives;

            if (!heartImages[i].enabled) continue;

            heartImages[i].sprite = (i < lives) ? fullHeart : emptyHeart;
        }
    }
}
