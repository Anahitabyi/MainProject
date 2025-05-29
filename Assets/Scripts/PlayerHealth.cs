using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 9;
    public int currentHealth;

    public AudioClip damageSound;

    public Animator animator; // Assign in Inspector
    private bool isDead = false; // Prevent multiple deaths

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void takeDamge(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die(); // Only play death animation if dead
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hurt"); // Only play Hurt if still alive
            }
        }

        // Optionally play damage sound
        // AudioSource.PlayClipAtPoint(damageSound, transform.position);
    }

    public void heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player healed. Current health: " + currentHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died.");

        if (animator != null)
        {
            animator.SetTrigger("Die"); // Trigger death animation
        }

        // Optionally: Disable player movement here
        // GetComponent<PlayerMovement>().enabled = false;

        StartCoroutine(RemoveAfterDeathAnimation());
    }

    private IEnumerator RemoveAfterDeathAnimation()
    {
        // Wait until the death animation starts playing
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Death"))
        {
            yield return null;
        }

        // Wait for the duration of the death animation
        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathDuration);

        // Hide or disable the player
        gameObject.SetActive(false);
    }
}
