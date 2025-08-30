using UnityEngine;
using Unity.Netcode;
using System;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public int CurrentHealth => currentHealth.Value;
    protected bool isDead = false;
    public bool IsDead => isDead;

    public Animator animator;

    private void Start()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;

        // Listen for health changes (optional, for client-side UI)
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private new void OnDestroy()
    {
        if (currentHealth != null)
            currentHealth.OnValueChanged -= OnHealthChanged;
    }


    // Called on both server and clients to apply damage
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (IsServer)
        {
            ApplyDamage(damage);
        }
        else
        {
            TakeDamageServerRpc(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        if (!isDead)
            ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        currentHealth.Value -= damage;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        animator?.SetTrigger("Hit");

        if (currentHealth.Value <= 0)
            Die();
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        // Optional: client-side effects like health bars, flash, etc.
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        animator?.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Delegate special behavior if needed
        IPooledDeathHandler deathHandler = GetComponent<IPooledDeathHandler>();
        deathHandler?.OnDeath();

        Destroy(gameObject, 0.5f);
    }

    public void SetHealth(int hp)
    {
        if (IsServer)
        {
            currentHealth.Value = Mathf.Clamp(hp, 0, maxHealth);
            if (currentHealth.Value <= 0) Die();
        }
        else
        {
            SetHealthServerRpc(hp);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(int hp, ServerRpcParams rpcParams = default)
    {
        currentHealth.Value = Mathf.Clamp(hp, 0, maxHealth);
        if (currentHealth.Value <= 0) Die();
    }

    public void KillImmediately()
    {
        if (isDead) return;
        isDead = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        Destroy(gameObject);
    }
}

public interface IPooledDeathHandler
{
    void OnDeath();
}

