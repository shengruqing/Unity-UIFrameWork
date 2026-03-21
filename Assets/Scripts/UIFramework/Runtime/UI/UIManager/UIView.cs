using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// 游戏界面View脚本。
    /// 只处理UI显示和用户交互，不处理业务逻辑。
    /// 
    /// 【架构规范】
    /// - View层：负责UI组件的获取、显示、用户交互事件转发
    /// - Logic层：负责业务逻辑、数据处理、通过View提供的公开方法操作UI
    /// - 禁止：Logic层直接访问UI组件
    /// - 禁止：View层编写业务逻辑代码
    /// </summary>
    public class UIView : ControlContainer
    {
        #region 字段

        /// <summary>
        /// 图形射线检测器。
        /// </summary>
        public GraphicRaycaster graphicRaycaster;

        /// <summary>
        /// 界面偏移。
        /// </summary>
        public float ViewOffset = 0;

        /// <summary>
        /// 关联的Logic对象。
        /// </summary>
        protected UIViewLogic _Logic;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化View容器，设置逻辑对象。
        /// </summary>
        /// <param name="obj">界面GameObject。</param>
        /// <param name="viewName">界面名称。</param>
        /// <param name="logic">关联的Logic对象。</param>
        public virtual void InitContainer(GameObject obj, string viewName, UIViewLogic logic)
        {
            gameObject = GameObject.Instantiate(obj);
            transform = gameObject.GetComponent<RectTransform>();
            _Logic = logic;

            if (UIRoot.Instance != null)
            {
                transform.SetParent(UIRoot.Instance.Trans);
            }

            ((RectTransform)transform).anchoredPosition3D = Vector3.zero;
            ((RectTransform)transform).offsetMax = Vector2.zero;
            ((RectTransform)transform).offsetMin = Vector2.zero;
            transform.localScale = Vector3.one;
            if (transform.Find("ContentTrans") != null)
            {
                ContentTrans = transform.Find("ContentTrans").gameObject.GetComponent<RectTransform>();
            }
            // Handle GraphicRaycaster
            graphicRaycaster = gameObject.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            ViewOffset = ((RectTransform)transform).sizeDelta.x / 2;
            //刘海屏 分辨率适配
            AdjustResolution();
            //自动初始化
            AutoInit();
            // 1. 先执行 View 的初始化（获取组件）
            OnInit();

            // 2. 再初始化 Logic（此时组件已经获取完毕，Logic 可以安全访问）
            _Logic?.InitLogic(this, viewName);
        }

        protected virtual void AutoInit()
        {
        }

        /// <summary>
        /// 分辨率适配
        /// </summary>
        public void AdjustResolution()
        {
            var notchHeight = SDKManager.Instance.GetNotchHeight();
            if (_Logic != null)
            {
                if (!_Logic.IsNoNotch && notchHeight > 0)
                {
                    if (_Logic.IsUseAdjust)
                    {
                        ((RectTransform)transform).sizeDelta = new Vector2(-notchHeight * 2, 0);
                        ViewOffset = ((RectTransform)transform).sizeDelta.x / 2;
                    }
                    else
                    {
                        if (ContentTrans != null)
                        {
                            if (GUIManager.Instance.GetCurOrientation() == ScreenOrientation.LandscapeLeft)
                            {
                                ContentTrans.offsetMin = new Vector2(notchHeight, 0);
                                ContentTrans.offsetMax = new Vector2(0, 0);
                            }
                            else
                            {
                                ContentTrans.offsetMin = new Vector2(0, 0);
                                ContentTrans.offsetMax = new Vector2(-notchHeight, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 应用层级设置。
        /// </summary>
        public virtual void AppLayer()
        {
            if (transform != null && _Logic != null)
            {
                var canvas = transform.GetComponent<Canvas>();
                if (canvas == null)
                {
                    Logger.Error("View root gameobject must be canvas!");
                    return;
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = (int)_Logic.Layer;
            }
        }

        #endregion

        #region 公共方法 - UI基础操作

        /// <summary>
        /// 设置交互性。
        /// </summary>
        /// <param name="isInteractable">true可交互，false不可交互。</param>
        public void SetInteractable(bool isInteractable)
        {
            if (graphicRaycaster != null)
                graphicRaycaster.enabled = isInteractable;
        }

        /// <summary>
        /// 设置激活状态。
        /// </summary>
        /// <param name="active">true激活，false取消激活。</param>
        public void SetActive(bool active)
        {
            if (gameObject != null)
                gameObject.SetActive(active);
        }

        /// <summary>
        /// 销毁GameObject。
        /// </summary>
        public void DestroyGameObject()
        {
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        #endregion

        #region 公共方法 - 常用UI组件操作

        /// <summary>
        /// 设置文本内容。
        /// 【View层规范】此方法由Logic层调用，用于更新UI显示。
        /// </summary>
        /// <param name="text">Text组件。</param>
        /// <param name="content">文本内容。</param>
        protected void SetText(Text text, string content)
        {
            if (text != null)
            {
                text.text = content;
            }
        }

        /// <summary>
        /// 设置按钮点击事件。
        /// 【View层规范】此方法由View自身在OnInit中调用，用于绑定UI交互事件。
        /// </summary>
        /// <param name="button">Button组件。</param>
        /// <param name="callback">点击回调。</param>
        protected void SetButtonClick(Button button, UnityAction callback)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(callback);
            }
        }

        /// <summary>
        /// 设置按钮交互状态。
        /// </summary>
        /// <param name="button">Button组件。</param>
        /// <param name="interactable">是否可交互。</param>
        protected void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        /// <summary>
        /// 设置图片精灵。
        /// </summary>
        /// <param name="image">Image组件。</param>
        /// <param name="sprite">精灵对象。</param>
        protected void SetImageSprite(Image image, Sprite sprite)
        {
            if (image != null)
            {
                image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置图片颜色。
        /// </summary>
        /// <param name="image">Image组件。</param>
        /// <param name="color">颜色。</param>
        protected void SetImageColor(Image image, Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
        }

        /// <summary>
        /// 设置滑动条值。
        /// </summary>
        /// <param name="slider">Slider组件。</param>
        /// <param name="value">值。</param>
        protected void SetSliderValue(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.value = value;
            }
        }

        /// <summary>
        /// 设置GameObject激活状态。
        /// </summary>
        /// <param name="targetObj">目标GameObject。</param>
        /// <param name="active">是否激活。</param>
        protected void SetGameObjectActive(GameObject targetObj, bool active)
        {
            if (targetObj != null)
            {
                targetObj.SetActive(active);
            }
        }

        #endregion

        #region 虚方法回调

        /// <summary>
        /// 初始化回调，界面需重写此方法用于组件获取。
        /// </summary>
        protected virtual void OnInit()
        {
        }

        #endregion
    }
}