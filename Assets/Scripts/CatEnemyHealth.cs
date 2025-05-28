using UnityEngine;

public class CatEnemyHealth : EnemyHealth
{
    [Header("Cat Enemy Effects")]
    public GameObject deathEffectPrefab; // Assign in Inspector

    protected override void Die()
    {
        base.Die(); // Call base death logic

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
