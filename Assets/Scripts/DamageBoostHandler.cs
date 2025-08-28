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
        var melee2 = player.GetComponent<PlayerControllerNew2>();
        if (melee2 != null)
        {
            originalDamage = melee2.attackDamage;
            melee2.attackDamage += boostAmount;
            Debug.Log("Melee damage boosted to: " + melee2.attackDamage);
            yield return new WaitForSeconds(duration);
            melee2.attackDamage = originalDamage;
            Debug.Log("Melee damage reset to: " + melee2.attackDamage);
        }
        var ranged2 = player.GetComponent<PlayerControllerNew>();
        if (ranged2 != null)
        {
            originalDamage = ranged2.attackDamage;
            ranged2.attackDamage += boostAmount;
            Debug.Log("Ranged damage boosted to: " + ranged2.attackDamage);
            yield return new WaitForSeconds(duration);
            ranged2.attackDamage = originalDamage;
            Debug.Log("Ranged damage reset to: " + ranged2.attackDamage);
        }

        isBoosting = false;
    }
}
