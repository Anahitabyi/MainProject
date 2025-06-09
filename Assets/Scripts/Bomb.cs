using UnityEngine;

public class Bomb : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;

    public void SetMovement(Vector2 dir, float spd)
    {
        moveDirection = dir.normalized;
        speed = spd;
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(2);
            }
        }

        BombPool.Instance.ReturnBomb(gameObject);
    }
}