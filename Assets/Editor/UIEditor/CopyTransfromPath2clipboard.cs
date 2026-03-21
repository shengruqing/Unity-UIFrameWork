using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#region 模块信息

// **********************************************************************
// 项目名(ProductName):           
// 文件名(File Name):             AutoCreatUIScript.cs
// 作者(Author):                  ShengRuQing
// 创建时间(CreateTime):           2026-2-2 10:10
// 修改者列表(modifier):
// 模块描述(Module description):
// **********************************************************************

#endregion

public class CopyTransfromPath2clipboard
{
    //复制资源路径到剪贴板
    [MenuItem("GameObject/Copy Transfrom Path to clipBoard &C", false, 0)]
    static void Copy()
    {
        var transform = Selection.activeTransform;
        if (!transform)
        {
            return;
        }

        string selectName = transform.name;
        string transF = GetTransfrom(transform);
        string path = transform.name;
        while (transform.parent)
        {
            transform = transform.parent;
            if (!transform.name.Contains("View") && !transform.name.Contains("UIRoot"))
            {
                path = transform.name + "/" + path;
            }
            else
            {
                break;
            }
        }

        //去掉名称空格
        selectName = Regex.Replace(selectName, @"\s", "");
        //检测选中的GameObject上面是否挂有UI组件或者名称中包含obj_
        if (transF != "" && !transF.Contains("GameObject") && !path.Contains("ComCurrency") && !path.Contains("ItemComponent"))
        {
            //获取UGUI组件
            path = transF + " " + selectName + " = this.transform.Find(\"" + path + "\")" + ".GetComponent<" + transF + ">();";
            if (transF == "Button")
            {
                string converted = ConvertToEventName(selectName).Replace("_", "");
                path = path + "\r\n" + selectName + ".onClick.AddListener(" + converted + ");";
            }
            else if (transF == "Toggle")
            {
                string converted = ConvertToEventName(selectName);
                path = path + "\r\n" + selectName + ".onValueChanged.AddListener(" + converted + ");";
            }
        }
        else
        {
            //获取GameObject
            path = "GameObject " + selectName + " = this.transform.Find(\"" + path + "\").gameObject;";
        }

        var text2Editor = new TextEditor
        {
            text = path
        };
        text2Editor.OnFocus();
        text2Editor.Copy();
    }

    static string ConvertToEventName(string input)
    {
        // 检查输入是否为空或不包含下划线
        if (string.IsNullOrEmpty(input) || !input.Contains("_"))
            return input;

        // 找到下划线的位置
        int underscoreIndex = input.IndexOf('_');

        // 获取下划线前面的部分
        string prefix = "On";

        // 获取下划线后面的部分
        string suffix = input.Substring(underscoreIndex + 1);

        // 将后缀的首字母大写
        if (suffix.Length > 0)
        {
            char firstChar = char.ToUpper(suffix[0]);
            string rest = suffix.Substring(1);
            string convertedSuffix = firstChar + rest;
            return prefix + convertedSuffix;
        }

        // 如果后缀为空，直接返回"On"
        return prefix;
    }

    /// <summary>
    /// 获取组件名称
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    static string GetTransfrom(Transform trans)
    {
        var rules = ScriptGeneratorSetting.Instance.ScriptGenerateRule;
        if (rules != null)
        {
            foreach (var rule in rules)
            {
                if (trans.name.Contains(rule.uiElementRegex))
                {
                    return rule.componentType;
                }
            }
        }

        return string.Empty;
    }
}