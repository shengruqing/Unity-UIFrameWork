using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 控件组View脚本。
    /// 只处理组件获取，不处理业务逻辑。
    /// </summary>
    public class UIGroup : ControlContainer
    {
        #region 字段

        /// <summary>
        /// 界面是否可见。
        /// </summary>
        protected bool _Visible;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化容器。
        /// </summary>
        /// <param name="obj">组件GameObject。</param>
        public virtual void InitContainer(GameObject obj)
        {
            gameObject = obj;
            transform = obj.transform;

            // 1. 先执行 View 的初始化（获取组件）
            OnInit();
        }

        /// <summary>
        /// 初始化容器（带父容器）。
        /// </summary>
        /// <param name="ownerContainer">父容器。</param>
        /// <param name="obj">组件GameObject。</param>
        public virtual void InitContainerByOwner(ControlContainer ownerContainer, GameObject obj)
        {
            gameObject = obj;
            transform = obj.transform;
        }

        #endregion

        #region 公共方法

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
        /// 显示组。
        /// </summary>
        public virtual void Show()
        {
            _Visible = true;
            SetActive(true);
            OnShow();
        }

        /// <summary>
        /// 隐藏组。
        /// </summary>
        public virtual void Hide()
        {
            _Visible = false;
            OnHide();
            SetActive(false);
        }

        /// <summary>
        /// 删除组。
        /// </summary>
        public virtual void Destroy()
        {
            _Visible = false;
            OnDestroy();
        }

        #endregion

        #region 虚方法回调

        /// <summary>
        /// 当初始化时回调，用于组件获取。
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 当显示时回调。
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// 当隐藏时回调。
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// 当删除时调用。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        #endregion
    }
}