using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public int playerId = 1;
    public Slider healthSlider;

    private PlayerHealth playerHealth;
    

    void Update()
    {
        // Lazy init: keep trying until we find the right player
        if (playerHealth == null)
        {
            PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (PlayerHealth ph in players)
            {
                if (ph.playerId == playerId)
                {
                    playerHealth = ph;

                    // subscribe once
                    playerHealth.currentHealth.OnValueChanged += OnHealthValueChanged;

                    // initialize immediately
                    healthSlider.maxValue = playerHealth.maxHealth;
                    healthSlider.value = playerHealth.currentHealth.Value;
                    break;
                }
            }
        }
        else
        {
            // Safety net: keep UI in sync even if event missed
            healthSlider.value = playerHealth.currentHealth.Value;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.currentHealth.OnValueChanged -= OnHealthValueChanged;
    }

    private void OnHealthValueChanged(int oldVal, int newVal)
    {
        healthSlider.value = newVal;
    }
}