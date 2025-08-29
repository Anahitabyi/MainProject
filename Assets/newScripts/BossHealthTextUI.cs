using TMPro;
using UnityEngine;
using Unity.Netcode;

public class BossHealthTextUI : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossEnemy boss;
    public TextMeshProUGUI healthText;

    void Start()
    {
        if (boss != null && healthText != null)
        {
            boss.currentHealth.OnValueChanged += OnHealthChanged;
            OnHealthChanged(boss.maxHealth, boss.currentHealth.Value); // init
        }
    }

    private void OnHealthChanged(float previous, float current)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.Max(0, current)}/{boss.maxHealth}";
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