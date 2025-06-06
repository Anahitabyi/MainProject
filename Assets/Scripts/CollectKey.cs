using UnityEngine;

public class CollectKey : MonoBehaviour
{
    private KeyTracker keyTracker; 
    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            keyTracker.GotKey();
            Destroy(this.gameObject);
        }
    }
}
