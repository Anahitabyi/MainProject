using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public int playerId = 1;  // Set in Inspector: 1 for Player 1, 2 for Player 2
    public Slider healthSlider; // Set in Inspector

    private PlayerHealth playerHealth;

    void Start()
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        // Find the correct PlayerHealth instance by ID
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
            playerHealth.OnHealthChanged += UpdateSlider;
            UpdateSlider(playerHealth.currentHealth.Value, playerHealth.maxHealth);
        }
        else
        {
            Debug.LogWarning("No PlayerHealth found with ID: " + playerId);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateSlider;
        }
    }

    private void UpdateSlider(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}
