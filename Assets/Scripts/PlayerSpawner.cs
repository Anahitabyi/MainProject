using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] characters;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        foreach (var player in CharacterSelectManager.Instance.players)
        {
            var spawned = Instantiate(characters[player.characterId], Vector3.zero, quaternion.identity);
            spawned.GetComponent<NetworkObject>().SpawnAsPlayerObject(player.clientId);
        }
    }
}
