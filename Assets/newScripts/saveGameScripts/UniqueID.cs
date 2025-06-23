using UnityEngine;

public class UniqueID : MonoBehaviour
{
    public string id;

    private void Reset()
    {
        id = System.Guid.NewGuid().ToString();
    }
}