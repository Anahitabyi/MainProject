using Unity.Netcode;
using UnityEngine;

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
            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                Destroy(gameObject); // fallback // Destroy bullet when it reaches the target
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("CameraBounds"))
            return;

//        Debug.Log($"[Bullet] Hit: {collision.gameObject.name}");
        //Debug.Log("collistion detected");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (explosionPrefab != null)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                var netObj = explosion.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn(true); // spawns across clients
            }
            NetworkObject.Despawn();
            return;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (explosionPrefab != null)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                var netObj = explosion.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn(true); // spawns across clients
            }
            NetworkObject.Despawn();
            return;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("NuclearThrone"))
        {
            if (collision.TryGetComponent<BossShooterDevice>(out var shooter))
            {
                shooter.TakeDamage(1); // This deals damage to the boss through the shooter
            }
            if (explosionPrefab != null)
            {
                var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                var netObj = explosion.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn(true); // spawns across clients
            }
            NetworkObject.Despawn();
            return;
        }
        // Check if we hit something that can take damage
        if (collision.TryGetComponent<EnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(1); // You can adjust damage value
            NetworkObject.Despawn();
        }
        else if (collision.TryGetComponent<BossEnemy>(out var boss))
        {
            boss.TakeDamage(1); // Adjust damage if needed
            NetworkObject.Despawn(); // fallback

        }
        //NetworkObject.Despawn();
    }
    //[ClientRpc]
// private void SpawnExplosionClientRpc(Vector3 position)
// {
//     if (explosionPrefab != null)
//         Instantiate(explosionPrefab, position, Quaternion.identity);
// }


}
