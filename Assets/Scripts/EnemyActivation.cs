using UnityEngine;
using UnityEngine.Audio;

public class EnemyActivator : MonoBehaviour
{
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    public Animator anim;
    public patrollingEnemy patrolScript;
    public AudioSource audioSource;
    public AudioClip appearSound;
    public AudioMixerGroup sfxMixerGroup;

    private bool isActivated = false;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (patrolScript != null)
            patrolScript.enabled = false;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
            if (!isActivated)
            {
                Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
                if (player != null)
                {
                    Debug.Log("Player detected! Activating ghost.");
                    isActivated = true;
                    anim.SetTrigger("Appear");

                    if (appearSound != null)
                        audioSource.PlayOneShot(appearSound);

                    Invoke(nameof(ActivatePatrolling), 1f); // Match the appear animation
                }
                else
                {
                    Debug.Log("No player detected.");
                }
            }

    }

    void ActivatePatrolling()
    {
        Debug.Log("Patrol activated!");
        if (patrolScript != null)
            patrolScript.enabled = true; if (patrolScript != null)
            patrolScript.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}