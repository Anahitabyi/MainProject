using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GhostSpawner : MonoBehaviour
{
    public GameObject ghostPrefab;
    public Transform spawnPoint;           // Empty object where ghost spawns
    public Image jumpScareImage;           // UI Image object
    public float imageDisplayTime = 2f;    // How long the image stays
    public float ghostDelay = 1f;          // Delay after image disappears before ghost spawns
    public AudioClip screamClip;

    private bool triggered = false;
    private AudioSource screamAudioSource;

    void Start()
    {
        // Create a new AudioSource dynamically
        screamAudioSource = gameObject.AddComponent<AudioSource>();
        screamAudioSource.playOnAwake = false;
        screamAudioSource.clip = screamClip;
    }

    void OnTriggerEnter2D(Collider2D other) // Use OnTriggerEnter for 3D
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(TriggerJumpScareSequence());
        }
    }

    IEnumerator TriggerJumpScareSequence()
    {
        // Show jumpscare image
        jumpScareImage.gameObject.SetActive(true);

        // Play scream sound if clip is assigned
        if (screamAudioSource.clip != null)
            screamAudioSource.Play();

        // Wait while image is shown
        yield return new WaitForSeconds(imageDisplayTime);

        // Hide image
        jumpScareImage.gameObject.SetActive(false);

        // Wait before spawning ghost
        yield return new WaitForSeconds(ghostDelay);

        // Spawn ghost
        Instantiate(ghostPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}