using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponUIIndicator : MonoBehaviour
{
    public Image swordImage; // Drag the UI image here in inspector
    public float flashDuration = 2f;
    public float flashSpeed = 5f;

    private Coroutine flashRoutine;

    void Start()
    {
        SetVisible(false);
    }

    public void ShowForDuration(float duration)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        SetVisible(true);
        flashRoutine = StartCoroutine(HandlePowerupUI(duration));
    }

    private IEnumerator HandlePowerupUI(float duration)
    {
        float flashStartTime = Time.time + duration - flashDuration;

        while (Time.time < flashStartTime)
        {
            yield return null;
        }

        // Start flashing
        float t = 0f;
        while (Time.time < flashStartTime + flashDuration)
        {
            float alpha = Mathf.PingPong(t * flashSpeed, 1f);
            Color color = swordImage.color;
            color.a = alpha;
            swordImage.color = color;

            t += Time.deltaTime;
            yield return null;
        }

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        Color color = swordImage.color;
        color.a = visible ? 1f : 0f;
        swordImage.color = color;
    }
}
