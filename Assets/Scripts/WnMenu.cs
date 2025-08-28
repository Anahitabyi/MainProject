using UnityEngine;
using Unity.Netcode;
public class WinMenu : NetworkBehaviour
{

    private void Update()
    {
        if (!IsHost) return; // only the host triggers the scene change

        // Check if any key or mouse button is pressed
        if (Input.anyKeyDown)
        {
            LoadMainMenuForAllServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadMainMenuForAllServerRpc()
    {
        if (!IsServer) return;
        
        NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}