using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; } = 0;
    public static ScoreManager Instance;
    //public ScoreUI ScoreUI;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //keeps ScoreManager across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one ScoreManager exists
        }
    }

    public void AddScore(int value)
    {
        CurrentScore += value;
        Debug.Log("Score is now: " + CurrentScore);
        //ScoreUI.UpdateScoreText(CurrentScore);
    }
}
