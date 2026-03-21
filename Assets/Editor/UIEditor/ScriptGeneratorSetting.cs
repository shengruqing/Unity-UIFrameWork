using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "GameFrame/ScriptGeneratorSetting", fileName = "ScriptGeneratorSetting")]
public class ScriptGeneratorSetting : ScriptableObject
{
    private static ScriptGeneratorSetting _instance;

    public static ScriptGeneratorSetting Instance
    {
        get
        {
            if (_instance == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ScriptGeneratorSetting");
                if (guids.Length >= 1)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<ScriptGeneratorSetting>(path);
                }
            }

            return _instance;
        }
    }

    // [FolderPath]
    // [LabelText("默认组件代码保存路径")]
    [SerializeField] private string genCodePath = "/Scripts/GameLogic/UI/UIView/Logic";
    [SerializeField] private string impCodePath = "/Scripts/GameLogic/UI/UIView/View";
    [SerializeField] private string autoGenerateCodePath = "/Scripts/GameLogic/UI/UIView/Gen";
    [SerializeField] private string groupCodePath = "/Scripts/GameLogic/UI/UIView/UIGroup";

    // [LabelText("绑定代码命名空间")]
    [SerializeField] private string _namespace = "GameLogic";

    // [LabelText("子组件名称(不会往下继续遍历)")]
    [SerializeField] private string _groupName = "UIGroup";


    public string GenCodePath => genCodePath;
    public string ImpCodePath => impCodePath;

    public string AutoGenerateCodePath => autoGenerateCodePath;
    
    public string GroupCodePath => groupCodePath;

    public string Namespace => _namespace;

    public string GroupName => _groupName;


    [SerializeField] private List<UIGenType> uiGenTypes = new List<UIGenType>()
    {
        new UIGenType("UIView", false),
        new UIGenType("UIGroup", false),
    };

    public List<UIGenType> UIGenTypes => uiGenTypes;


    [SerializeField] private List<ScriptGenerateRuler> scriptGenerateRule = new List<ScriptGenerateRuler>()
    {
        new ScriptGenerateRuler("go_", UIComponentName.GameObject, "GameObject"),
        new ScriptGenerateRuler("tf_", UIComponentName.Transform, "Transform"),
        new ScriptGenerateRuler("rect_", UIComponentName.RectTransform, "RectTransform"),
        new ScriptGenerateRuler("text_", UIComponentName.Text, "Text"),
        new ScriptGenerateRuler("richText_", UIComponentName.RichTextItem, "RichTextItem"),
        new ScriptGenerateRuler("btn_", UIComponentName.Button, "Button"),
        new ScriptGenerateRuler("img_", UIComponentName.Image, "Image"),
        new ScriptGenerateRuler("rimg_", UIComponentName.RawImage, "RawImage"),
        new ScriptGenerateRuler("scrollBar_", UIComponentName.Scrollbar, "Scrollbar"),
        new ScriptGenerateRuler("scroll_", UIComponentName.ScrollRect, "ScrollRect"),
        new ScriptGenerateRuler("input_", UIComponentName.InputField, "InputField"),
        new ScriptGenerateRuler("grid_", UIComponentName.GridLayoutGroup, "GridLayoutGroup"),
        new ScriptGenerateRuler("hlay_", UIComponentName.HorizontalLayoutGroup, "HorizontalLayoutGroup"),
        new ScriptGenerateRuler("vlay_", UIComponentName.VerticalLayoutGroup, "VerticalLayoutGroup"),
        new ScriptGenerateRuler("slider_", UIComponentName.Slider, "Slider"),
        new ScriptGenerateRuler("group_", UIComponentName.ToggleGroup, "ToggleGroup"),
        new ScriptGenerateRuler("curve_", UIComponentName.AnimationCurve, "AnimationCurve"),
        new ScriptGenerateRuler("canvasGroup_", UIComponentName.CanvasGroup, "CanvasGroup"),
        new ScriptGenerateRuler("lab_", UIComponentName.TextMeshProUGUI, "TextMeshProUGUI"),
        new ScriptGenerateRuler("toggle_", UIComponentName.Toggle, "Toggle"),
    };

    public List<ScriptGenerateRuler> ScriptGenerateRule => scriptGenerateRule;


    [MenuItem("GameFramework/Create ScriptGeneratorSetting")]
    private static void CreateAutoBindGlobalSetting()
    {
        string[] paths = AssetDatabase.FindAssets("t:ScriptGeneratorSetting");
        if (paths.Length >= 1)
        {
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            EditorUtility.DisplayDialog("警告", $"已存在ScriptGeneratorSetting，路径:{path}", "确认");
            return;
        }

        ScriptGeneratorSetting setting = CreateInstance<ScriptGeneratorSetting>();
        AssetDatabase.CreateAsset(setting, "Assets/Editor/ScriptGeneratorSetting.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static List<ScriptGenerateRuler> GetScriptGenerateRule()
    {
        if (Instance == null)
        {
            return null;
        }

        return Instance.ScriptGenerateRule;
    }

    public static string GetUINameSpace()
    {
        if (Instance == null)
        {
            return string.Empty;
        }

        return Instance.Namespace;
    }


    public static string GetGenCodePath() => Instance?.GenCodePath;
    public static string GetImpCodePath() => Instance?.ImpCodePath;
    public static string GetAutoGenerateCodePath() => Instance?.AutoGenerateCodePath;
    public static string GetGroupCodePath() => Instance?.GroupCodePath;

    public static string GetGroupName()
    {
        if (Instance == null)
        {
            return string.Empty;
        }

        return Instance.GroupName;
    }


    public static string GetUIComponentWithoutPrefixName(UIComponentName uiComponentName)
    {
        if (Instance.ScriptGenerateRule == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < Instance.ScriptGenerateRule.Count; i++)
        {
            var rule = Instance.ScriptGenerateRule[i];

            if (rule.componentName == uiComponentName)
            {
                return rule.uiElementRegex.Substring(rule.uiElementRegex.IndexOf("_", StringComparison.Ordinal) + 1);
            }
        }

        return string.Empty;
    }

    public static UIGenType GetUIGenType(string uiGenTypeName)
    {
        if (string.IsNullOrEmpty(uiGenTypeName))
        {
            return null;
        }

        var tempList = Instance.UIGenTypes;
        for (int i = 0; i < tempList.Count; i++)
        {
            var uiGenType = tempList[i];

            if (string.Equals(uiGenTypeName, uiGenType.uiTypeName, StringComparison.Ordinal))
            {
                return uiGenType;
            }
        }

        return null;
    }
}