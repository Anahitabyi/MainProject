using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class GenerateID : MonoBehaviour
{
    [SerializeField] private string uniqueID;

    public string Id => _id;

    private string _id => string.IsNullOrEmpty(uniqueID) ? GenerateAndAssignID() : uniqueID;

    private string GenerateAndAssignID()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
        return uniqueID;
    }

#if UNITY_EDITOR
    [ContextMenu("Generate New ID")]
    private void GenerateNewID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
        EditorUtility.SetDirty(this);
    }
#endif
}
