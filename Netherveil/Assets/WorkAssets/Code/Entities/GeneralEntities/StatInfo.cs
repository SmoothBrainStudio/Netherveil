using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// Si vous ajoutez un truc, ajoutez le à la fin
public enum Stat
{
    HP,
    ATK,
    ATK_RANGE,
    FIRE_RATE,
    SPEED,
    CATCH_RADIUS,
    VISION_RANGE,
    CRIT_RATE,
    CRIT_DAMAGE,
    CORRUPTION,
    LIFE_STEAL,
    HEAL_COEFF,
    KNOCKBACK_COEFF,
    KNOCKBACK_DISTANCE,
    STAGGER_DURATION,
}

[Serializable]
public class StatInfo
{
    public Stat stat;

    public bool hasMaxStat;
    public bool hasMinStat;
    public bool hasCoeff;

    public float value;
    public float lastValue;

    public float coeff;
    public float maxValue;
    public float minValue;

    public float overload;
    public float underload;

    
}


#if UNITY_EDITOR
///// Affichage carré dans l'éditeur /////

[CustomPropertyDrawer(typeof(StatInfo))]
public class StatInfoDrawerUIE : PropertyDrawer
{
    public static readonly int statInfoExpendSize = 5;
    public static readonly int minMaxExpendSize = 2;
    public static readonly int coefExpendSize = 1;
    
    SerializedProperty statProperty;
    SerializedProperty valueProperty;
    SerializedProperty coeffProperty;
    SerializedProperty hasCoeffProperty;
    SerializedProperty hasMaxStatProperty;
    SerializedProperty hasMinStatProperty;
    SerializedProperty maxStatProperty;
    SerializedProperty minStatProperty;
    SerializedProperty overloadProperty;
    SerializedProperty underloadProperty;

    int nbMember = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
        nbMember = 0;
        EditorGUI.BeginProperty(position, label, property);
        statProperty = property.FindPropertyRelative("stat");
        valueProperty = property.FindPropertyRelative("value");
        coeffProperty = property.FindPropertyRelative("coeff");

        hasCoeffProperty = property.FindPropertyRelative("hasCoeff");
        hasMaxStatProperty = property.FindPropertyRelative("hasMaxStat");
        hasMinStatProperty = property.FindPropertyRelative("hasMinStat");

        maxStatProperty = property.FindPropertyRelative("maxValue");
        minStatProperty = property.FindPropertyRelative("minValue");

        overloadProperty = property.FindPropertyRelative("overload");
        underloadProperty = property.FindPropertyRelative("underload");


        label.text = statProperty.enumDisplayNames[statProperty.enumValueIndex];
        Rect foldoutBox = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutBox, property.isExpanded, label);
        if (property.isExpanded)
        {
            DrawMember(position, statProperty);
            DrawMember(position, valueProperty);

            DrawMember(position, hasCoeffProperty);
            if (hasCoeffProperty.boolValue)
            {
                if(coeffProperty.floatValue == 0)
                {
                    coeffProperty.floatValue = 1f;
                }
                EditorGUI.indentLevel++;
                DrawMember(position, coeffProperty);
                EditorGUI.indentLevel--;
            }

            DrawMember(position, hasMaxStatProperty);
            if (hasMaxStatProperty.boolValue) 
            {
                EditorGUI.indentLevel++;
                DrawMember(position, maxStatProperty);
                GUI.enabled = false;
                DrawMember(position, overloadProperty);
                GUI.enabled = true;
                EditorGUI.indentLevel--;

                if(valueProperty.floatValue > maxStatProperty.floatValue)
                {
                    valueProperty.floatValue = maxStatProperty.floatValue;
                }
            }

            DrawMember(position, hasMinStatProperty);
            if (hasMinStatProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawMember(position, minStatProperty);
                GUI.enabled = false;
                DrawMember(position, underloadProperty); 
                GUI.enabled = true;
                EditorGUI.indentLevel--;

                if (valueProperty.floatValue < minStatProperty.floatValue)
                {
                    valueProperty.floatValue = minStatProperty.floatValue;
                }
            }
        }
        
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int totalLine = 1;
        if (property.isExpanded)
        {
            totalLine += statInfoExpendSize;
            if (property.FindPropertyRelative("hasMaxStat").boolValue) totalLine += minMaxExpendSize;
            if (property.FindPropertyRelative("hasMinStat").boolValue) totalLine += minMaxExpendSize;
            if (property.FindPropertyRelative("hasCoeff").boolValue) totalLine += coefExpendSize;
            
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