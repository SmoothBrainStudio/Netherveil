using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueTreeRunner))]
public class DialogueTreeRunnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //DialogueTreeRunner runner = (DialogueTreeRunner)target;

        //GUILayout.Space(10f);
        //GUILayout.Label("Editor Debug", new GUIStyle
        //{
        //    fontStyle = FontStyle.Bold,
        //    normal = new GUIStyleState
        //    {
        //        textColor = new Color(0.75f, 0.75f, 0.75f, 1f),
        //    }
        //});
        //if (GUILayout.Button("Start dialogue"))
        //{
        //    runner.StartDialogue();
        //}
    }
}
