using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 9;
    public int currentHealth;
    //HealthUI healthUI;
    public AudioClip damageSound;

    void start()
    {
        currentHealth = maxHealth;
    }

    public void takeDamge(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Player took damage. Current health: " + currentHealth);
        //healthUI.update();
    }

    public void heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Player healed. Current health: " + currentHealth);
        //healthUI.update();
    }
}
