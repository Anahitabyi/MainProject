using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 9;
    public int currentHealth;

    public int maxLives = 3;
    public int currentLives;

    public AudioClip damageSound;

    public Animator animator;
    private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnLivesChanged;

    void Start()
    {
        currentLives = maxLives;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }

    public void takeDamge(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Player took damage. Current health: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            LoseLife();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void setHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void LoseLife()
{
    Debug.Log($"LoseLife called. Lives before: {currentLives}");

    currentLives = Mathf.Max(0, currentLives - 1);
    Debug.Log($"Lives after decrement: {currentLives}");

    OnLivesChanged?.Invoke(currentLives, maxLives);

    if (currentLives <= 0)
    {
        Die(true);
    }
    else
    {
        StartCoroutine(RespawnAfterDelay(1f));
    }
}


    private IEnumerator RespawnAfterDelay(float delay)
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        yield return new WaitForSeconds(delay);

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        isDead = false;
        gameObject.SetActive(true); // Reactivate if needed
    }

    private void Die(bool final)
    {
        isDead = true;
        Debug.Log("Player is permanently dead.");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        StartCoroutine(RemoveAfterDeathAnimation());
    }

    private IEnumerator RemoveAfterDeathAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
        {
            yield return null;
        }

        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        gameObject.SetActive(false); // Or call a respawn manager here
    }
}
