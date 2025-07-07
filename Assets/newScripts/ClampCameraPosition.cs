using System;
using UnityEngine;

public class ClampCameraPosition : MonoBehaviour
{
    public Transform cameraObject;
    public float upperX;
    public float downerX;
    public float upperY;
    public float downerY;


    // Update is called once per frame
    void Update()
    {
        float posX = Math.Clamp(cameraObject.position.x, downerX, upperX);
        float posY = Math.Clamp(cameraObject.position.y, downerY, upperY);
        cameraObject.position = new Vector3 (posX, posY, transform.position.x);
    }
}
