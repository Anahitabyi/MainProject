using UnityEngine;
using Unity.Netcode;
public class newBullet : NetworkBehaviour
{
    private AnimationCurve trajectoryAnimationCurve;
    private Vector3 targetPos;
    private float trajectoryMaxHeight;
    private float bulletSpeed;

    private Vector3 trajectoryStartPoint;

    private float elapsedTime = 0f;
    private float totalTravelTime;
    public GameObject explosionPrefab;


    public void InitializeProjectile(Vector3 targetPos, float trajectoryMaxHeight, float bulletSpeed)
    {
        this.targetPos = targetPos;
        this.trajectoryMaxHeight = trajectoryMaxHeight;
        this.bulletSpeed = bulletSpeed;

        trajectoryStartPoint = transform.position;

        float distance = Vector3.Distance(trajectoryStartPoint, targetPos);
        totalTravelTime = distance / bulletSpeed;
        elapsedTime = 0f;
    }

    public void InitializeAnimationCurve(AnimationCurve animationCurve)
    {
        this.trajectoryAnimationCurve = animationCurve;
    }

    void Update()
    {
        if (!IsServer) return;
        if (trajectoryAnimationCurve == null)
            return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / totalTravelTime);

        // Base position interpolated linearly from start to target
        Vector3 basePosition = Vector3.Lerp(trajectoryStartPoint, targetPos, t);

        // Evaluate curve height at t (normalized 0-1)
        float heightOffset = trajectoryAnimationCurve.Evaluate(t) * trajectoryMaxHeight;

        // Add height offset on the Y axis (or adjust for 3D if needed)
        Vector3 curvedPosition = new Vector3(basePosition.x, basePosition.y + heightOffset, basePosition.z);

        transform.position = curvedPosition;

        if (t >= 1f)
        {
            Destroy(gameObject);  // Destroy bullet when it reaches the target
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return; // ✅ server handles all collisions

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            SpawnExplosion();
            NetworkObject.Despawn();
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SpawnExplosion();
            NetworkObject.Despawn();
            return;
        }

        if (collision.TryGetComponent<BossShooterDevice>(out var shooter))
        {
            shooter.TakeDamage(1);
            SpawnExplosion();
            NetworkObject.Despawn();
            return;
        }

        if (collision.TryGetComponent<EnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(1);
            SpawnExplosion();
            NetworkObject.Despawn();
            return;
        }

        if (collision.TryGetComponent<BossEnemy>(out var boss))
        {
            boss.TakeDamage(1);
            SpawnExplosion();
            NetworkObject.Despawn();
        }
    }

    private void SpawnExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var netObj = effect.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn(true);
            else
                Destroy(effect, 1f); // fallback if non-networked prefab
        }
    }


}
