using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RoundTransformValues
{
    [MenuItem("Tools/Graphs Utilities/Round transform")]
    public static void RoundTransform()
    {
        Transform[] transforms = Selection.gameObjects.Select(x => x.transform).ToArray();
        foreach (var t in transforms)
        {
            t.position = t.position.ToRound(2);
            t.eulerAngles = t.eulerAngles.ToRound(2);
            t.localScale = t.localScale.ToRound(2);
        }
    }
}