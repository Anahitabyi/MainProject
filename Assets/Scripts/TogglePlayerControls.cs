using UnityEngine;

public class TogglePlayerControls : MonoBehaviour
{
    [SerializeField] private MonoBehaviour movementScript; // Reference to movement script

    public void EnableControls()
    {
        if (movementScript != null)
        {
            movementScript.enabled = true;
            Debug.Log("Movement script enabled");
        }
    }

    public void DisableControls()
    {
        if (movementScript != null)
        {
            movementScript.enabled = false;
            Debug.Log("Movement script disabled");
        }
    }
}
