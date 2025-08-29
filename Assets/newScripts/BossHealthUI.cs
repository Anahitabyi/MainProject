using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    public BossEnemy boss;       // Reference to BossEnemy
    public Slider healthSlider;  // UI slider

    void Start()
    {
        if (boss != null && healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;

            // Subscribe to networked health changes
            boss.currentHealth.OnValueChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(float previous, float current)
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp(current, 0, boss.maxHealth);
        }
    }

    void OnDestroy()
    {
        if (boss != null)
        {
            boss.currentHealth.OnValueChanged -= OnHealthChanged;
        }
    }
}