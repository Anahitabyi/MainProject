using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public Image fadeImage; // assign a full-screen black Image with alpha=0
    public float fadeDuration = 5f;

    public void StartFadeAndLoad(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        float elapsed = 0f;

        Color startColor = fadeImage.color;
        startColor.a = 0f;

        Color targetColor = startColor;
        targetColor.a = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
