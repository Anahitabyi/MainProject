using UnityEngine;
using UnityEngine.SceneManagement;

public class FootstepAudio : MonoBehaviour
{
    public AudioSource audioSource;
    
    public float stepInterval = 0.5f; //footstep timing
    private float stepTimer;
    
    public AudioClip[] stoneFootsteps;
    
    public AudioClip[] grassFootsteps;

    [Header("Current Surface Type (Auto-set per scene)")]
    public SurfaceType surface = SurfaceType.Grass;

    private Rigidbody2D rb;

    public enum SurfaceType { Grass, Stone }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals("Level1"))
            surface = SurfaceType.Grass;
        else if (sceneName.Equals("Level2"))
            surface = SurfaceType.Stone;
    }

    void Update()
    {
        if (IsMoving())
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private bool IsMoving()
    {
        return Mathf.Abs(rb.linearVelocity.x) > 0.1f;
    }

    private void PlayFootstep()
    {
        AudioClip[] clips = surface == SurfaceType.Stone ? stoneFootsteps : grassFootsteps;
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}