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
    public MonoBehaviour movementScriptMono;

    private IPlayerInputBlocker inputBlocker;
    private bool isDead = false;
    private bool isInvincible = false;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnLivesChanged;
    public int playerId = 1;

    void Start()
{
    inputBlocker = movementScriptMono as IPlayerInputBlocker;

    if (playerStatsManager.Instance != null)
    {
        playerStatsManager.Instance.LoadIntoPlayer(this);
    }

    OnHealthChanged?.Invoke(currentHealth, maxHealth);
    OnLivesChanged?.Invoke(currentLives, maxLives);
}


    public void TakeDamage(int damage)
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
        playerStatsManager.Instance?.SaveFromPlayer(this);
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
            StartCoroutine(RespawnAfterDelay(4f));
        }
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    private void Die(bool final)
    {
        isDead = true;
        isInvincible = true;

        animator?.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = false;
        playerCollider.enabled = false;

        if (inputBlocker != null) inputBlocker.isInputBlocked = true;

        if (final)
        {
            StartCoroutine(LoadGameOverAfterDeathAnimation());
        }
    }


    private IEnumerator RespawnAfterDelay(float delay)
    {
        Die(final: false);
        StartCoroutine(FlashDuringInvincibility());
        yield return new WaitForSeconds(delay);

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        playerCollider.enabled = true;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (inputBlocker != null) inputBlocker.isInputBlocked = false;

        isDead = false;

        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    private IEnumerator FlashDuringInvincibility()
    {
        isInvincible = true;

        if (spriteRenderer == null)
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private IEnumerator RemoveAfterDeathAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            yield return null;

        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        gameObject.SetActive(false);
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    public void AddHealth(int amount)
    {
        if (amount <= 0 || isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    public void AddLives(int amount)
    {
        if (amount <= 0 || isDead) return;

        currentLives += amount;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        OnLivesChanged?.Invoke(currentLives, maxLives);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }
    private IEnumerator LoadGameOverAfterDeathAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            yield return null;

        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        // Optional: reset stats if you want
        // playerStatsManager.Instance?.ResetAllStats();

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }


    public bool IsDead() => isDead;
    public bool IsInvincible() => isInvincible;
}
