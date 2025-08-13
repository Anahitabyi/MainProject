using UnityEngine;

public class Trampoline : MonoBehaviour
{
    private bool onTop;
    public float bouncePower;
    private Animator anim;
    private GameObject bouncer;

    private bool hasBounced = false;
    private float bounceCooldown = 1f; // cooldown duration in seconds
    private float bounceTimer = 0f;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Handle cooldown timer
        if (hasBounced)
        {
            bounceTimer += Time.deltaTime;
            if (bounceTimer >= bounceCooldown)
            {
                hasBounced = false;
                bounceTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            onTop = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            onTop = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (onTop && !hasBounced && other.gameObject.CompareTag("Player"))
        {
            bouncer = other.gameObject;
            hasBounced = true;

            if (anim != null)
            {
                anim.SetTrigger("Bounce"); // Animation will call jump() via animation event
                jump();
            }
        }
    }

    // Called by animation event
    public void jump()
    {
        Debug.Log("called jump trampoline!");
        if (bouncer != null)
        {
            PlayerControllerNew pc = bouncer.GetComponent<PlayerControllerNew>();
            PlayerControllerNew2 pc2 = bouncer.GetComponent<PlayerControllerNew2>();
            if (pc != null)
            {
                pc.ExecuteBounce(bouncePower);
            }
            if (pc2 != null)
            {
                pc2.ExecuteBounce(bouncePower);
            }
        }
    }
}
