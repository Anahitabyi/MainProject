using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [Header("References")]
    public BossEnemy boss;       // مرجع به اسکریپت BossEnemy
    public Slider healthSlider;  // اسلایدر برای نمایش سلامتی

    void Start()
    {
        if (boss != null && healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;
        }
    }

    void Update()
    {
        if (boss != null && healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp(boss.GetCurrentHealth(), 0, boss.maxHealth);
        }
    }
}