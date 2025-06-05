using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class FootstepAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public float stepInterval = 0.5f;
    private float stepTimer;

    public AudioClip[] stoneFootsteps;
    public AudioClip[] grassFootsteps;

    private bool isGrounded;
    private bool wasGrounded;

    public AudioClip jumpSound;
    public AudioClip landGrassSound;
    public AudioClip landStoneSound;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Current Surface Type (Auto-set per scene)")]
    public SurfaceType surface = SurfaceType.Grass;

    private Rigidbody2D rb;
    public Animator animator;

    public enum SurfaceType { Grass, Stone }

    public AudioMixerGroup sfxMixerGroup;
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";

    private bool allowFootsteps = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (audioSource != null && sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals("Level1"))
        {
            surface = SurfaceType.Grass;
            allowFootsteps = false; // Disable footsteps in Level1
        }
        else if (sceneName.Equals("Level2"))
        {
            surface = SurfaceType.Stone;
        }
    }

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    void Update()
    {
        CheckGround();

        if (allowFootsteps && IsMoving())
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

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            audioSource.PlayOneShot(jumpSound);
        }

        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("falling");
            PlayLandingSound();
        }

        wasGrounded = isGrounded;
    }

    private void PlayLandingSound()
    {
        AudioClip landClip = surface == SurfaceType.Stone ? landStoneSound : landGrassSound;
        if (landClip != null)
        {
            audioSource.PlayOneShot(landClip);
        }
    }

    private bool IsMoving()
    {
        float moveInput = Input.GetAxisRaw(horizontalAxis);
        return Mathf.Abs(moveInput) > 0.1f;
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    private void PlayFootstep()
    {
        AudioClip[] clips = surface == SurfaceType.Stone ? stoneFootsteps : grassFootsteps;
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
