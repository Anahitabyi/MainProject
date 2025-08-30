using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;

public class newShooterPlayerMovement : NetworkBehaviour
{
    public bool isInputBlocked { get; set; } = false;
    public NetworkVariable<bool> isInputBlockedNet = new NetworkVariable<bool>(false);

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
    [SerializeField] private AnimationCurve trajectoryAnimationCurve;
    [SerializeField] private float trajectoryMaxHeight;
    public float fireRate = 0.1f;
    private CinemachineImpulseSource impulseSource;
    public Camera hobbitCamera;


    [Header("Attack")]
    public int attackDamage = 1;
    

    public WeaponUIIndicator weaponUIIndicator;

    Rigidbody2D rb;
    public NetworkVariable<float> magnitude = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


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
        isInputBlocked = isInputBlockedNet.Value;
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
        //animator.SetFloat("magnitude", moveInput.magnitude);
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            animator.SetFloat("magnitude", moveInput.magnitude);
            //Debug.Log("offline!");
        }
        else
        {
            if (IsOwner)
            {
                    magnitude.Value = moveInput.magnitude;
                    //Debug.Log("magnitude: " + magnitude1.Value);
                    animator.SetFloat("magnitude", magnitude.Value);
                
                // else if (playerIdentifier.playerType == PlayerIdentifier.PlayerType.Hooded)
                // {
                //     magnitude2.Value = localMag;
                //     Debug.Log("magnitude: " + magnitude2.Value);
                //     animator.SetFloat("magnitude", magnitude2.Value);
                // }

            }

        }
        // ------------------------------------------------------------
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
        if (isInputBlocked) return;
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
    public void OnNewShoot(InputAction.CallbackContext context)
    {
        if (!context.performed || isInputBlocked) return;

        // Get mouse position in world space (owner only)
        Vector3 mousePosition = hobbitCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Call server to spawn bullet
        NewShootServerRpc(firePoint.position, mousePosition);
    }

    [ServerRpc(RequireOwnership = true)]
    private void NewShootServerRpc(Vector3 spawnPosition, Vector3 targetPosition)
    {
        // Spawn bullet on server
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        newBullet bullet = bulletObject.GetComponent<newBullet>();
        if (bullet != null)
        {
            bullet.InitializeProjectile(targetPosition, trajectoryMaxHeight, bulletSpeed);
            bullet.InitializeAnimationCurve(trajectoryAnimationCurve);
        }

        // Spawn bullet as NetworkObject for all clients
        NetworkObject netObj = bulletObject.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn(true);

        // Optional: impact effect (local only on owner)
        if (impactEffect != null)
        {
            GameObject effectObj = Instantiate(impactEffect, spawnPosition, Quaternion.identity);
            NetworkObject netEffect = effectObj.GetComponent<NetworkObject>();
            if (netEffect != null)
                netEffect.Spawn(); // now all clients see it
        }
    }

    // private void newShoot(){

    //     Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); //this is the target
    //     Vector3 direction = (mousePosition - (Vector3)firePoint.position).normalized;

    //     GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    //     newBullet bullet = bulletObject.GetComponent<newBullet>();
    //     bullet.InitializeProjectile(mousePosition, trajectoryMaxHeight, bulletSpeed);
    //     bullet.InitializeAnimationCurve(trajectoryAnimationCurve);


    //     if (impactEffect != null)
    //     {
    //         GameObject flash = Instantiate(impactEffect, firePoint.position, firePoint.rotation, firePoint);
    //         //Destroy(flash, 0.5f);
    //     }
    //     if (impulseSource != null)
    //     {
    //         impulseSource.GenerateImpulse(-direction * 0.2f);
    //     }
    //     sfx.PlaySound(sfx.attackSound);
    // }

}
