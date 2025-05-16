using UnityEngine;
using Unity.Cinemachine;

public class CameraFollow : MonoBehaviour
{
    [Header("Unity Cameras")]
    public Camera camera1;
    public Camera camera2;

    [Header("Cinemachine Cameras (CinemachineCamera v3)")]
    public CinemachineCamera vcam1;
    public CinemachineCamera vcam2;

    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Offsets")]
    public Vector3 camera1Offset = new Vector3(0, 5, -10);
    public Vector3 camera2Offset = new Vector3(0, 5, -10);

    void Start()
    {
        // Assign follow targets
        if (vcam1 != null && player1 != null)
        {
            vcam1.Follow = player1;
            vcam1.OutputChannel = 0; // Channel 0
            vcam1.enabled = true;
        }

        if (vcam2 != null && player2 != null)
        {
            vcam2.Follow = player2;
            vcam2.OutputChannel = (OutputChannels)1; // Channel 1
            vcam2.enabled = true;
        }

        // Setup Unity Camera 1
        if (camera1 != null)
        {
            camera1.rect = new Rect(0f, 0.5f, 1f, 0.5f); // Top half
            camera1.transform.position = player1.position + camera1Offset;

            var brain1 = camera1.GetComponent<CinemachineBrain>();
            if (brain1 == null)
                brain1 = camera1.gameObject.AddComponent<CinemachineBrain>();

            brain1.ChannelMask = (OutputChannels)(1 << 0); // Accept output from Channel 0 only
        }

        // Setup Unity Camera 2
        if (camera2 != null)
        {
            camera2.rect = new Rect(0f, 0f, 1f, 0.5f); // Bottom half
            camera2.transform.position = player2.position + camera2Offset;

            var brain2 = camera2.GetComponent<CinemachineBrain>();
            if (brain2 == null)
                brain2 = camera2.gameObject.AddComponent<CinemachineBrain>();

            brain2.ChannelMask = (OutputChannels)(1 << 1); // Accept output from Channel 1 only
        }

        Debug.Log("✅ Cameras assigned to channels successfully.");
    }
}
