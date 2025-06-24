using UnityEngine;

public class UniqueID : MonoBehaviour
{
    public string id;

    private void Reset()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }
}
