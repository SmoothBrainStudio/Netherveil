using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomPrefab))]
public class RandomPrefabEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        RandomPrefab randomPrefabScript = (RandomPrefab)target;
        if (GUILayout.Button("Create"))
        {
            randomPrefabScript.Create();
        }
    }
}
