using UnityEngine;
using Unity.Netcode;

public class Bomb : NetworkBehaviour
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
        // move only on server, then use NetworkTransform
        if (IsServer)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return; // Only server handles stuff

        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(2); 
            }
        }
        // Despawn the bomb via Netcode, which triggers pool handler
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Despawn();
        }
    }
}