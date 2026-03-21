using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// UI根节点管理器。
    /// 管理UI根节点、UI相机、Canvas等基础组件。
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        #region 单例

        /// <summary>
        /// UI根节点单例。
        /// </summary>
        private static UIRoot m_Instance = null;

        /// <summary>
        /// 获取UI根节点单例。
        /// </summary>
        public static UIRoot Instance
        {
            get { return m_Instance; }
        }

        #endregion

        #region 字段

        /// <summary>
        /// UI根节点Transform组件。
        /// </summary>
        private Transform m_Trans;

        /// <summary>
        /// 获取UI根节点Transform。
        /// </summary>
        public Transform Trans
        {
            get { return m_Trans; }
        }

        /// <summary>
        /// UI相机。
        /// </summary>
        private Camera m_UICamera;

        /// <summary>
        /// 获取UI相机。
        /// </summary>
        public Camera UICameara
        {
            get { return m_UICamera; }
        }

        /// <summary>
        /// Canvas缩放器。
        /// </summary>
        private CanvasScaler scaler;

        /// <summary>
        /// 获取Canvas缩放器。
        /// </summary>
        public CanvasScaler Scaler
        {
            get { return scaler; }
        }

        /// <summary>
        /// 根Canvas。
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// 获取根Canvas。
        /// </summary>
        public Canvas Canvas
        {
            get { return canvas; }
        }

        /// <summary>
        /// 根RectTransform。
        /// </summary>
        private RectTransform rt;

        /// <summary>
        /// 获取根RectTransform。
        /// </summary>
        public RectTransform rectTransform
        {
            get { return rt; }
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 屏幕坐标转换到相对于Canvas坐标。
        /// </summary>
        /// <param name="position">屏幕坐标。</param>
        /// <returns>Canvas坐标。</returns>
        public Vector2 Screen2Canvas(Vector2 position)
        {
            Vector2 world = UICameara.ScreenToViewportPoint(position);
            return World2Canvas(world);
        }

        /// <summary>
        /// Canvas坐标转换到屏幕坐标。
        /// </summary>
        /// <param name="position">Canvas坐标。</param>
        /// <returns>屏幕坐标。</returns>
        public Vector2 Canvas2Screen(Vector2 position)
        {
            Vector3 worldPos = rectTransform.TransformPoint(position);
            return m_UICamera.WorldToScreenPoint(worldPos);
        }

        /// <summary>
        /// 世界坐标转换为相对于Canvas坐标。
        /// </summary>
        /// <param name="position">世界坐标。</param>
        /// <returns>Canvas坐标。</returns>
        public Vector2 World2Canvas(Vector2 position)
        {
            return rectTransform.InverseTransformPoint(position);
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// Awake初始化。
        /// </summary>
        private void Awake()
        {
            if (UIRoot.Instance != null)
            {
                throw new UnityException("UIRoot can't duplicate!");
            }

            m_Instance = this;
            m_Trans = transform;
            m_UICamera = transform.Find("UICamera").GetComponent<Camera>();
            if (m_UICamera == null)
            {
                throw new UnityException("UICamera Not Found! Please Add UI Camera in UIRoot Child");
            }

            scaler = transform.GetComponent<CanvasScaler>();
            canvas = transform.GetComponent<Canvas>();
            float scalerAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;
            float aspect = (float)Screen.width / (float)Screen.height;
            scaler.matchWidthOrHeight = scalerAspect > aspect ? 0 : 1;
            rt = GetComponent<RectTransform>();
        }

        private void Update()
        {
            GUIManager.Instance.Update();
        }

        private void LateUpdate()
        {
            GUIManager.Instance.LateUpdate();
        }

        #endregion
    }
}