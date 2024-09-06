using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PreviewPicture))]
public class PreviewPictureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PreviewPicture script = target as PreviewPicture;

        GUILayout.Space(4);
        if (GUILayout.Button("Take Picture"))
        {
            script.TakePicture();
        }
    }
}
