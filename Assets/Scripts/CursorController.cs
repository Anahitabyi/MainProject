using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Texture2D cursorIdle;
    public Texture2D cursorClick;
    public Vector2 cursorHotspot = Vector2.zero;

    private bool isClicking = false;

    void Start()
    {
        Cursor.SetCursor(cursorIdle, cursorHotspot, CursorMode.Auto);
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(cursorClick, cursorHotspot, CursorMode.Auto);
            isClicking = true;
        }
        else if (Input.GetMouseButtonUp(0) && isClicking)
        {
            Cursor.SetCursor(cursorIdle, cursorHotspot, CursorMode.Auto);
            isClicking = false;
        }
    }
}