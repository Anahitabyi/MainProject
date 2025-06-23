using System.Collections.Generic;
using UnityEngine;

public class SaveTracker : MonoBehaviour
{
    public static SaveTracker Instance { get; private set; }

    public HashSet<string> collectedIDs = new();
    public HashSet<string> defeatedEnemyIDs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarkCollected(string id)
    {
        collectedIDs.Add(id);
    }

    public bool IsCollected(string id) => collectedIDs.Contains(id);

    public void MarkEnemyDefeated(string id)
    {
        defeatedEnemyIDs.Add(id);
    }

    public bool IsEnemyDefeated(string id) => defeatedEnemyIDs.Contains(id);
}
