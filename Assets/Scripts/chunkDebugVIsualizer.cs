using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class chunkDebugVisualizer : MonoBehaviour
{
    public float width = 20f;
    public float height = 10f;

    void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.loop = false;
        lr.widthMultiplier = 0.1f;
        lr.useWorldSpace = false;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;

        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(0, 0, 0);
        corners[1] = new Vector3(width, 0, 0);
        corners[2] = new Vector3(width, height, 0);
        corners[3] = new Vector3(0, height, 0);
        corners[4] = corners[0];

        lr.SetPositions(corners);
    }
}
