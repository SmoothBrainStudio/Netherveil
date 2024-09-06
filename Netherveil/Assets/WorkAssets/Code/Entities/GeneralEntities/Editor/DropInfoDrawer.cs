using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DropInfo))]
public class DropInfoDrawer : PropertyDrawer
{
    enum SerializeType
    {
        NONE,
        INT,
        RANGE,
        FLOAT
    }
    SerializedProperty lootProperty;
    SerializedProperty chanceProperty;
    SerializedProperty maxQuantityProperty;
    SerializedProperty minQuantityProperty;

    SerializedProperty isChanceSharedProperty;
    SerializedProperty decreasingValueProperty;

    int nbMember = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        nbMember = 0;
        EditorGUI.BeginProperty(position, label, property);
        lootProperty = property.FindPropertyRelative("loot");
        chanceProperty = property.FindPropertyRelative("chance");
        maxQuantityProperty = property.FindPropertyRelative("maxQuantity");
        minQuantityProperty = property.FindPropertyRelative("minQuantity");
        if(maxQuantityProperty.intValue < minQuantityProperty.intValue) maxQuantityProperty.intValue = minQuantityProperty.intValue;
        isChanceSharedProperty = property.FindPropertyRelative("isChanceShared");
        decreasingValueProperty = property.FindPropertyRelative("decreasingValuePerDrop");
        if(lootProperty.objectReferenceValue != null)
        {
            label.text = lootProperty.objectReferenceValue.name;
        }
        else
        {
            label.text = "None";
        }
        
        Rect foldoutBox = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutBox, property.isExpanded, label);
        bool shared = isChanceSharedProperty.boolValue;
        if (property.isExpanded)
        {
            DrawMember(position, lootProperty);
            DrawMember(position, chanceProperty, SerializeType.RANGE);
            if(!shared) DrawMember(position, minQuantityProperty);
            DrawMember(position, maxQuantityProperty);
            DrawMember(position, isChanceSharedProperty);
            if(!shared) DrawMember(position, decreasingValueProperty, SerializeType.RANGE);
        }
        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
        if(GUI.changed) EditorUtility.SetDirty(property.serializedObject.targetObject);

    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLine = 1;
        if (property.isExpanded)
        {
            totalLine += 4;
            if (!property.FindPropertyRelative("isChanceShared").boolValue) totalLine += 2;
        }
        return (EditorGUIUtility.singleLineHeight + 2) * totalLine;
    }

    private void DrawMember(Rect position, SerializedProperty propertyToDraw, SerializeType type = SerializeType.NONE)
    {
        nbMember++;
        EditorGUI.indentLevel++;
        float posX = position.min.x;
        float posY = position.min.y + (2 + EditorGUIUtility.singleLineHeight) * nbMember;
        float width = position.size.x;
        float height = EditorGUIUtility.singleLineHeight;

        Rect drawArea = new Rect(posX, posY, width, height);

        switch(type)
        {
            case SerializeType.INT:
                propertyToDraw.intValue = EditorGUI.IntField(drawArea, new GUIContent(propertyToDraw.name.SeparateAllCase()), propertyToDraw.intValue);
                break;
            case SerializeType.RANGE:
                propertyToDraw.floatValue = EditorGUI.Slider(drawArea, new GUIContent(propertyToDraw.name.SeparateAllCase()), propertyToDraw.floatValue, 0, 1);
                break;
            default:
                EditorGUI.PropertyField(drawArea, propertyToDraw);
                break;
        }
        
        EditorGUI.indentLevel--;
    }
}
