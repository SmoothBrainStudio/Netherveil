using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor;
#endif

//used to serialize list of lists, so that you can have List<NestedList<T>> and be serialized in inspector.
[System.Serializable]
public class NestedList<T>
{
    public List<T> data;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NestedList<>))]
public class NestedListDrawer : PropertyDrawer
{

    SerializedProperty dataProperty;
    int nbMember = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        nbMember = 0;
        EditorGUI.BeginProperty(position, label, property);
        dataProperty = property.FindPropertyRelative("data");

        Rect foldoutBox = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutBox, property.isExpanded, label);
        if (property.isExpanded)
        {
            DrawMember(position, dataProperty);
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLine = 1;
        dataProperty = property.FindPropertyRelative("data");
        if (property.isExpanded)
        {
            totalLine += 1;
            if(dataProperty.isExpanded)
            {
                totalLine += 2;
                totalLine += dataProperty.arraySize;
            }
        }
        return EditorGUIUtility.singleLineHeight * totalLine;
    }

    private void DrawMember(Rect position, SerializedProperty propertyToDraw)
    {
        nbMember++;
        EditorGUI.indentLevel++;
        float posX = position.min.x;
        float posY = position.min.y + EditorGUIUtility.singleLineHeight * nbMember;
        float width = position.size.x;
        float height = EditorGUIUtility.singleLineHeight;

        Rect drawArea = new Rect(posX, posY, width, height);
        EditorGUI.PropertyField(drawArea, propertyToDraw);
        EditorGUI.indentLevel--;
    }
}
#endif
