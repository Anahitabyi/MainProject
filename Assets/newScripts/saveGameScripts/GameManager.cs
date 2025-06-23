using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameData currentData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // stays through scene loads
        }
        else
        {
            Destroy(gameObject); // prevent duplicates
        }
    }
}