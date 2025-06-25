using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public bool IsDead => isDead; // Add this line at the class level (public getter)

    [Header("Health Settings")]
    public int maxHealth = 100;               // Maximum health of the enemy
    protected int currentHealth;              // Current health that changes during gameplay

    [Header("Animation")]
    public Animator animator;                 // Animator to trigger hit and death animations
    protected bool isDead = false;            // Flag to prevent multiple deaths

    protected virtual void Start()
    {
        currentHealth = maxHealth;            // Initialize health at the start
    }

    // Called when the enemy takes damage
    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;                   // If already dead, ignore further damage

        currentHealth -= damageAmount;        // Reduce current health by damage amount
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current HP: {currentHealth}");

        animator.SetTrigger("Hit");           // Trigger the 'Hit' animation
        // SaveIfHasID();
        if (currentHealth <= 0)
        {
            Die();                            // Call death logic if health reaches 0 or below
        }
    }

    // Called when the enemy dies
    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");

        // ✅ Let another script handle special behavior
        IPooledDeathHandler deathHandler = GetComponent<IPooledDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.OnDeath(); // Delegates to patrollingEnemy
        }

        // ✅ Save defeated enemy ID if applicable
        GenerateID unique = GetComponent<GenerateID>();
        if (unique != null)
        {
            Debug.Log("Found the id!");
            SaveTracker.Instance.MarkEnemyDefeated(unique.Id);
        }

        animator.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        // SaveIfHasID();
        Destroy(gameObject, 0.5f);
    }
    public void SetHealth(int hp)
        {
            currentHealth = hp;
            isDead = hp <= 0;
        }



    private bool hasBeenKilled = false;

public void KillImmediately()
{
    if (hasBeenKilled) return;
    hasBeenKilled = true;

    isDead = true;

    Collider2D col = GetComponent<Collider2D>();
    if (col) col.enabled = false;

    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb)
    {
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    Destroy(gameObject);
}

public int CurrentHealth => currentHealth;


}
public interface IPooledDeathHandler
{
    void OnDeath();
}

