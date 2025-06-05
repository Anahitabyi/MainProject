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
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("Death effect instantiated."); // Confirm that effect was spawned

            Animator anim = effect.GetComponent<Animator>();
            if (anim != null)
            {
                float animTime = anim.GetCurrentAnimatorStateInfo(0).length;
                Destroy(effect, animTime);
            }
            else
            {
                Destroy(effect, 2f); // Fallback time if Animator not found
                Debug.LogWarning("No Animator found on death effect. Destroying after 2 seconds.");
            }
        }
        else
        {
            Debug.LogWarning("No deathEffectPrefab assigned to CatEnemy."); // Warning if prefab missing
        }
    }
}
