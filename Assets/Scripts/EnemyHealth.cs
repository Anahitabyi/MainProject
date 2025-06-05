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

        if (currentHealth <= 0)
        {
            Die();                            // Call death logic if health reaches 0 or below
        }
    }

    // Called when the enemy dies
    protected virtual void Die()
    {
        isDead = true;                        // Mark enemy as dead
        Debug.Log($"{gameObject.name} has died."); // Log death event

        animator.SetTrigger("Die");           // Trigger the 'Die' animation

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;         // Disable the collider so it doesn't interact anymore

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero; // Stop all movement
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze physics to stop interactions
        }

        Destroy(gameObject, 0.5f);            // Destroy the enemy object after the animation plays
    }
}
