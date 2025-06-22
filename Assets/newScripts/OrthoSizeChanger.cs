
using UnityEngine;
using Unity.Cinemachine;
//using Cinemachine;
using System.Collections;

public class OrthoSizeChanger : MonoBehaviour
{
    public CinemachineCamera framingTransposer;
    public CinemachineGroupFraming groupFramingCamera;
    public float targetSize = 10f;
    public float duration = 2f;
    public float waitTime = 1f;
    private float originalSize;

    void Start()
    {
        if (framingTransposer != null)
        {
            //originalSize = framingTransposer.Lens.OrthographicSize;
            originalSize = groupFramingCamera.OrthoSizeRange.x;
        }
    }

    public void StartChangeOrthoSize()
    {
        StartCoroutine(ChangeOrthoSize());
    }

    public IEnumerator ChangeOrthoSize()
    {
        Debug.Log("ortho size changed!");
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            groupFramingCamera.OrthoSizeRange.x = Mathf.Lerp(originalSize, targetSize, elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            groupFramingCamera.OrthoSizeRange.x = Mathf.Lerp(targetSize, originalSize, elapsedTime / duration);
            yield return null;
        }
    }
}
