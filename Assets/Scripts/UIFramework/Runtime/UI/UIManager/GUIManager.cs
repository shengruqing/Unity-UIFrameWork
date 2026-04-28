using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 游戏界面管理器。
    /// 负责游戏中所有UI界面的创建、显示、隐藏、销毁和生命周期管理。
    /// 提供界面堆栈管理、界面查找、事件分发等核心功能。
    /// 支持同步调用接口，底层使用异步加载机制。
    /// </summary>
    public class GUIManager
    {
        #region 单例

        private static readonly Lazy<GUIManager> _instance = new Lazy<GUIManager>(() => new GUIManager());

        /// <summary>
        /// 获取GUIManager单例实例。
        /// </summary>
        public static GUIManager Instance => _instance.Value;

        #endregion

        #region 字段

        /// <summary>
        /// 界面堆栈，用于记录界面打开顺序 {index:viewName}。
        /// </summary>
        private List<string> uiStack = new List<string>();

        /// <summary>
        /// 界面顺序队列，用于返回键依次返回上次打开界面 {index:logic}。
        /// </summary>
        private List<UIViewLogic> orderViewStack = new List<UIViewLogic>();

        /// <summary>
        /// 所有界面表 {name:logic}。
        /// </summary>
        private Dictionary<string, UIViewLogic> viewList = new Dictionary<string, UIViewLogic>();

        /// <summary>
        /// 缓存正在加载的界面 {name:logic}。
        /// </summary>
        private Dictionary<string, UIViewLogic> _loadingCache = new Dictionary<string, UIViewLogic>();

        /// <summary>
        /// View对象存储 {name:view}。
        /// </summary>
        private Dictionary<string, UIView> viewObjects = new Dictionary<string, UIView>();

        /// <summary>
        /// 需要更新的界面Logic集合（HashSet保证O(1)查找）。
        /// </summary>
        private readonly HashSet<UIViewLogic> _updateSet = new HashSet<UIViewLogic>();

        /// <summary>
        /// 更新迭代快照缓冲，避免迭代期间集合被修改。
        /// </summary>
        private readonly List<UIViewLogic> _updateBuffer = new List<UIViewLogic>();

        public ScreenOrientation curOrientation = Screen.orientation; // 当前屏幕方向

        /// <summary>
        /// 性能统计信息
        /// </summary>
        private readonly Dictionary<string, long> _performanceStats = new Dictionary<string, long>();

        /// <summary>
        /// 界面创建计数器
        /// </summary>
        private int _viewCreateCount = 0;

        public bool canResponseShowExitGame = true;

        /// <summary>
        /// 从 Logic 类型获取界面名称（去掉 Logic 后缀）。
        /// </summary>
        private static string GetViewNameFromLogicType(Type logicType)
        {
            string viewName = logicType.Name;
            if (viewName.EndsWith("Logic"))
            {
                viewName = viewName.Substring(0, viewName.Length - 5);
            }

            return viewName;
        }

        #endregion

        #region 创建实例

        /// <summary>
        /// 创建View对象实例，支持自动查找View类。
        /// </summary>
        /// <param name="viewName">界面名称。</param>
        /// <returns>创建的UIView实例，如果失败返回UIView基类。</returns>
        private UIView CreateViewInstance(string viewName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                //Logger.Log($"创建View对象实例: {viewName}");

                // 使用类型缓存管理器查找类型
                var type = TypeCacheManager.FindViewType(viewName);

                if (type != null)
                {
                    try
                    {
                        var instance = (UIView)System.Activator.CreateInstance(type);
                        _viewCreateCount++;
                        stopwatch.Stop();
                        RecordPerformance($"CreateViewInstance_{viewName}", stopwatch.ElapsedMilliseconds);
                        //Logger.Log($"[CreateViewInstance] 成功创建View实例: {type.FullName}");
                        return instance;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[CreateViewInstance] 创建View实例失败 {type.FullName}: {ex.Message}");
                    }
                }

                // 如果都找不到，返回UIView基类
                Logger.Warning($"[CreateViewInstance] 未找到具体View类 {viewName}，使用UIView基类");
                return new UIView();
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }

        #endregion

        #region 界面管理

        private UIViewLogic CreateInstance(Type type)
        {
            UIViewLogic view = Activator.CreateInstance(type) as UIViewLogic;
            UIAttribute attribute = Attribute.GetCustomAttribute(type, typeof(UIAttribute)) as UIAttribute;

            if (view == null)
                throw new($"UIView {type.FullName} create instance failed.");

            if (attribute != null)
            {
                view.Init(attribute.Layer, attribute.CurStackMode, attribute.NeedUpdate,
                    attribute.IsNoNotch, attribute.IsUseAdjust);
            }
            else
            {
                view.Init(ViewLayer.Layer1, ViewStack.FullOnly);
            }

            return view;
        }

        /// <summary>
        /// 删除界面（内部方法）。
        /// </summary>
        /// <param name="viewName">界面名称。</param>
        /// <param name="isAll">是否删除堆栈中所有该界面实例。</param>
        private void DestroyViewInternal(string viewName, bool isAll)
        {
            // 直接从字典获取Logic
            if (!viewList.TryGetValue(viewName, out UIViewLogic logic))
                return;

            if (logic == null) return;

            if (isAll)
            {
                for (var i = uiStack.Count - 1; i >= 0; i--)
                {
                    if (uiStack[i] == viewName)
                    {
                        uiStack.RemoveAt(i);
                    }
                }

                for (var i = orderViewStack.Count - 1; i >= 0; i--)
                {
                    if (orderViewStack[i] == logic)
                    {
                        orderViewStack.RemoveAt(i);
                    }
                }
            }

            UnregisterUpdateView(logic);
            logic.Destroy();

            // 销毁View对象
            if (viewObjects.TryGetValue(viewName, out UIView view))
            {
                view.DestroyGameObject();
                viewObjects.Remove(viewName);
            }

            viewList.Remove(viewName);
        }

        /// <summary>
        /// 获取界面在顺序堆栈中的索引。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        /// <returns>索引位置，不存在返回-1。</returns>
        private int GetOrderStackIndex(UIViewLogic logic)
        {
            return orderViewStack.IndexOf(logic);
        }

        #endregion

        #region 界面显示控制

        /// <summary>
        /// 返回键弹出界面。
        /// </summary>
        private void EscapeCurView()
        {
            // iOS平台不支持ESC键返回
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            // 没有界面在堆栈中
            if (orderViewStack.Count == 0)
            {
                return;
            }

            // 从顶部开始处理返回事件
            while (orderViewStack.Count > 0)
            {
                var topIndex = orderViewStack.Count - 1;
                UIViewLogic logic = orderViewStack[topIndex];

                // 移除无效或不可见的界面
                if (logic == null || !logic.IsVisible())
                {
                    orderViewStack.RemoveAt(topIndex);
                    continue;
                }

                // 处理返回事件
                var escapeResult = logic.OnEscape();

                switch (escapeResult)
                {
                    case ViewEscape.Hide:
                        // 隐藏当前界面并停止传递
                        orderViewStack.RemoveAt(topIndex);
                        return;

                    case ViewEscape.Ignore:
                        // 忽略当前界面，继续处理下一个
                        orderViewStack.RemoveAt(topIndex);
                        break;

                    case ViewEscape.Wait:
                        // 等待状态，停止处理
                        return;

                    default:
                        // 未知状态，停止处理
                        Logger.Warning($"未知返回状态: {escapeResult}");
                        return;
                }
            }
        }

        #endregion

        #region 更新循环

        /// <summary>
        /// 实时更新所有可见的需要更新的界面。
        /// </summary>
        public void LateUpdate()
        {
            try
            {
                _updateBuffer.Clear();
                _updateBuffer.AddRange(_updateSet);
                foreach (var logic in _updateBuffer)
                {
                    if (logic != null && logic.IsVisible())
                        logic.Update();
                    else
                        _updateSet.Remove(logic);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"GUIManager.LateUpdate异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步战斗的UpdateLogic。
        /// </summary>
        /// <param name="delta">时间增量。</param>
        public void UpdateLogic(int delta)
        {
            try
            {
                _updateBuffer.Clear();
                _updateBuffer.AddRange(_updateSet);
                foreach (var logic in _updateBuffer)
                {
                    if (logic != null && logic.IsVisible())
                        logic.UpdateLogic(delta);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"GUIManager.UpdateLogic异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 检测按键输入（主要是ESC键）。
        /// </summary>
        public void Update()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                EscapeCurView();
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 显示界面（同步调用版本）。
        /// 底层使用异步加载，但对外提供同步接口。
        /// </summary>
        /// <typeparam name="T">界面Logic类型。</typeparam>
        /// <param name="args">传递给界面的参数。</param>
        /// <returns>界面Logic实例。</returns>
        public void ShowView<T>(params object[] args) where T : UIViewLogic
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            Debug.Log("开始加载界面：" + viewName);
            if (_loadingCache.ContainsKey(viewName))
            {
                Logger.Warning($"GUIManager.ShowView: {viewName} 正在加载中，请勿重复调用！");
                return;
            }

            if (viewList.TryGetValue(viewName, out var logic))
            {
                if (!logic.IsVisible())
                {
                    logic.Show(args);
                }

                logic.RegisterEvent();
            }
            else
            {
                logic = CreateInstance(typeof(T));
                if (logic == null)
                {
                    Logger.Error($"创建界面实例失败: {typeof(T).Name}");
                    return;
                }

                logic.Name = viewName;
                _loadingCache.Add(viewName, logic);
                // 必须在 LoadUIView 之前注册事件，否则同步/快速加载时 OnViewPrepare 可能在 RegisterEvent 之前执行，导致 _cachedEventMessages 为空
                logic.RegisterEvent();
                logic.LoadUIViewAsync(viewName, OnViewPrepare, args);
            }
        }

        private void OnViewPrepare(UIViewLogic uiViewLogic, GameObject viewObj)
        {
            // 检查UI逻辑对象是否已经被销毁
            if (uiViewLogic == null)
            {
                Logger.Error("OnViewPrepare: uiViewLogic 为 null");
                return;
            }

            if (viewObj != null)
            {
                UIView view = CreateViewInstance(uiViewLogic.Name);
                if (view != null)
                {
                    view.InitContainer(viewObj, uiViewLogic.Name, uiViewLogic);
                    view.AppLayer();
                    uiViewLogic.InitShow();
                    uiViewLogic.ExecuteEvents();
                    viewObjects[uiViewLogic.Name] = view;
                    viewList[uiViewLogic.Name] = uiViewLogic;
                }
                else
                {
                    Logger.Error($"创建UIView实例失败: {uiViewLogic.Name}");
                    // 即使View创建失败，也要清理加载缓存并调用UI逻辑的清理方法
                    uiViewLogic.Destroy();
                }
            }
            else
            {
                Logger.Error($"UI资源加载失败: {uiViewLogic.Name}，viewObj 为 null");

                // 销毁UI逻辑实例以避免内存泄漏
                uiViewLogic.Destroy();
            }

            // 总是清理加载缓存，无论成功还是失败
            _loadingCache.Remove(uiViewLogic.Name);
        }

        /// <summary>
        /// 是否正在加载界面
        /// </summary>
        /// <returns>是否正在加载界面</returns>
        public bool IsLoadingCache<T>()
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            return _loadingCache.ContainsKey(viewName);
        }

        /// <summary>
        /// 获取界面Logic实例。
        /// </summary>
        /// <typeparam name="T">界面Logic类型。</typeparam>
        /// <returns>界面Logic实例，不存在返回null。</returns>
        public T GetView<T>() where T : UIViewLogic
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            viewList.TryGetValue(viewName, out UIViewLogic logic);
            return logic as T;
        }

        /// <summary>
        /// 隐藏界面。
        /// </summary>
        /// <typeparam name="T">界面Logic类型。</typeparam>
        public void HideView<T>() where T : UIViewLogic
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            if (!viewList.TryGetValue(viewName, out var logic)) return;
            if (logic != null && logic.IsVisible())
            {
                logic.Hide();
            }

            Logger.Log("隐藏界面：" + viewName);
        }

        /// <summary>
        /// 是否有任意窗口正在加载。
        /// </summary>
        public bool IsAnyLoading()
        {
            if (_loadingCache.Count > 0)
                return true;

            for (int i = 0; i < orderViewStack.Count; i++)
            {
                var view = orderViewStack[i];
                if (view is { IsLoadDone: false })
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 压入UI堆栈。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void PushView(UIViewLogic logic)
        {
            if (logic == null) return;

            switch (logic.CurStackMode)
            {
                case ViewStack.OverLay:
                    return;
                case ViewStack.FullOnly when uiStack.Count > 0:
                {
                    string topViewName = uiStack[uiStack.Count - 1];
                    if (topViewName != null && topViewName != logic.Name)
                    {
                        if (viewList.TryGetValue(topViewName, out var topLogic))
                        {
                            topLogic?.ActiveHide(false);
                        }

                        uiStack.Add(logic.Name);
                    }

                    break;
                }
                case ViewStack.FullOnly:
                    uiStack.Add(logic.Name);
                    break;
                case ViewStack.OverMain:
                {
                    if (uiStack.Count > 0)
                    {
                        string topViewName = uiStack[uiStack.Count - 1];
                        if (topViewName != null && topViewName != logic.Name)
                        {
                            if (viewList.TryGetValue(topViewName, out var topLogic))
                            {
                                if (topLogic != null && topLogic.CurStackMode == logic.CurStackMode)
                                {
                                    topLogic.ActiveHide(false);
                                }
                            }

                            uiStack.Add(logic.Name);
                        }
                    }
                    else
                    {
                        uiStack.Add(logic.Name);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 弹出UI堆栈。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void PopView(UIViewLogic logic)
        {
            if (logic == null) return;

            if (logic.CurStackMode == ViewStack.OverLay || uiStack.Count == 0)
            {
                return;
            }

            string topViewName = uiStack[uiStack.Count - 1];
            uiStack.RemoveAt(uiStack.Count - 1);

            if (logic.Name == topViewName && uiStack.Count > 0)
            {
                string nextStack = uiStack[uiStack.Count - 1];
                if (viewList.TryGetValue(nextStack, out UIViewLogic nextLogic) && nextLogic != null)
                {
                    nextLogic.ActiveShow(false);
                }
            }
        }

        /// <summary>
        /// 删除界面。
        /// </summary>
        /// <param name="viewName">界面名称。</param>
        /// <param name="isAll">是否删除堆栈中所有该界面实例，默认false。</param>
        public void DestroyView(string viewName, bool isAll = false)
        {
            DestroyViewInternal(viewName, isAll);
        }

        /// <summary>
        /// 删除界面。
        /// </summary>
        /// <typeparam name="T">界面Logic类型。</typeparam>
        /// <param name="isAll">是否删除堆栈中所有该界面实例，默认false。</param>
        public void DestroyView<T>(bool isAll = false) where T : UIViewLogic
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            DestroyViewInternal(viewName, isAll);
        }

        /// <summary>
        /// 隐藏所有OverLay叠加界面。
        /// </summary>
        public void HideAllOverLayView()
        {
            foreach (var kvp in viewList)
            {
                UIViewLogic logic = kvp.Value;
                if (logic == null || !logic.IsVisible()) continue;
                if (logic.CurStackMode == ViewStack.OverLay)
                    logic.Hide();
            }
        }

        /// <summary>
        /// 隐藏所有层级大于Bottom的界面。
        /// </summary>
        public void HideAllLayerMoreThanZero()
        {
            foreach (var kvp in viewList)
            {
                UIViewLogic logic = kvp.Value;
                if (logic == null || !logic.IsVisible()) continue;
                if (logic.Layer > ViewLayer.Bottom)
                    logic.Hide();
            }
        }

        /// <summary>
        /// 删除所有界面。
        /// </summary>
        public void DestroyAllView()
        {
            try
            {
                foreach (var kvp in viewList)
                {
                    UIViewLogic logic = kvp.Value;
                    if (logic == null) continue;
                    try
                    {
                        logic.ActiveHide(false);
                        logic.Destroy();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"销毁界面 {kvp.Key} 时出错: {ex.Message}");
                    }
                }

                foreach (var kvp in viewObjects)
                {
                    UIView view = kvp.Value;
                    try
                    {
                        view?.DestroyGameObject();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"销毁View对象 {kvp.Key} 时出错: {ex.Message}");
                    }
                }

                foreach (var kvp in _loadingCache)
                {
                    UIViewLogic logic = kvp.Value;
                    if (logic == null) continue;
                    try
                    {
                        logic.ActiveHide(false);
                        logic.Destroy();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"销毁界面 {kvp.Key} 时出错: {ex.Message}");
                    }
                }

                uiStack.Clear();
                _updateSet.Clear();
                _updateBuffer.Clear();
                orderViewStack.Clear();
                viewList.Clear();
                viewObjects.Clear();
                _loadingCache.Clear();

                Logger.Log("GUIManager: 所有界面已销毁");
            }
            catch (Exception ex)
            {
                Logger.Error($"DestroyAllView异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 判断界面是否在显示。
        /// </summary>
        /// <typeparam name="T">界面Logic类型。</typeparam>
        /// <returns>true表示显示，false表示未显示。</returns>
        public bool IsViewVisible<T>() where T : UIViewLogic
        {
            string viewName = GetViewNameFromLogicType(typeof(T));
            if (!viewList.TryGetValue(viewName, out var logic)) return false;
            return logic != null && logic.IsVisible();
        }

        /// <summary>
        /// 获取当前已创建的界面数量。
        /// </summary>
        public int GetViewCount()
        {
            return viewList.Count;
        }

        /// <summary>
        /// 获取所有界面的只读视图（用于调试或遍历）。
        /// </summary>
        public IReadOnlyDictionary<string, UIViewLogic> GetAllViews()
        {
            return viewList;
        }

        /// <summary>
        /// 将Logic压入顺序堆栈。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void PushOrderStack(UIViewLogic logic)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            if (logic != null && GetOrderStackIndex(logic) == -1)
            {
                orderViewStack.Add(logic);
            }
        }

        /// <summary>
        /// 从顺序堆栈中弹出Logic。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void PopOrderStack(UIViewLogic logic)
        {
            orderViewStack.Remove(logic);
        }

        /// <summary>
        /// 显示退出游戏确认界面。
        /// </summary>
        public void ShowExitGame()
        {
            if (canResponseShowExitGame)
            {
            }
        }

        /// <summary>
        /// 释放UI
        /// </summary>
        public void Release()
        {
            Debug.Log("info: release ui manager");
            uiStack.Clear();
            DestroyAllView();
        }

        /// <summary>
        /// 卸载无用界面
        /// </summary>
        public void UnloadUnusedView()
        {
            List<UIViewLogic> unloadViewKeys = new List<UIViewLogic>();
            foreach (var view in viewList.Values)
            {
                if (view != null && !view.IsVisible() && view.CanUnload)
                {
                    unloadViewKeys.Add(view);
                }
            }

            for (int i = unloadViewKeys.Count - 1; i >= 0; i--)
            {
                DestroyView(unloadViewKeys[i].Name);
            }
        }

        /// <summary>
        /// 屏幕方向改变时调用。
        /// </summary>
        public void OnOrientationChanged(ScreenOrientation newOrientation)
        {
            curOrientation = newOrientation;
            Logger.Log("GUIManager: 屏幕方向已改变为 " + curOrientation);
            foreach (var kvp in viewObjects)
            {
                UIView logic = kvp.Value;
                if (logic == null) continue;
                logic.AdjustResolution();
            }
        }

        /// <summary>
        /// 获取当前屏幕方向。
        /// </summary>
        public ScreenOrientation GetCurOrientation()
        {
            return curOrientation;
        }

        /// <summary>
        /// 注册需要更新的Logic。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void RegisterUpdateView(UIViewLogic logic)
        {
            if (logic == null || !logic.CanUpdate()) return;
            _updateSet.Add(logic);
        }

        /// <summary>
        /// 注销需要更新的Logic。
        /// </summary>
        /// <param name="logic">界面Logic实例。</param>
        public void UnregisterUpdateView(UIViewLogic logic)
        {
            if (logic == null) return;
            _updateSet.Remove(logic);
        }

        /// <summary>
        /// 记录性能统计信息
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <param name="duration">耗时（毫秒）</param>
        public void RecordPerformance(string operation, long duration)
        {
            _performanceStats[operation] = duration;
        }

        /// <summary>
        /// 获取性能统计信息
        /// </summary>
        /// <returns>性能统计字典</returns>
        public Dictionary<string, long> GetPerformanceStats()
        {
            return new Dictionary<string, long>(_performanceStats);
        }

        /// <summary>
        /// 获取界面创建统计
        /// </summary>
        /// <returns>界面创建计数</returns>
        public int GetViewCreateCount()
        {
            return _viewCreateCount;
        }

        /// <summary>
        /// 重置性能统计
        /// </summary>
        public void ResetPerformanceStats()
        {
            _performanceStats.Clear();
            _viewCreateCount = 0;
        }

        #endregion

        public void SetCanResponseShowExitGame(bool b)
        {
            canResponseShowExitGame = b;
        }
    }
}