using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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

    public CharacterSFX sfx;



    [Header("Shooting")]
    public GameObject bulletPrefab;
    //private GameObject activeBullet = null;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    //public float bulletSpawnDelay = 0.15f;
    public GameObject impactEffect;
    //public LineRenderer lineRenderer;
    private bool isShooting;
    private Coroutine shootingCoroutine;
    public float fireRate = 0.1f;
    private CinemachineImpulseSource impulseSource;
    public Camera hobbitCamera;

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
        impulseSource = GetComponent<CinemachineImpulseSource>();

        sfx = GetComponent<CharacterSFX>();
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
        //Debug.Log("Move Input: " + moveInput);
    }


    // public void OnShootStarted(InputAction.CallbackContext context)
    // {
    //     if (!isShooting)
    //     {
    //         isShooting = true;
    //         shootingCoroutine = StartCoroutine(ShootContinuously());
    //     }
    // }
    // public void OnShootCanceled(InputAction.CallbackContext context)
    // {
    //     Debug.Log("shoot canceled!");
    //     isShooting = false;
    //     if (shootingCoroutine != null)
    //         StopCoroutine(shootingCoroutine);
    // }

    public void OnShoot(InputAction.CallbackContext context){
        if(context.performed){
            Shoot();
        }

    }

    private void Shoot()
    {
        //Debug.Log("shoot started! 2");
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePosition - (Vector2)firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        if (impactEffect != null)
        {
            GameObject flash = Instantiate(impactEffect, firePoint.position, firePoint.rotation);
            //Destroy(flash, 0.5f);
        }
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(-direction * 0.2f);
        }
        sfx.PlaySound(sfx.attackSound);
    }

}
