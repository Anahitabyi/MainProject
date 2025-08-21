using UnityEngine;
using System.Collections;
using System;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health & Lives")]
    public int maxHealth = 9;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public int maxLives = 3;
    public NetworkVariable<int> currentLives = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
    public int playerId = 1; // Keep for UI/logic

    private void Start()
    {
        if (!IsOwner)
            return;

        inputBlocker = movementScriptMono as IPlayerInputBlocker;

        if (playerStatsManager.Instance != null)
        {
            { playerStatsManager.Instance.LoadIntoPlayer(this);}
            
        }

        // Sync initial state
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            currentLives.Value = maxLives;
        }

        // Listen for changes
        currentHealth.OnValueChanged += (oldVal, newVal) => OnHealthChanged?.Invoke(newVal, maxHealth);
        currentLives.OnValueChanged += (oldVal, newVal) => OnLivesChanged?.Invoke(newVal, maxLives);

        OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
        OnLivesChanged?.Invoke(currentLives.Value, maxLives);
    }

    // Add this Update for networked debug log
    private void Update()
    {
         if (!IsOwner)
            return;
        // This will print the health and lives for each player object in every editor window
        Debug.Log($"[Networked][PlayerId:{playerId}][IsOwner:{IsOwner}] Health: {currentHealth.Value} / {maxHealth} | Lives: {currentLives.Value} / {maxLives}");
    }

    public void TakeDamage(int damage)
    {
        if (!IsOwner) return; // Only owner can initiate damage
        if (isDead || isInvincible) return;

        TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    private void TakeDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        if (isDead || isInvincible) return;
        currentHealth.Value -= damage;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        // Notify owner for animation/logic
        TakeDamageClientRpc(currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            LoseLifeServerRpc();
        }
        else
        {
            // Animation trigger for owner
            TriggerHurtClientRpc();
        }
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int newHealth)
    {
        currentHealth.Value = newHealth;
        OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
    }

    [ClientRpc]
    private void TriggerHurtClientRpc()
    {
        if (IsOwner)
        {
            animator?.SetTrigger("Hurt");
        }
    }

    [ServerRpc]
    private void LoseLifeServerRpc(ServerRpcParams rpcParams = default)
    {
        currentLives.Value = Mathf.Max(0, currentLives.Value - 1);
        OnLivesChanged?.Invoke(currentLives.Value, maxLives);

        if (currentLives.Value <= 0)
        {
            DieServerRpc(final: true);
        }
        else
        {
            StartCoroutine(RespawnAfterDelay(4f));
        }
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    [ServerRpc]
    private void DieServerRpc(bool final, ServerRpcParams rpcParams = default)
    {
        isDead = true;
        isInvincible = true;

        DieClientRpc(final);

        if (final)
        {
            StartCoroutine(LoadGameOverAfterDeathAnimation());
        }
    }

    [ClientRpc]
    private void DieClientRpc(bool final)
    {
        animator?.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = false;
        playerCollider.enabled = false;

        if (inputBlocker != null) inputBlocker.isInputBlocked = true;
        isDead = true;
        isInvincible = true;
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        DieServerRpc(final: false);
        StartCoroutine(FlashDuringInvincibility());
        yield return new WaitForSeconds(delay);

        RespawnServerRpc();
    }

    [ServerRpc]
    private void RespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        currentHealth.Value = maxHealth;
        playerCollider.enabled = true;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (inputBlocker != null) inputBlocker.isInputBlocked = false;

        isDead = false;
        isInvincible = false;

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
        if (!IsOwner) return;
        SetHealthServerRpc(newHealth);
    }

    [ServerRpc]
    private void SetHealthServerRpc(int newHealth, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    public void AddHealth(int amount)
    {
        if (!IsOwner) return;
        if (amount <= 0 || isDead) return;
        AddHealthServerRpc(amount);
    }

    [ServerRpc]
    private void AddHealthServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value += amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    public void AddLives(int amount)
    {
        if (!IsOwner) return;
        if (amount <= 0 || isDead) return;
        AddLivesServerRpc(amount);
    }

    [ServerRpc]
    private void AddLivesServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        currentLives.Value += amount;
        currentLives.Value = Mathf.Clamp(currentLives.Value, 0, maxLives);
        playerStatsManager.Instance?.SaveFromPlayer(this);
    }

    private IEnumerator LoadGameOverAfterDeathAnimation()
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
            yield return null;

        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }

    public bool IsDead() => isDead;
    public bool IsInvincible() => isInvincible;
}