using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class KeyTracker : MonoBehaviour
{
    public static KeyTracker Instance { get; private set; }

    public int totalKey = 4;

    // Use a HashSet to store collected key indices
    private HashSet<int> collectedKeys = new HashSet<int>();

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

    public void GotKey(int keyIndex)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("GotKey should only be called on the server!");
            return;
        }

        if (keyIndex < 0 || keyIndex >= totalKey)
        {
            Debug.LogError($"Key index {keyIndex} is out of bounds!");
            return;
        }

        if (collectedKeys.Add(keyIndex)) // Returns true if the key was newly added
        {
            Debug.Log($"Key collected: {keyIndex}. Total keys collected: {GetCurrentKeyCount()}");
        }
        Debug.Log($"Total keys collected so far: {GetCurrentKeyCount()}");
    }

    public bool HasKey(int keyIndex)
    {
        return collectedKeys.Contains(keyIndex);
    }

    public int GetCurrentKeyCount()
    {
        return collectedKeys.Count;
    }

    public void ResetKeys()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("ResetKeys should only be called on the server!");
            return;
        }
        collectedKeys.Clear();
        Debug.Log("All keys have been reset.");
    }
}
