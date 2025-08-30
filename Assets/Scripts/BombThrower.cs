using UnityEngine;
using Unity.Netcode;

public class BombThrower : NetworkBehaviour
{
    public float spawnInterval = 5f;
    public float bombSpeed = 20f;

    public enum Direction { Up, Down, Left, Right }
    public Direction throwDirection = Direction.Down;

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
        return throwDirection switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            _ => Vector2.zero
        };
    }
}