using UnityEngine;
using System;

[ExecuteInEditMode] // Runs in Editor (even when not playing)
public class UniqueID : MonoBehaviour
{
    [SerializeField]
    public string id;

    private void Awake()
    {
#if UNITY_EDITOR
        // Only generate in editor, so it’s saved with the scene
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this); // Marks the object dirty so Unity saves the new ID
        }
#endif
    }
}
