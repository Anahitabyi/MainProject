using UnityEngine;
using Unity.Netcode;

public class KeyTracker : NetworkBehaviour
{
    public static KeyTracker Instance { get; private set; }
    
    public int totalKey = 4;
    
    private NetworkVariable<int> keyBitmask = new NetworkVariable<int>(0);

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
        if (!IsServer)
        {
            Debug.LogWarning("GotKey should only be called on the server!");
            return;
        }

        if (keyIndex < 0 || keyIndex >= totalKey)
        {
            Debug.LogError($"Key index {keyIndex} is out of bounds!");
            return;
        }

        int mask = 1 << keyIndex;
        if ((keyBitmask.Value & mask) == 0)
        {
            keyBitmask.Value |= mask;
            Debug.Log($"Key collected: {keyIndex}. Total keys collected: {GetCurrentKeyCount()}");
        }
    }

    public bool HasKey(int keyIndex)
    {
        int mask = 1 << keyIndex;
        return (keyBitmask.Value & mask) != 0;
    }

    public int GetCurrentKeyCount()
    {
        int count = 0;
        int bits = keyBitmask.Value;
        for (int i = 0; i < totalKey; i++)
        {
            if ((bits & (1 << i)) != 0)
                count++;
        }
        return count;
    }

    public void ResetKeys()
    {
        if (!IsServer)
        {
            Debug.LogWarning("ResetKeys should only be called on the server!");
            return;
        }
        keyBitmask.Value = 0;
    }
}