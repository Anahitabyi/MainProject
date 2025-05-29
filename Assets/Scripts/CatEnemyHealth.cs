using UnityEngine;

public class CatEnemyHealth : EnemyHealth
{
    [Header("Cat Enemy Effects")]
    public GameObject deathEffectPrefab; // Assign in Inspector

    protected override void Die()
    {
        Debug.Log("CatEnemy has died!"); // Confirm that Die() is called

        base.Die(); // Call base death logic

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("Death effect instantiated."); // Confirm that effect was spawned
        }
        else
        {
            Debug.LogWarning("No deathEffectPrefab assigned to CatEnemy."); // Warning if prefab missing
        }
    }
}
