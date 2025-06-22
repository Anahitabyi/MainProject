using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    public GameObject inputBlockerPanel; // assign your transparent full screen panel here

    void Start()
    {
        SetAlpha(0f);
        fadeImage.gameObject.SetActive(false);
        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(false);
    }

    public void PlayCutsceneFade()
    {
        fadeImage.gameObject.SetActive(true);
        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(true);  // block input

        StartCoroutine(FadeOutThenIn());
    }

    IEnumerator FadeOutThenIn()
    {
        Time.timeScale = 0f;  // freeze game

        yield return StartCoroutine(FadeTo(1f));

        yield return new WaitForSecondsRealtime(0.5f);  // wait in real time

        Time.timeScale = 1f;  // resume game

        yield return StartCoroutine(FadeTo(0f));

        fadeImage.gameObject.SetActive(false);

        if (inputBlockerPanel != null)
            inputBlockerPanel.SetActive(false);  // unblock input
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float time = 0f;
        float startAlpha = fadeImage.color.a;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
