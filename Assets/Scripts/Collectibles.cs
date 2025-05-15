using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int scoreValue = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name);

        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(scoreValue);
            Debug.Log("Score added: " + scoreValue);
        }

        Destroy(gameObject);
    }

    void Update()
    {
        Debug.Log("Collectible alive: " + gameObject.name);
    }
}
