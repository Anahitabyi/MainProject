using UnityEngine;

public class BombThrower : MonoBehaviour
{
    public float spawnInterval = 5f;
    public float bombSpeed = 20f;

    public enum Direction { Up, Down, Left, Right }
    public Direction throwDirection = Direction.Down;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            GameObject bomb = BombPool.Instance.GetBomb();
            bomb.transform.position = transform.position;

            Vector2 dir = GetDirectionVector();
            Bomb bombScript = bomb.GetComponent<Bomb>();
            if (bombScript != null)
            {
                bombScript.SetMovement(dir, bombSpeed);
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