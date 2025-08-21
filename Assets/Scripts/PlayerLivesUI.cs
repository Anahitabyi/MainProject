using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    public int playerId = 1;                  // Set this in the Inspector (1 or 2)
    public Image[] heartImages;              // Assign in Inspector
    public Sprite fullHeart;                 // Assign in Inspector
    public Sprite emptyHeart;                // Assign in Inspector

    private PlayerHealth playerHealth;

    void Start()
{
    // Find the correct PlayerHealth component based on playerId
    PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
    foreach (PlayerHealth ph in players)
    {
        if (ph.playerId == playerId)
        {
            playerHealth = ph;
            break;
        }
    }

    if (playerHealth != null)
    {
        playerHealth.OnLivesChanged += UpdateHearts;
        UpdateHearts(playerHealth.currentLives.Value, playerHealth.maxLives);
    }
    else
    {
        Debug.LogWarning("PlayerLivesUI: No PlayerHealth found with ID: " + playerId);
    }
}


    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateHearts;
        }
    }

    void UpdateHearts(int lives, int maxLives)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < lives)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }

            heartImages[i].enabled = i < maxLives;
        }
    }
}
