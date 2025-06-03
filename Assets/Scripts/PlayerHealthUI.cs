using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;  // Assign in Inspector
    public Slider healthSlider;        // Assign in Inspector

    void Start()
    {
        if (playerHealth != null)
        {
            // ✅ Subscribe to the event
            playerHealth.OnHealthChanged += UpdateSlider;

            // Initial update
            UpdateSlider(playerHealth.currentHealth, playerHealth.maxHealth);
        }
    }

    void OnDestroy()
    {
        // ✅ Unsubscribe to avoid memory leaks
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
