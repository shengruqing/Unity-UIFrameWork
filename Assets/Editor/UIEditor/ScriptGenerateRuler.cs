using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ScriptGenerateRuler
{
    [SerializeField] public string uiElementRegex;
    [SerializeField] public UIComponentName componentName;
    [SerializeField] public string componentType;
    [SerializeField] public bool isUIGroup = false;

    public ScriptGenerateRuler(string uiElementRegex, UIComponentName componentName,string componentType, bool isUIGroup = false)
    {
        this.uiElementRegex = uiElementRegex;
        this.componentName = componentName;
        this.isUIGroup = isUIGroup;
        this.componentType = componentType;
    }
}

[Serializable]
public class UIGenType
{
    public UIGenType(string uiTypeName, bool isGeneric)
    {
        this.uiTypeName = uiTypeName;
        this.isGeneric = isGeneric;
    }

    public string uiTypeName;
    public bool isGeneric;
}

[CustomPropertyDrawer(typeof(ScriptGenerateRuler))]
public class ScriptGenerateRulerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        var uiElementRegexRect = new Rect(position.x, position.y, 120, position.height);
        var componentNameRect = new Rect(position.x + 125, position.y, 150, position.height);
        var isUIGroupRect = new Rect(position.x + 325, position.y, 150, position.height);
        EditorGUI.PropertyField(uiElementRegexRect, property.FindPropertyRelative("uiElementRegex"), GUIContent.none);
        EditorGUI.PropertyField(componentNameRect, property.FindPropertyRelative("componentName"), GUIContent.none);
        EditorGUI.PropertyField(isUIGroupRect, property.FindPropertyRelative("isUIGroup"), GUIContent.none);
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

public enum UIComponentName
{
    GameObject,
    Transform,
    RectTransform,
    Text,
    RichTextItem,
    Button,
    Image,
    RawImage,
    ScrollRect,
    Scrollbar,
    InputField,
    GridLayoutGroup,
    HorizontalLayoutGroup,
    VerticalLayoutGroup,
    Slider,
    Toggle,
    ToggleGroup,
    AnimationCurve,
    CanvasGroup,
    TextMeshProUGUI,
    Canvas,
    Dropdown,
}