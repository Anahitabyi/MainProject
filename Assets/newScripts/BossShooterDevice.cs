using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

public class BossShooterDevice : MonoBehaviour
{
    [Header("Players")]
    public Transform player1;
    public Transform player2;
    public Transform currentTarget;

    [Header("Zone")]
    public Vector2 zoneCenter = Vector2.zero;
    public Vector2 zoneSize = new Vector2(5f, 3f);

    [Header("Animation")]
    public Animator deviceAnimator;
    public string alertTrigger = "Alert";
    public string attackTrigger = "Attack";

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform[] firePoints;
    public float spreadAngle = 15f;
    public int numberOfWaves = 3;
    public float delayBetweenWaves = 0.2f;

    [Header("Reference")]
    public BossEnemy bossRef;

    private bool hasPlayedAlert = false;


    [Header("Camera Shake")]
    // public CameraShake cameraShake;
    // public float shakeDuration = 0.3f;
    // public float shakeMagnitude = 0.1f;
    private CinemachineImpulseSource impulseSource;
    public OrthoSizeChanger orthoSizeChanger;


    //private HashSet<Transform> damagedPlayersTHisWave = new HashSet<Transform>();

    void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();

    }

    void Update()
    {
        CheckAlertZone();
    }

    private void CheckAlertZone()
    {
        if (hasPlayedAlert) return;

        bool player1In = IsPlayerInZone(player1);
        bool player2In = IsPlayerInZone(player2);

        if (player1In || player2In)
        {
            hasPlayedAlert = true;
            if (deviceAnimator != null)
                deviceAnimator.SetTrigger(alertTrigger);
                
                if(orthoSizeChanger!=null){
                   // Debug.Log("ortho not null!");
                    orthoSizeChanger.StartChangeOrthoSize();
                }
                else{
                    //Debug.Log("ortho null!");
                }


            if (bossRef != null)
                bossRef.StartAttackWithDelay();
        }
    }

    private bool IsPlayerInZone(Transform player)
    {
        Vector2 worldCenter = (Vector2)transform.position + zoneCenter;
        Vector2 halfSize = zoneSize * 0.5f;
        Vector2 playerPos = player.position;

        return (playerPos.x >= worldCenter.x - halfSize.x && playerPos.x <= worldCenter.x + halfSize.x &&
                playerPos.y >= worldCenter.y - halfSize.y && playerPos.y <= worldCenter.y + halfSize.y);
    }

    public void TriggerAttack()
    {
        if (deviceAnimator != null)
            deviceAnimator.SetTrigger(attackTrigger);
    }

    // 🔔 Call this from animation event during device attack animation
    public void FireBullets()
    {
        if (currentTarget == null || firePoints.Length == 0)
        {
            Debug.LogWarning("[ShooterDevice] Cannot fire bullets: No target or fire points.");
            return;
        }
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        foreach (Transform firePoint in firePoints)
        {
            firePoint.rotation = Quaternion.Euler(0, 0, angle + 270);
        }
        StartCoroutine(ShootBulletWaves());
        
    }

    private IEnumerator ShootBulletWaves()
    {
        Transform closest = firePoints[0];
        float closestDistance = Vector2.Distance(currentTarget.position, closest.position);

        foreach (Transform point in firePoints)
        {
            float dist = Vector2.Distance(currentTarget.position, point.position);
            if (dist < closestDistance)
            {
                closest = point;
                closestDistance = dist;
            }
        }

        for (int wave = 0; wave < numberOfWaves; wave++)
        {
            for (int i = -1; i <= 1; i++)
            {
                //damagedPlayersTHisWave.Clear();
                float angle = i * spreadAngle;
                Quaternion rotation = Quaternion.Euler(0, 0, firePoints[1].rotation.eulerAngles.z + angle);
                SpawnBullet(firePoints[1].position, rotation);
                rotation = Quaternion.Euler(0, 0, firePoints[0].rotation.eulerAngles.z + angle);
                SpawnBullet(firePoints[0].position, rotation);
            }

            if (wave < numberOfWaves - 1)
                yield return new WaitForSeconds(delayBetweenWaves);
        }
    }
    void SpawnBullet(Vector3 pos, Quaternion rot){
        GameObject bullet = Instantiate(bulletPrefab, pos, rot);
        BossBullet bossBulletScript = bullet.GetComponent<BossBullet>();
        bossBulletScript.bossEnemy = bossRef;
    }
    private void OnDrawGizmosSelected()
    {
        Vector2 worldCenter = (Vector2)transform.position + zoneCenter;
        Gizmos.color = new Color(1, 0.5f, 0f, 0.4f);
        Gizmos.DrawCube(worldCenter, zoneSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldCenter, zoneSize);
    }


}
