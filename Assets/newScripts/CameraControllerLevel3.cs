using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraControllerLevel3 : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    public BoxCollider2D confinerBounds;

    [System.Obsolete]
    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // Enable confiner bounds
                if (confinerBounds != null)
                    EnableConfiner(confinerBounds);
            }
        }
    }

    private void EnableConfiner(object confSinerBounds)
    {
        throw new NotImplementedException();
    }

    public void MoveAndLock(Vector3 newPosition)
    {
        targetPosition = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        isMoving = true;
    }

    [System.Obsolete]
    void EnableConfiner(BoxCollider2D bounds)
{
    var confiner = GetComponent<CinemachineConfiner2D>();
    if (confiner != null)
    {
        confiner.BoundingShape2D = bounds;
        confiner.InvalidateCache(); // ✅ correct for 3.x
    }
}
}
