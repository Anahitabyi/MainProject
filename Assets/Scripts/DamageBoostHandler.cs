using System.Collections;
using UnityEngine;

public class DamageBoostHandler : MonoBehaviour
{
    private bool isBoosting = false;

    public void ApplyBoost(GameObject player, int boostAmount, float duration)
    {
        if (!isBoosting)
            StartCoroutine(DamageBoostCoroutine(player, boostAmount, duration));
    }

    private IEnumerator DamageBoostCoroutine(GameObject player, int boostAmount, float duration)
    {
        isBoosting = true;

        int originalDamage = 0;

        // Try melee
        var melee = player.GetComponent<meleePlayerMovement>();
        if (melee != null)
        {
            originalDamage = melee.attackDamage;
            melee.attackDamage += boostAmount;
            Debug.Log("Melee damage boosted to: " + melee.attackDamage);
            yield return new WaitForSeconds(duration);
            melee.attackDamage = originalDamage;
            Debug.Log("Melee damage reset to: " + melee.attackDamage);
        }

        // Try ranged
        var ranged = player.GetComponent<playerMovement>();
        if (ranged != null)
        {
            originalDamage = ranged.attackDamage;
            ranged.attackDamage += boostAmount;
            Debug.Log("Ranged damage boosted to: " + ranged.attackDamage);
            yield return new WaitForSeconds(duration);
            ranged.attackDamage = originalDamage;
            Debug.Log("Ranged damage reset to: " + ranged.attackDamage);
        }

        isBoosting = false;
    }
}
