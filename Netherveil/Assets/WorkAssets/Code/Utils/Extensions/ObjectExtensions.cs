using UnityEngine;

static public class ObjectExtensions
{
#if UNITY_EDITOR
    /// <summary>
    /// Used to destroy Object in OnValidate functions (can undo destroy in editor)
    /// </summary>
    public static void DestroyOnValidate(this Object component)
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            //Object.DestroyImmediate(component);
            UnityEditor.Undo.DestroyObjectImmediate(component);
        };
    }
#endif

}