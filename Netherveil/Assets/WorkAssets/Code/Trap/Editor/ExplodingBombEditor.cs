using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExplodingBomb))]
public class ExplodingBombEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExplodingBomb bomb = target as ExplodingBomb;

        GUILayout.Space(10);
        if (GUILayout.Button("Activate"))
        {
            bomb.Activate();
        }
        if (GUILayout.Button("Explode"))
        {
            bomb.Explode();
        }
        //if (GUILayout.Button("Throw"))
        //{
        //    bomb.ThrowToPos(FindObjectOfType<Hero>().transform.position, 5f);
        //}
    }
}
