using UnityEngine;
using Unity.Netcode;

public class CatThrower : NetworkBehaviour
{
    /*public float spawnInterval = 5f;
    public float bombSpeed = 20f;

    public Vector2 direction;

    private float timer;

    void Update()
    {
        // Only the server spawns bombs
        if (!IsServer) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            // BombPool's Instantiate method
            var bombNetObj = BombPool.Instance.Instantiate(OwnerClientId, transform.position, Quaternion.identity);
            Bomb bomb = bombNetObj.GetComponent<Bomb>();
            if (bomb != null)
            {
                Vector2 dir = GetDirectionVector();
                bomb.SetMovement(dir, bombSpeed);
            }

            timer = 0f;
        }
    }

    private Vector2 GetDirectionVector()
    { 
        return Vector2.right * direction.x;
    }*/
}