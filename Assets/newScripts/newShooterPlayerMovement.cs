using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class newShooterPlayerMovement : MonoBehaviour
{
    public bool isInputBlocked { get; set; } = false;

    public Animator animator;

    [Header("Movement")]
    Vector2 moveInput;
    [SerializeField] private float movementSpeed = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    private GameObject activeBullet = null;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float bulletSpawnDelay = 0.2f;

    public Camera hobbitCamera;
    public float shakeDuration;
    public float shakeMagnitude;

    [Header("Attack")]
    public int attackDamage = 1;

    public WeaponUIIndicator weaponUIIndicator;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (audioSource != null && sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    void Update()
    {
        if (isInputBlocked)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("magnitude", 0);
            return;
        }

        // Update movement
        rb.linearVelocity = moveInput * movementSpeed;
        // Flip the sprite based on horizontal movement
        // Flip sprite without overriding original scale
        Vector3 localScale = transform.localScale;

        if (moveInput.x > 0.01f)
        {
            localScale.x = Mathf.Abs(localScale.x); // Ensure it's positive
            transform.localScale = localScale;
        }
        else if (moveInput.x < -0.01f)
        {
            localScale.x = -Mathf.Abs(localScale.x); // Flip X
            transform.localScale = localScale;
        }


        // Animator control for movement magnitude
        animator.SetFloat("magnitude", moveInput.magnitude);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isInputBlocked) return;
        moveInput = context.ReadValue<Vector2>();  // Get movement input
        Debug.Log("Move Input: " + moveInput);
    }


    public void Shoot(InputAction.CallbackContext context)
    {
        if (isInputBlocked || !context.performed) return;

        animator.SetTrigger("shoot");
        StartCoroutine(DelayedBulletSpawn());
    }

    private IEnumerator DelayedBulletSpawn()
    {
        yield return new WaitForSeconds(bulletSpawnDelay);

        if (activeBullet != null) yield break;

        Vector3 mousePosition = hobbitCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

        activeBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = activeBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        Bullet bulletScript = activeBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = attackDamage;
            bulletScript.OnDestroyed += HandleBulletDestroyed;
        }

        StartCoroutine(ShakeCamera());
    }

    private void HandleBulletDestroyed()
    {
        activeBullet = null;
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = hobbitCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
            hobbitCamera.transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        hobbitCamera.transform.position = originalPos;
    }
}
