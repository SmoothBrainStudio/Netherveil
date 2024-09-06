using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class StringChoice
{
    public List<string> choice = new();
    public List<string> choices = new();

    public StringChoice(List<string> choices)
    {
        this.choices = choices;
    }
 }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StringChoice))]
public class StringChoiceUIE : PropertyDrawer
{
    int nbMember = 0;
    //int selectedIndex = 0; // Commenter par Dorian -> WARNING
    SerializedProperty choiceProperty;
    SerializedProperty choicesProperty;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        nbMember = 0;
        choiceProperty = property.FindPropertyRelative("choice");
        choicesProperty = property.FindPropertyRelative("choices");
        EditorGUI.BeginProperty(position, label, property);
        Debug.Log(choicesProperty.arraySize);
        for(int i = 0; i < choicesProperty.arraySize; ++i)
        {
            Debug.Log(choicesProperty.GetArrayElementAtIndex(i).stringValue);
        }
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLine = 1;
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
