using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.UI;
using UnityEditor;
#endif

public class MinMaxSlider : Selectable
{
    public Sprite positiveSprite;
    public Sprite negativeSprite;
    public RectTransform fillRect;
    public float minValue = -1.0f;
    public float maxValue = 1.0f;
    public bool wholeNumbers = false;
    private float mValue = 0.0f;
    public Slider.SliderEvent m_OnValueChanged = new Slider.SliderEvent();

    public float value
    {
        get
        {
            return wholeNumbers ? Mathf.Round(mValue) : mValue;
        }
        set
        {
            Set(value);
        }
    }

    private void Set(float input, bool sendCallback = true)
    {
        // Clamp the input
        float newValue = ClampValue(input);

        // If the stepped value doesn't match the last one, it's time to update
        if (mValue == newValue)
            return;

        mValue = newValue;
        UpdateVisuals();
        if (sendCallback)
        {
            UISystemProfilerApi.AddMarker("Slider.value", this);
            m_OnValueChanged.Invoke(newValue);
        }
    }

    private float ClampValue(float input)
    {
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);
        return newValue;
    }

    private void UpdateVisuals()
    {
        RectTransform curRect = GetComponent<RectTransform>();
        float x = mValue > 0 ? curRect.sizeDelta.x * (mValue / maxValue) : curRect.sizeDelta.x * (mValue / minValue);
        fillRect.sizeDelta = new Vector2(x, fillRect.sizeDelta.y);

        fillRect.GetComponent<Image>().sprite = mValue > 0 ? positiveSprite : negativeSprite;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (wholeNumbers)
        {
            minValue = Mathf.Round(minValue);
            maxValue = Mathf.Round(maxValue);
        }

        minValue = Mathf.Min(minValue, 0.0f);
        maxValue = Mathf.Max(maxValue, 0.0f);

        UpdateVisuals();
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MinMaxSlider))]
public class MinMaxSliderEditor : SelectableEditor
{
    SerializedProperty positiveSprite;
    SerializedProperty negativeSprite;
    SerializedProperty fillRect;
    SerializedProperty minValue;
    SerializedProperty maxValue;
    SerializedProperty wholeNumbers;
    SerializedProperty m_OnValueChanged;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        positiveSprite = serializedObject.FindProperty("positiveSprite");
        negativeSprite = serializedObject.FindProperty("negativeSprite");
        fillRect = serializedObject.FindProperty("fillRect");
        minValue = serializedObject.FindProperty("minValue");
        maxValue = serializedObject.FindProperty("maxValue");
        wholeNumbers = serializedObject.FindProperty("wholeNumbers");
        m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");

        MinMaxSlider slider = (MinMaxSlider)target;

        // Update the serialized object's representation
        serializedObject.Update();

        // Display a slider for the Value property in the Inspector
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(positiveSprite);
        EditorGUILayout.PropertyField(negativeSprite);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(fillRect);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(minValue);
        EditorGUILayout.PropertyField(maxValue);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(wholeNumbers);
        float newValue = EditorGUILayout.Slider("Value", slider.value, slider.minValue, slider.maxValue);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_OnValueChanged);

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            // If the value has changed, update the property
            slider.value = newValue;
        }
    }
}
#endif