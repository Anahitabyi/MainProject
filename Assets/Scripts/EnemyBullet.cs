using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
    {
        Debug.Log("Bullet hit player!");
        PlayerHealth health = collision.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.takeDamge(1);
        }
        Destroy(gameObject);
    }
        else if (!collision.isTrigger) // Optional: destroy on hitting anything solid
        {
            Destroy(gameObject);
        }
    }
}
