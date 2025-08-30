using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int damage = 1;
    public NetworkObject keyPrefab;   // ✅ should be a NetworkObject if you want it networked
    public Vector2 keySpawnOffset = new Vector2(0, 2f);
    public NetworkObject impactEffectPrefab; // ✅ same here if networked
    public event System.Action OnDestroyed;

    private void Start()
    {
        if (IsServer)
        {
            // auto-despawn after 3s (network-wide)
            Invoke(nameof(DespawnBullet), 3f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return; // ✅ Only server handles logic
        // Ignore CameraBounds
        if (other.gameObject.layer == LayerMask.NameToLayer("CameraBounds"))
            return;
        if (other.CompareTag("Exclude"))
        return;
        Debug.Log($"[Bullet] Hit: {other.gameObject.name}");
        

        // Impact effect (optional networked)
            if (impactEffectPrefab != null)
            {
                var impact = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
                impact.Spawn();
                // Optional: auto-despawn impact after 0.5s
                Destroy(impact.gameObject, 0.5f);
            }
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Debug.Log("detected wall");
            DespawnBullet();
            return;
        }

        // Player collision
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Optional: apply damage to player if needed
            DespawnBullet();
            return;
        }

        // Boss Shooter device
        if (other.gameObject.layer == LayerMask.NameToLayer("NuclearThrone"))
        {
            if (other.TryGetComponent<BossShooterDevice>(out var shooter))
            {
                shooter.TakeDamage(damage);
            }
            DespawnBullet();
            return;
        }
        // Enemy damage
        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            DespawnBullet();
            return;
        }

        // Boss damage
        var boss = other.GetComponent<BossEnemy>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            DespawnBullet();
            return;
        }

        // GoalPoint -> spawn key
        if (other.CompareTag("GoalPoint"))
        {
            if (keyPrefab != null)
            {
                Vector3 spawnPos = other.transform.position + (Vector3)keySpawnOffset;
                var key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);
                key.Spawn();
            }

            // Despawn goal point if it’s networked
            var netObj = other.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Despawn();
            else
                Destroy(other.gameObject);

            DespawnBullet();
            return;
        }

        // Hit something else
        DespawnBullet();
    }

    private void DespawnBullet()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
