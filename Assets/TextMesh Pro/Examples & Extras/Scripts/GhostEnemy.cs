using UnityEngine;

public class GhostEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseDistance = 10f;

    [Header("Player References")]
    public Transform player1;
    public Transform player2;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private bool hasAppeared = false;
    private bool isChasing = false;
    private Vector2 movement;

    void Update()
    {
        if (!hasAppeared)
        {
            Transform closestPlayer = GetClosestPlayer();
            if (closestPlayer != null)
            {
                float distance = Vector2.Distance(transform.position, closestPlayer.position);
                if (distance <= chaseDistance)
                {
                    TriggerAppear();
                }
            }
            return;
        }

        if (!isChasing || (player1 == null && player2 == null))
        {
            movement = Vector2.zero;
            return;
        }

        Transform target = GetClosestPlayer();
        if (target == null)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 direction = target.position - transform.position;

        if (direction.magnitude <= chaseDistance)
        {
            direction.Normalize();
            movement = direction;

            // Flip sprite (reversed)
            if (movement.x > 0.01f)
                spriteRenderer.flipX = true;
            else if (movement.x < -0.01f)
                spriteRenderer.flipX = false;
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void TriggerAppear()
    {
        if (hasAppeared) return;

        hasAppeared = true;
        animator?.SetTrigger("Appear");
    }

    // Called by Animation Event at end of "Appear" animation
    public void OnAppearFinished()
    {
        isChasing = true;
        animator?.SetBool("IsChasing", true);
    }

    Transform GetClosestPlayer()
    {
        if (player1 == null) return player2;
        if (player2 == null) return player1;

        float dist1 = Vector2.Distance(transform.position, player1.position);
        float dist2 = Vector2.Distance(transform.position, player2.position);

        return (dist1 < dist2) ? player1 : player2;
    }
}
