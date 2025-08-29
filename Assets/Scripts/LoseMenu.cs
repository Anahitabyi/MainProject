using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LoseMenu : NetworkBehaviour
{
    public TMP_Text[] options; // assign in Inspector
    private int selectedIndex = 0;

    // NetworkVariable to sync selection
    private NetworkVariable<int> netSelectedIndex = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private void Start()
    {
        netSelectedIndex.OnValueChanged += OnSelectionChanged;
        UpdateHighlight();
    }

    private void Update()
    {
        if (!IsHost) return; // only host controls

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            netSelectedIndex.Value = selectedIndex; // update networked value
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            netSelectedIndex.Value = selectedIndex;
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelectionServerRpc(selectedIndex); // confirm on server
        }
    }

    private void OnSelectionChanged(int oldValue, int newValue)
    {
        selectedIndex = newValue;
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (i == selectedIndex)
                options[i].color = Color.yellow;
            else
                options[i].color = Color.white;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ConfirmSelectionServerRpc(int index)
    {
        // handle what happens for both clients
        if (index == 1)
        {
            Debug.Log("Give Up selected - everyone goes to main menu"); 
            NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        else if (index == 0)
        {
            Debug.Log("Restart selected - reload scene for everyone");
            playerStatsManager.Instance.ResetAllStats();
            NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }
}
