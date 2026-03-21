namespace GameLogic
{
    /// <summary>
    /// 界面堆栈模式。
    /// 用于控制界面显示和堆栈管理行为。
    /// </summary>
    public enum ViewStack
    {
        /// <summary>
        /// 全屏模式，仅显示此界面。
        /// 当前界面显示时会隐藏其他所有界面。
        /// </summary>
        FullOnly = 0,

        /// <summary>
        /// 覆盖主界面模式，除主界面只会显示此界面。
        /// 允许保留主界面的同时显示当前界面。
        /// </summary>
        OverMain = 1,

        /// <summary>
        /// 叠加模式，界面显示时一直向上叠加显示。
        /// 界面会叠加在其他界面上方显示。
        /// </summary>
        OverLay = 2,
    }

    /// <summary>
    /// 界面回退返回状态。
    /// 用于处理ESC键或返回按钮的行为。
    /// </summary>
    public enum ViewEscape
    {
        /// <summary>
        /// 隐藏状态，响应后停止向下一界面传递回退消息。
        /// 当前界面隐藏后不再传递回退事件。
        /// </summary>
        Hide = 0,

        /// <summary>
        /// 忽略状态，继续向下一顺序界面传递回退消息。
        /// 当前界面忽略回退事件，传递给下一个界面处理。
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// 等待状态，回退不处理。
        /// 暂时阻止回退操作，等待用户进一步操作。
        /// </summary>
        Wait = 2,
    }

    /// <summary>
    /// 界面层级。
    /// 定义界面的显示顺序和层级关系，数值越大显示层级越高。
    /// </summary>
    public enum ViewLayer
    {
        /// <summary>
        /// 底层，最底层的界面。
        /// </summary>
        Bottom = 0,

        /// <summary>
        /// 默认层，普通界面使用的层级。
        /// </summary>
        Default = 1,

        /// <summary>
        /// 中间层级1，用于较重要的界面。
        /// </summary>
        Layer1 = 10,

        /// <summary>
        /// 中间层级2，用于重要界面。
        /// </summary>
        Layer2 = 20,

        /// <summary>
        /// 中间层级3，用于很重要界面。
        /// </summary>
        Layer3 = 30,

        /// <summary>
        /// 中间层级4，用于非常重要的界面。
        /// </summary>
        Layer4 = 40,

        /// <summary>
        /// 弹出层，用于弹窗类界面。
        /// </summary>
        Pop = 50,

        /// <summary>
        /// 弹出层2，用于特殊弹窗。
        /// </summary>
        Pop2 = 55,

        /// <summary>
        /// 顶层，用于最上层界面。
        /// </summary>
        Top = 60,

        /// <summary>
        /// 顶层2，用于特殊顶层界面。
        /// </summary>
        Top2 = 65,

        /// <summary>
        /// 提示层，用于提示信息。
        /// </summary>
        Tip = 70,

        /// <summary>
        /// 引导层，用于游戏引导。
        /// </summary>
        Guide = 80,

        /// <summary>
        /// 引导提示层，用于引导过程中的提示。
        /// </summary>
        GuideTip = 81,

        /// <summary>
        /// 引导跳转层，用于引导跳转界面。
        /// </summary>
        GuideJump = 82,

        /// <summary>
        /// 遮罩层，用于半透明遮罩效果。
        /// </summary>
        Mask = 89,

        /// <summary>
        /// 加载层，用于加载界面。
        /// </summary>
        Loading = 90,

        /// <summary>
        /// 网络阻塞层，用于网络阻塞提示。
        /// </summary>
        NetBlock = 95,

        /// <summary>
        /// 模式层，用于特殊模式界面。
        /// </summary>
        Mode = 100,

        /// <summary>
        /// 初始化层，仅InitView界面使用。
        /// 用于游戏初始化界面。
        /// </summary>
        Init = 101,
    }
}