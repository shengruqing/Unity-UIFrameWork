using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    /// <summary>
    /// UI对象控件容器抽象类。
    /// 提供UI组件的基础操作和事件管理功能。
    /// </summary>
    public class ControlContainer
    {
        #region 字段

        /// <summary>
        /// GameObject引用。
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// Transform引用。
        /// </summary>
        public Transform transform;

        public RectTransform ContentTrans;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public ControlContainer()
        {
        }

        #endregion

        #region 子类操作

        /// <summary>
        /// 获取子类GameObject。
        /// </summary>
        /// <param name="childPath">子类路径。</param>
        /// <returns>子类GameObject，不存在返回null。</returns>
        protected GameObject GetChildObj(string childPath)
        {
            if (gameObject == null) return null;
            // return UIHelper.GetChild(gameObject, childPath);
            Transform t = gameObject.transform.Find(childPath);
            return t != null ? t.gameObject : null;
        }

        /// <summary>
        /// 获取子类组件。
        /// </summary>
        /// <param name="childPath">子类路径。</param>
        /// <param name="componentType">组件类型。</param>
        /// <returns>组件实例，不存在返回null。</returns>
        private Component GetChildCompByObj(string childPath, System.Type componentType)
        {
            if (gameObject == null) return null;
            // return UIHelper.GetChildCompByObj(gameObject, childPath, componentType);
            Transform t = gameObject.transform.Find(childPath);
            if (t != null)
            {
                return t.GetComponent(componentType);
            }

            return null;
        }

        /// <summary>
        /// 获取子类组件（泛型版本）。
        /// </summary>
        /// <typeparam name="T">组件类型。</typeparam>
        /// <param name="childPath">子类路径。</param>
        /// <returns>组件实例，不存在返回null。</returns>
        protected T GetChildCompByObj<T>(string childPath) where T : Component
        {
            return (T)GetChildCompByObj(childPath, typeof(T));
        }

        #endregion

        #region 克隆操作

        /// <summary>
        /// 克隆同级物体。
        /// </summary>
        /// <param name="template">模板GameObject。</param>
        /// <param name="resetLocalTransform">是否重置本地Transform为默认值。</param>
        /// <returns>克隆的GameObject。</returns>
        public GameObject CloneTemplate(GameObject template, bool resetLocalTransform = false)
        {
            if (template == null)
            {
                return null;
            }

            GameObject cloneObj = Object.Instantiate(template, template.transform.parent, true);
            
            if (resetLocalTransform)
            {
                // 重置为默认值
                cloneObj.transform.localPosition = Vector3.zero;
                cloneObj.transform.localRotation = Quaternion.identity;
                cloneObj.transform.localScale = Vector3.one;
            }
            else
            {
                // 保持原Transform
                cloneObj.transform.localPosition = template.transform.localPosition;
                cloneObj.transform.localRotation = template.transform.localRotation;
                cloneObj.transform.localScale = template.transform.localScale;
            }
            
            return cloneObj;
        }

        /// <summary>
        /// 克隆同级组容器。
        /// </summary>
        /// <param name="template">模板GameObject。</param>
        /// <param name="groupInstance">组实例。</param>
        public void CloneGroup(GameObject template, UIGroup groupInstance)
        {
            GameObject cloneObj = CloneTemplate(template);
            if (cloneObj == null)
            {
                Logger.Warning("Warning: Clone group error.cloneObj is null");
                return;
            }

            if (groupInstance != null)
            {
                groupInstance.InitContainer(cloneObj);
            }
        }

        /// <summary>
        /// 克隆同级组容器并传入父类容器。
        /// </summary>
        /// <param name="ownerContainer">父类容器。</param>
        /// <param name="template">模板GameObject。</param>
        /// <param name="groupInstance">组实例。</param>
        public void CloneGroupByContainer(ControlContainer ownerContainer, GameObject template, UIGroup groupInstance)
        {
            GameObject cloneObj = CloneTemplate(template);
            if (cloneObj == null)
            {
                Logger.Warning("Warning: Clone group error.cloneObj is null");
                return;
            }

            if (groupInstance != null)
            {
                groupInstance.InitContainerByOwner(ownerContainer, cloneObj);
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 设置transform的父节点。
        /// </summary>
        /// <param name="trans">子节点Transform。</param>
        /// <param name="parent">父节点Transform。</param>
        /// <param name="resetLocalTransform">是否重置本地Transform为默认值。</param>
        public void Transform_SetParent(Transform trans, Transform parent, bool resetLocalTransform = false)
        {
            if (trans == null || parent == null) return;

            Vector3 pos = trans.localPosition;
            Quaternion rotation = trans.localRotation;
            Vector3 scale = trans.localScale;

            // 如果需要重置，则使用默认值
            if (resetLocalTransform)
            {
                pos = Vector3.zero;
                rotation = Quaternion.identity;
                scale = Vector3.one;
            }
            // 否则保持当前值（即原始逻辑的正确实现）

            trans.SetParent(parent, false); // false表示不自动设置worldPositionStays
            trans.localPosition = pos;
            trans.localRotation = rotation;
            trans.localScale = scale;
        }

        /// <summary>
        /// 重置transform的本地位置、旋转和缩放。
        /// </summary>
        /// <param name="trans">Transform对象。</param>
        public void Transform_Reset(Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }

        /// <summary>
        /// 设置transform及其所有子对象的层。
        /// </summary>
        /// <param name="trans">Transform对象。</param>
        /// <param name="layerName">层名称。</param>
        public void Transform_Setlayer(Transform trans, string layerName)
        {
            Transform[] children = trans.GetComponentsInChildren<Transform>(true);

            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }

        #endregion
    }
}