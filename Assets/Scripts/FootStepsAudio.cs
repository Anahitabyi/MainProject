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

    [Header("Landing & Jump Sounds")]
    public AudioClip jumpSound;
    public AudioClip landStoneSound;
    public AudioClip landGrassSound;

    [Header("Surface Type (Auto-set by scene)")]
    public SurfaceType surface = SurfaceType.Grass;
    public enum SurfaceType { Grass, Stone }

    public AudioMixerGroup sfxMixerGroup;
    public string horizontalAxis = "Horizontal";

    private Rigidbody2D rb;
    private bool wasGrounded;
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals("Level1"))
            surface = SurfaceType.Grass;
        else if (sceneName.Equals("Level2"))
            surface = SurfaceType.Stone;
    }

    void Update()
    {
        // Ground check
        isGrounded = CheckGrounded();

        if (IsMoving() && isGrounded)
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
            PlayLandingSound();
        }

        wasGrounded = isGrounded;
    }

    private bool CheckGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private bool IsMoving()
    {
        float moveInput = Input.GetAxisRaw(horizontalAxis);
        return Mathf.Abs(moveInput) > 0.1f;
    }

    private void PlayFootstep()
    {
        AudioClip[] clips = surface == SurfaceType.Stone ? stoneFootsteps : grassFootsteps;
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void PlayLandingSound()
    {
        AudioClip landClip = surface == SurfaceType.Stone ? landStoneSound : landGrassSound;
        if (landClip != null)
        {
            audioSource.PlayOneShot(landClip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
