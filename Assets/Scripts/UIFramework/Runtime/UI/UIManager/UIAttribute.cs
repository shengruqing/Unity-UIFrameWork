using System;

namespace GameLogic
{
    /// <summary>
    /// UI窗口属性特性。
    /// 用于标记UI界面的基本属性，如层级、堆栈模式和更新需求。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIAttribute : Attribute
    {
        /// <summary>
        /// 窗口层级。
        /// 决定界面的显示顺序和层级关系。
        /// </summary>
        public readonly ViewLayer Layer;

        /// <summary>
        /// 当前界面堆栈模式。
        /// 控制界面在堆栈中的行为和显示方式。
        /// </summary>
        public readonly ViewStack CurStackMode;

        /// <summary>
        /// 是否需要更新。
        /// true表示界面需要每帧更新，false表示不需要。
        /// </summary>
        public readonly bool NeedUpdate;

        /// 是否此view不关心刘海屏的安全区域(safeArea)。
        public readonly bool IsNoNotch;

        /// 是否使用双边刘海的适配方式。
        public readonly bool IsUseAdjust;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="layer">窗口层级。</param>
        /// <param name="curStackMode">堆栈模式，默认为FullOnly。</param>
        /// <param name="needUpdate">是否需要更新，默认为false。</param>
        /// <param name="isNoNotch">是否不关心刘海屏的安全区域，默认为false。</param>
        /// <param name="isUseAdjust">是否使用双边刘海适配方式，默认为false。</param>
        public UIAttribute(ViewLayer layer, ViewStack curStackMode = ViewStack.FullOnly, bool needUpdate = false, bool isNoNotch = false, bool isUseAdjust = false)
        {
            Layer = layer;
            CurStackMode = curStackMode;
            NeedUpdate = needUpdate;
            IsNoNotch = isNoNotch;
            IsUseAdjust = isUseAdjust;
        }

    }
}