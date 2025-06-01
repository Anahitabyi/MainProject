using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    public PlayerHealth playerHealth;     // Assign in Inspector
    public Image[] heartImages;           // Should be size 3
    public Sprite fullHeart;              // Assign in Inspector
    public Sprite emptyHeart;             // Assign in Inspector

    void Start()
{
    Debug.Log("PlayerLivesUI Start() called.");

    if (playerHealth != null)
    {
        playerHealth.OnLivesChanged += UpdateHearts;
        Debug.Log($"Initial Lives: {playerHealth.currentLives}/{playerHealth.maxLives}");

        UpdateHearts(playerHealth.currentLives, playerHealth.maxLives);
    }
    else
    {
        Debug.LogWarning("PlayerHealth reference is missing!");
    }
}


    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateHearts;
        }
    }

    void UpdateHearts(int lives, int maxLives)
{
    Debug.Log($"UpdateHearts() called with lives = {lives}, maxLives = {maxLives}");

    for (int i = 0; i < heartImages.Length; i++)
    {
        if (i < lives)
        {
            Debug.Log($"Heart {i}: FULL");
            heartImages[i].sprite = fullHeart;
        }
        else
        {
            Debug.Log($"Heart {i}: EMPTY");
            heartImages[i].sprite = emptyHeart;
        }

        heartImages[i].enabled = i < maxLives;
    }
}

}
