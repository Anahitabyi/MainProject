using System.Collections;
using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public int damage = 1;
    public float damageCooldown = 1f; // Delay between each damage tick

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D col)
    {
        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null && CanDamage(player))
        {
            player.takeDamge(damage);
            StartCoroutine(DamageCooldown(player));
        }
    }

    private bool CanDamage(PlayerHealth player)
    {
        return !player.gameObject.GetComponent<TrapDamageCooldown>(); // Only damage if not on cooldown
    }

    private IEnumerator DamageCooldown(PlayerHealth player)
    {
        player.gameObject.AddComponent<TrapDamageCooldown>(); // Mark player as on cooldown
        yield return new WaitForSeconds(damageCooldown);
        Destroy(player.gameObject.GetComponent<TrapDamageCooldown>()); // Remove cooldown marker
    }

    // Small helper class to mark cooldown
    private class TrapDamageCooldown : MonoBehaviour {}
}
