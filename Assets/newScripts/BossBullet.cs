using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 10;

    [Header("Layers")]
    public LayerMask playerLayer;

    [HideInInspector]
    public BossEnemy bossEnemy;

    private void Start()
    {
        // Destroy bullet after some time to avoid memory leaks
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Move bullet forward based on facing direction
         transform.position += transform.up * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Try to deal damage to the player
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null && bossEnemy != null)
        {
            if(bossEnemy.Registerdamage(collision.transform)){
                //Debug.Log("pplayer took damage.");
            player.TakeDamage(damage);
            
            }
            
        }
        Destroy(gameObject);

        // If you want to also destroy bullet on hitting walls/ground, add checks here
    }
}
}