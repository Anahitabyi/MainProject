using UnityEngine;
using System.Collections.Generic;
public class PushAngel : MonoBehaviour
{
    public float pushSpeed = 2f;
    private Rigidbody2D rb;

    private HashSet<GameObject> pushingPlayers = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 100f; // allows it to fall in holes
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // Only allow push if exactly two players are in contact and pushing
        if (pushingPlayers.Count == 2)
        {
            Vector2 pushDirection = Vector2.zero;

            foreach (GameObject player in pushingPlayers)
            {
                float input = GetPlayerPushInput(player);
                pushDirection += new Vector2(input, 0f);
            }

            if (pushDirection != Vector2.zero)
            {
                rb.linearVelocity = pushDirection.normalized * pushSpeed;
                return;
            }
        }

        // If one or both aren't pushing anymore, stop movement
        rb.linearVelocity = Vector2.zero;
    }

    private float GetPlayerPushInput(GameObject player)
    {
        // Adjust this based on how you distinguish input per player
        if (player.name.Contains("1"))
            return Input.GetAxisRaw("HorizontalP1");
        else if (player.name.Contains("2"))
            return Input.GetAxisRaw("HorizontalP2");
        else
            return 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            pushingPlayers.Add(collision.gameObject);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            pushingPlayers.Remove(collision.gameObject);
    }
}