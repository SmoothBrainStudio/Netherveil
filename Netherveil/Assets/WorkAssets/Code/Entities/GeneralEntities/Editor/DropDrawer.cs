using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Drop))]
public class DropDrawerUIE : PropertyDrawer
{
    SerializedProperty dropProperty;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        dropProperty = property.FindPropertyRelative("dropList");
        EditorGUILayout.PropertyField(dropProperty, label);
        if (GUI.changed) EditorUtility.SetDirty(property.serializedObject.targetObject);
    }
}