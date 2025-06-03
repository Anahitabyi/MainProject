using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health & Lives")]
    public int maxHealth = 9;
    public int currentHealth;
    public int maxLives = 3;
    public int currentLives;

    [Header("Invincibility")]
    public float invincibilityDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("References")]
    public Animator animator;
    public Collider2D playerCollider;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public meleePlayerMovement movementScript;

    private bool isDead = false;
    private bool isInvincible = false;

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
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            LoseLife();
        }
        else
        {
            animator?.SetTrigger("Hurt");
        }
    }

    private void LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        OnLivesChanged?.Invoke(currentLives, maxLives);

        if (currentLives <= 0)
        {
            Die(final: true);
        }
        else
        {
            StartCoroutine(FlashDuringInvincibility());
            StartCoroutine(RespawnAfterDelay(4f));
        }
    }

    private void Die(bool final)
    {
        isDead = true;
        isInvincible = true;

        animator?.SetTrigger("Die");

        movementScript.isInputBlocked = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = false;
        playerCollider.enabled = false;

        StartCoroutine(RemoveAfterDeathAnimation());
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        isDead = true;
        isInvincible = true;

        animator?.SetTrigger("Die");

        movementScript.isInputBlocked = true;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = false;
        playerCollider.enabled = false;

        yield return new WaitForSeconds(delay);

        // Respawn logic
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        playerCollider.enabled = true;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        movementScript.isInputBlocked = false;


        yield return new WaitForSeconds(2f);

        isInvincible = false;
        isDead = false;
    }

    private IEnumerator FlashDuringInvincibility()
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.enabled = true;
    }

    private IEnumerator RemoveAfterDeathAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            yield return null;

        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        gameObject.SetActive(false); // Or trigger game over, reload level, etc.
    }


    public void setHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    } 
    public void AddLives(int amount)
    {
    // if (amount <= 0 || isDead) return;

    currentLives += amount;
    currentLives = Mathf.Clamp(currentLives, 0, maxLives);
    OnLivesChanged?.Invoke(currentLives, maxLives);
    }

    public void AddHealth(int amount)
    {
    // if (amount <= 0 || isDead) return;

    currentHealth += amount;
    //Debug.Log("Added the heealth.");
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    public bool IsDead() => isDead;
    public bool IsInvincible() => isInvincible;
}
