using TMPro;
using UnityEngine;

public class BossHealthTextUI : MonoBehaviour
{
    [Header("Boss Settings")]
    public BossEnemy boss;               // رفرنس به اسکریپت باس
    public TextMeshProUGUI healthText;   // رفرنس به TextMeshPro

    void Update()
    {
        if (boss != null && healthText != null)
        {
            healthText.text = $"HP: {Mathf.Max(0, boss.GetCurrentHealth())}/{boss.maxHealth}";
        }
    }
}