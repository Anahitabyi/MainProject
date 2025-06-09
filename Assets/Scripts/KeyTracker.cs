using UnityEngine;

public class KeyTracker : MonoBehaviour
{
    public int totalKey = 4;
    public int currentKey = 0;

    public void GotKey()
    {
        currentKey++;
        Debug.Log(currentKey);
    }

    public int getCurrentKey()
    {
        return currentKey;
    }
}
