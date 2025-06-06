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

    public AudioMixerGroup sfxMixerGroup;
    public string horizontalAxis = "Horizontal";

    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    public SurfaceType surface = SurfaceType.Grass;
    public enum SurfaceType { Grass, Stone }

    private bool isGrounded;
    private bool footstepsEnabled = true;

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
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Equals("Level1"))
        {
            footstepsEnabled = false; // Disable footsteps in Level1
        }
        else if (sceneName.Equals("Level2"))
        {
            surface = SurfaceType.Stone;
        }
    }

    void Update()
    {
        if (!footstepsEnabled) return;

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
