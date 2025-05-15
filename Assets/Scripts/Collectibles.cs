using UnityEngine;

public class Collectibles : MonoBehaviour
{
    public int scoreValue;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered with: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched collectible");
            Destroy(gameObject);
            if (ScoreManager.Instance != null)
            { //calls add score to add the value
                ScoreManager.Instance.AddScore(scoreValue);
            }
        }
    }
}
