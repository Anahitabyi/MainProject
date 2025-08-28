using UnityEngine;
using Unity.Netcode;

public class CatEnemyHealth : EnemyHealth
{
    [Header("Cat Enemy Effects")]
    public GameObject deathEffectPrefab; // Assign in Inspector

    protected override void Die()
    {
        Debug.Log("CatEnemy has died!"); // Confirm that Die() is called

        base.Die(); // Call base death logic (likely handles disabling/spawning etc.)

        if (deathEffectPrefab == null)
        {
            Debug.LogWarning("No deathEffectPrefab assigned to CatEnemy.");
            return;
        }

        if (IsServer)
        {
            NetworkObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity)
                .GetComponent<NetworkObject>();

            effect.Spawn(); // Sync with clients
            Debug.Log("Death effect instantiated.");

            float lifetime = 2f; // fallback

            Animator anim = effect.GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                // Grab the first clip length
                AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
                if (clips.Length > 0) lifetime = clips[0].length;
            }
            else
            {
                Debug.LogWarning("No Animator found on death effect. Using fallback lifetime.");
            }

            StartCoroutine(DespawnAfter(effect, lifetime));
        }
    }

    private System.Collections.IEnumerator DespawnAfter(NetworkObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.IsSpawned) obj.Despawn(true);
    }
}
