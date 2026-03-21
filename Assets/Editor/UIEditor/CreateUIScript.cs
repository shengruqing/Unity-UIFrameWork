using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ScriptTemplates
{
    public class CreateUIScript
    {
        private static readonly Regex NamespaceStartRegex =
            new Regex(@"namespace \{ScriptNameSpace\}\s*\{\s*\n", RegexOptions.Compiled);

        private static readonly Regex NamespaceEndRegex = new Regex(@"\n\}\s*$", RegexOptions.Compiled);
        private static readonly Regex IndentRegex = new Regex(@"^    ", RegexOptions.Compiled | RegexOptions.Multiline);

        [MenuItem("GameObject/创建UI脚本", false, 0)]
        public static void CreateScript()
        {
            var transform = Selection.activeTransform;
            if (!transform)
            {
                return;
            }

            string selectName = transform.name;
            if (!selectName.Contains("View"))
            {
                if (EditorUtility.DisplayDialog("", "Prefab名称错误，请以View后缀结尾！", "修改", "取消"))
                {
                    transform.name = transform.name + "View";
                }

                return;
            }


            string logicPath = ScriptGeneratorSetting.Instance.GenCodePath;
            string viewPath = ScriptGeneratorSetting.Instance.ImpCodePath;
            string nameSpace = ScriptGeneratorSetting.Instance.Namespace;
            string autoGenerate = ScriptGeneratorSetting.Instance.AutoGenerateCodePath;
            if (FileExistsInFolder(Application.dataPath + viewPath, selectName + ".cs"))
            {
                string message = "脚本存在！创建UI脚本会覆盖旧的代码数据，导致已有的脚本数据丢失！";
                if (EditorUtility.DisplayDialog("错误", message, "确定"))
                {
                }

                return;
            }

            if (string.IsNullOrEmpty(logicPath) || string.IsNullOrEmpty(viewPath) || string.IsNullOrEmpty(autoGenerate))
            {
                if (EditorUtility.DisplayDialog("", "没有配置脚本保存路径！", "设置路径", "取消"))
                {
                    SettingsService.OpenProjectSettings("Project/GameFramework/UISettings");
                }

                return;
            }

            GenerateLogicScript(selectName, nameSpace, Application.dataPath + logicPath);
            GenerateViewScript(selectName, nameSpace, Application.dataPath + viewPath);
            GenerateAutoScript(selectName, nameSpace, Application.dataPath + autoGenerate);
        }

        [MenuItem("GameObject/创建Group脚本", false, 0)]
        public static void CreateGroupScript()
        {
            var transform = Selection.activeTransform;
            if (!transform)
            {
                return;
            }

            string selectName = transform.name;
            string nameSpace = ScriptGeneratorSetting.Instance.Namespace;
            string groupPath = ScriptGeneratorSetting.Instance.GroupCodePath;

            if (FileExistsInFolder(Application.dataPath + groupPath, selectName + ".cs"))
            {
                string message = "脚本存在！创建UI脚本会覆盖旧的代码数据，导致已有的脚本数据丢失！";
                if (EditorUtility.DisplayDialog("错误", message, "确定"))
                {
                }

                return;
            }

            if (string.IsNullOrEmpty(groupPath))
            {
                if (EditorUtility.DisplayDialog("", "没有配置脚本保存路径！", "设置路径", "取消"))
                {
                    SettingsService.OpenProjectSettings("Project/GameFramework/UISettings");
                }

                return;
            }

            GenerateGroupScript(selectName, nameSpace, Application.dataPath + groupPath);
        }

        private static void GenerateGroupScript(string scriptName, string ns, string groupPath)
        {
            GenerateScriptInternal(scriptName, ns, groupPath, "Group", "/Editor/UIEditor/Temp/GroupTemp.txt");
        }

        public static void GenerateLogicScript(string scriptName, string ns, string scriptPath)
        {
            GenerateScriptInternal(scriptName, ns, scriptPath, "Logic", "/Editor/UIEditor/Temp/ViewLogicTemp.txt");
        }

        public static void GenerateViewScript(string scriptName, string ns, string scriptPath)
        {
            GenerateScriptInternal(scriptName, ns, scriptPath, "", "/Editor/UIEditor/Temp/ViewTemp.txt");
        }

        public static void GenerateAutoScript(string scriptName, string ns, string scriptPath)
        {
            GenerateScriptInternal(scriptName, ns, scriptPath, "Wrap", "/Editor/UIEditor/Temp/GenerateTemp.txt");
        }

        private static void GenerateScriptInternal(string scriptName, string ns, string scriptPath, string suffix,
            string templateRelativePath)
        {
            try
            {
                if (!IsValidPath(scriptPath))
                {
                    Debug.LogError($"无效的脚本路径: {scriptPath}");
                    return;
                }

                string templatePath = Application.dataPath + templateRelativePath;
                if (!IsValidTemplatePath(templatePath))
                {
                    Debug.LogError($"模板文件路径不合法: {templatePath}");
                    return;
                }

                if (!File.Exists(templatePath))
                {
                    Debug.LogError($"模板文件不存在: {templatePath}");
                    return;
                }

                string templateContent;
                try
                {
                    templateContent = File.ReadAllText(templatePath, Encoding.UTF8);
                }
                catch (IOException ex)
                {
                    Debug.LogError($"读取模板文件失败: {ex.Message}");
                    return;
                }

                string scriptContent = templateContent.Replace("{scriptName}", scriptName);

                if (string.IsNullOrEmpty(ns))
                {
                    scriptContent = NamespaceStartRegex.Replace(scriptContent, "");
                    scriptContent = NamespaceEndRegex.Replace(scriptContent, "");
                    scriptContent = IndentRegex.Replace(scriptContent, "");
                }
                else
                {
                    scriptContent = scriptContent.Replace("{ScriptNameSpace}", ns);
                }

                // 确保目录存在
                try
                {
                    if (!Directory.Exists(scriptPath))
                    {
                        Directory.CreateDirectory(scriptPath);
                    }
                }
                catch (IOException ex)
                {
                    Debug.LogError($"创建目录失败: {ex.Message}");
                    return;
                }

                string fileName = suffix.Length > 0 ? scriptName + suffix + ".cs" : scriptName + ".cs";
                string filePath = Path.Combine(scriptPath, fileName);
                if (suffix == "Wrap")
                {
                    scriptContent = AddComponent(scriptContent, true);
                    scriptContent = AddComponentPath(scriptContent, scriptName, true);
                }
                else if (suffix == "Logic")
                {
                    scriptContent = AddAddListener(scriptContent, "", true);
                    scriptContent = AddAddListenerMethod(scriptContent, true);
                }
                else if (suffix == "Group")
                {
                    scriptContent = AddComponent(scriptContent, false);
                    scriptContent = AddComponentPath(scriptContent, scriptName, false);
                    scriptContent = AddAddListener(scriptContent, suffix, false);
                    scriptContent = AddAddListenerMethod(scriptContent, false);
                }

                if (!IsSafeFilePath(filePath))
                {
                    Debug.LogError($"生成的文件路径不安全: {filePath}");
                    return;
                }

                try
                {
                    File.WriteAllText(filePath, scriptContent, Encoding.UTF8);
                    Debug.Log($"脚本生成成功: {filePath}");
                }
                catch (IOException ex)
                {
                    Debug.LogError($"写入文件失败: {ex.Message}");
                    return;
                }


                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"生成脚本时发生未知错误: {ex.Message}");
            }
        }

        private static bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            string normalizedPath =
                Path.GetFullPath(new Uri(Path.Combine(Directory.GetCurrentDirectory(), path)).LocalPath);
            string dataPath = Path.GetFullPath(Application.dataPath);

            return normalizedPath.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidTemplatePath(string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
                return false;

            string normalizedPath =
                Path.GetFullPath(new Uri(Path.Combine(Directory.GetCurrentDirectory(), templatePath)).LocalPath);
            string dataPath = Path.GetFullPath(Application.dataPath);

            return normalizedPath.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSafeFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string normalizedPath =
                Path.GetFullPath(new Uri(Path.Combine(Directory.GetCurrentDirectory(), filePath)).LocalPath);
            string dataPath = Path.GetFullPath(Application.dataPath);

            return normalizedPath.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase);
        }

        public static string AddComponent(string scriptContent, bool isAll)
        {
            var selectedTransforms = GetTransform(isAll);
            StringBuilder sb = new StringBuilder();
            foreach (var trans in selectedTransforms)
            {
                if (!string.IsNullOrEmpty(trans.name))
                {
                    var componentType = GetTransfrom(trans);
                    if (!string.IsNullOrEmpty(componentType))
                    {
                        var fieldName = Regex.Replace(trans.name, @"\s", "");
                        sb.AppendLine("        public " + componentType + " " + fieldName + ";");
                    }
                }
            }

            return scriptContent.Replace("{component}", sb.ToString());
        }

        public static string AddComponentPath(string scriptContent, string scriptName, bool isAll)
        {
            var selectedTransforms = GetTransform(isAll);
            StringBuilder sb = new StringBuilder();
            foreach (var trans in selectedTransforms)
            {
                if (!string.IsNullOrEmpty(trans.name))
                {
                    string path = GetTransfromPath(trans);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (isAll)
                        {
                            sb.AppendLine("            " + path);
                        }
                        else
                        {
                            path = path.Replace(scriptName + "/", "");
                            sb.AppendLine("            " + path);
                        }
                    }
                }
            }

            return scriptContent.Replace("{componentPath}", sb.ToString());
        }

        public static string AddAddListener(string scriptContent, string suffix = "", bool isAll = true)
        {
            var selectedTransforms = GetTransform(isAll);
            StringBuilder sb = new StringBuilder();
            foreach (var trans in selectedTransforms)
            {
                if (!string.IsNullOrEmpty(trans.name))
                {
                    var line = AddAddListener(trans, suffix);
                    if (!string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine("            " + line);
                    }
                }
            }

            return scriptContent.Replace("{AddListener}", sb.ToString());
        }

        public static string AddAddListenerMethod(string scriptContent, bool isAll = true)
        {
            var selectedTransforms = GetTransform(isAll);
            StringBuilder sb = new StringBuilder();
            foreach (var trans in selectedTransforms)
            {
                if (!string.IsNullOrEmpty(trans.name))
                {
                    sb.AppendLine(AddAddListenerMethod(trans));
                }
            }

            return scriptContent.Replace("{ListenerMethod}", sb.ToString());
        }

        /// <summary>
        /// 获取组件名称
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        static string GetTransfrom(Transform trans)
        {
            var rules = ScriptGeneratorSetting.Instance.ScriptGenerateRule;
            foreach (var rule in rules)
            {
                if (trans.name.Contains(rule.uiElementRegex))
                {
                    return rule.componentType;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取组件路径
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        static string GetTransfromPath(Transform trans)
        {
            var rules = ScriptGeneratorSetting.Instance.ScriptGenerateRule;
            foreach (var rule in rules)
            {
                if (trans.name.Contains(rule.uiElementRegex))
                {
                    string transName = Regex.Replace(trans.name, @"\s", "");
                    string path = trans.name;
                    var current = trans.parent;
                    while (current)
                    {
                        if (!current.name.Contains("View") && !current.name.Contains("UIRoot"))
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (rule.componentName == UIComponentName.Button || rule.componentName == UIComponentName.Toggle)
                    {
                        path = transName + " = GetChildCompByObj<" + rule.componentType + ">(\"" + path + "\");";
                    }
                    else
                    {
                        path = transName + " = GetChildObj(\"" + path + "\");";
                    }

                    return path;
                }
            }

            return string.Empty;
        }

        public static string AddAddListener(Transform trans, string suffix)
        {
            var rules = ScriptGeneratorSetting.Instance.ScriptGenerateRule;
            foreach (var rule in rules)
            {
                if (trans.name.Contains(rule.uiElementRegex)
                    && rule.componentName is UIComponentName.Button or UIComponentName.Toggle)
                {
                    string transName = Regex.Replace(trans.name, @"\s", "");
                    string path = trans.name;
                    var current = trans.parent;
                    while (current)
                    {
                        if (!current.name.Contains("View") && !current.name.Contains("UIRoot"))
                        {
                            path = current.name + "/" + path;
                            current = current.parent;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (rule.componentName == UIComponentName.Button)
                    {
                        string converted = ConvertToEventName(transName).Replace("_", "");
                        path = transName + ".onClick.AddListener(" + converted + ");";
                    }
                    else if (rule.componentName == UIComponentName.Toggle)
                    {
                        string converted = ConvertToEventName(transName).Replace("_", "");
                        path = transName + ".onValueChanged.AddListener(" + converted + ");";
                    }

                    if (suffix == "Group")
                    {
                        return path;
                    }
                    else
                    {
                        return "View." + path;
                    }
                }
            }

            return string.Empty;
        }

        public static string AddAddListenerMethod(Transform trans)
        {
            var rules = ScriptGeneratorSetting.Instance.ScriptGenerateRule;
            foreach (var rule in rules)
            {
                if (trans.name.Contains(rule.uiElementRegex)
                    && rule.componentName is UIComponentName.Button or UIComponentName.Toggle)
                {
                    string transName = trans.name;
                    //获取UGUI组件
                    transName = Regex.Replace(transName, @"\s", "");
                    StringBuilder sb = new StringBuilder();
                    if (rule.componentName == UIComponentName.Button)
                    {
                        string converted = ConvertToEventName(transName).Replace("_", "");
                        sb.AppendLine("        " + "private void " + converted + "()");
                        sb.AppendLine("        {");
                        sb.AppendLine("        }");
                    }
                    else if (rule.componentName == UIComponentName.Toggle)
                    {
                        string converted = ConvertToEventName(transName).Replace("_", "");
                        sb.AppendLine("        " + "private void " + converted + "(bool" + " value)");
                        sb.AppendLine("        {");
                        sb.AppendLine("        }");
                    }

                    return sb.ToString(); // 8个空格代替\t\t\t
                }
            }

            return string.Empty;
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
        /// 检查文件夹中是否存在指定文件
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        /// <returns>是否存在</returns>
        public static bool FileExistsInFolder(string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
                return false;

            string fullPath = Path.Combine(folderPath, fileName);
            return File.Exists(fullPath);
        }

        public static Transform[] GetTransform(bool isAll)
        {
            var active = Selection.activeTransform;
            if (!active)
                return Array.Empty<Transform>();

            // 仅遍历当前选中对象的子树，避免全工程扫描带来的巨大开销
            var list = new List<Transform>(64);
            if (!isAll)
            {
                for (int i = 0; i < active.childCount; i++)
                    list.Add(active.GetChild(i));
            }
            else
            {
                var children = GetAllChildren(active, includeInactive: true);
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (isIgnore(child, "Component"))
                        continue;
                    list.Add(child);
                }
            }

            return list.ToArray();
        }

        public static bool isIgnore(Transform trans, string str)
        {
            string path = trans.name;
            var current = trans.parent;
            while (current)
            {
                if (!path.Contains(str))
                {
                    path = current.name + "/" + path;
                    current = current.parent;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Transform> GetAllChildren(Transform parent, bool includeInactive = true)
        {
            List<Transform> children = new List<Transform>();
            GetChildrenRecursive(parent, children, includeInactive);
            return children;
        }

        private static void GetChildrenRecursive(Transform parent, List<Transform> children, bool includeInactive)
        {
            foreach (Transform child in parent)
            {
                if (includeInactive || child.gameObject.activeInHierarchy)
                {
                    children.Add(child);
                    GetChildrenRecursive(child, children, includeInactive);
                }
            }
        }
    }
}