using UnityEngine;
using Unity.Cinemachine;

public class CameraAssignNetwork : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private PlayerIdentifier.PlayerType targetType; 
    // Set this in Inspector: Hooded → Camera1, Hobbit → Camera2

    private void Awake()
    {
        if (vcam == null)
            vcam = GetComponent<CinemachineCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (vcam == null) return;

        vcam.Follow = target;
        vcam.LookAt = target;
        vcam.gameObject.SetActive(true);
    }

    public PlayerIdentifier.PlayerType TargetType => targetType;
}
