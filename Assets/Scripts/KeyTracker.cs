using System.Collections.Generic;
using UnityEngine;

public class KeyTracker : MonoBehaviour
{
    public static KeyTracker Instance { get; private set; }  // Singleton instance

    public int totalKey = 4;
    private HashSet<string> collectedKeys = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Enforce singleton uniqueness
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // Optional, if you want to persist between scenes
    }

    public void GotKey(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"Key collected: {keyID}. Total keys collected: {collectedKeys.Count}");
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    public int GetCurrentKeyCount()
    {
        return collectedKeys.Count;
    }

    public void ResetKeys()
    {
        collectedKeys.Clear();
    }
}
