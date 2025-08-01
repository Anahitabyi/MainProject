using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class OrthoSizeChanger : MonoBehaviour
{
    public CinemachineCamera framingTransposer;
    public CinemachineGroupFraming groupFramingCamera;
    public float targetSize = 10f;
    public float duration = 2f;
    public float waitTime = 1f;

    private float originalSize;
    private bool skipEffect = false;

    void Start()
    {
        // Skip if this effect was already played
        if (SaveTracker.Instance != null && SaveTracker.Instance.cameraSizeChanged)
        {
            skipEffect = true;
            this.enabled = false;
            return;
        }

        if (groupFramingCamera != null)
        {
            originalSize = groupFramingCamera.OrthoSizeRange.x;
        }
    }

    public void StartChangeOrthoSize()
    {
        if (!skipEffect)
        {
            StartCoroutine(ChangeOrthoSize());
        }
    }

    private IEnumerator ChangeOrthoSize()
    {
        Debug.Log("ortho size changed!");
        float elapsedTime = 0f;

        // Zoom in
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            groupFramingCamera.OrthoSizeRange.x = Mathf.Lerp(originalSize, targetSize, elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        // Zoom back out
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            groupFramingCamera.OrthoSizeRange.x = Mathf.Lerp(targetSize, originalSize, elapsedTime / duration);
            yield return null;
        }

        // ✅ Mark as completed so it won't run again
        if (SaveTracker.Instance != null)
        {
            SaveTracker.Instance.cameraSizeChanged = true;
            GameSaveController.Instance?.SaveToFile();
            Debug.Log("cameraSizeChanged flag set!");
        }
    }
}
