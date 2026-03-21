using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sun.Runtime.UniEvent.Runtime;
using UnityEngine;
using Object = System.Object;

namespace GameLogic
{
    /// <summary>
    /// UI逻辑基类。
    /// 处理UI的业务逻辑、生命周期和事件管理。
    /// 所有自定义界面Logic都应继承此类。
    /// </summary>
    public class UIViewLogic
    {
        #region 字段

        /// <summary>
        /// 界面名称。
        /// </summary>
        public string Name;

        /// <summary>
        /// 界面层级。
        /// </summary>
        public ViewLayer Layer = ViewLayer.Default;

        /// <summary>
        /// 当前界面堆栈模式。
        /// </summary>
        public ViewStack CurStackMode = ViewStack.FullOnly;

        /// <summary>
        /// 是否播放音效。
        /// </summary>
        public bool IsAudio = true;

        /// <summary>
        /// 是否需要更新。
        /// </summary>
        public bool NeedUpdate = false;

        /// 是否不关心刘海屏的安全区域。
        public bool IsNoNotch = false;

        /// 是否使用双边刘海的适配方式。
        public bool IsUseAdjust = false;

        /// <summary>
        /// 界面是否可见。
        /// </summary>
        protected bool _Visible;

        /// <summary>
        /// 关联的View对象。
        /// </summary>
        protected UIView _View;

        /// <summary>
        /// 用户传入的数据。
        /// </summary>
        protected Object[] userDatas;

        /// <summary>
        /// 准备完成后的回调函数。
        /// </summary>
        private System.Action<UIViewLogic, GameObject> _prepareCallback;

        /// <summary>
        /// 用于取消异步加载的令牌源（Destroy 时取消）。
        /// </summary>
        private CancellationTokenSource _loadCts;

        /// <summary>
        /// 用于缓存事件监听器的结构，保存监听器和其事件ID信息。
        /// </summary>
        private struct QueuedEventListener
        {
            public Action<GameEventArgs> Listener;
            public string EventId;

            public QueuedEventListener(Action<GameEventArgs> listener, string eventId)
            {
                Listener = listener;
                EventId = eventId;
            }
        }

        /// <summary>
        /// 缓存已接收到的事件消息实例，用于在监听器注册后重新派发。
        /// </summary>
        private readonly Dictionary<string, List<GameEventArgs>> _cachedEventMessages =
            new Dictionary<string, List<GameEventArgs>>();

        /// <summary>
        /// 事件消息队列，用于存储待处理的事件监听器。
        /// </summary>
        private readonly Queue<QueuedEventListener> _eventMessageQueue = new Queue<QueuedEventListener>();

        /// <summary>
        /// 跟踪已注册的事件监听器，避免重复注册。
        /// </summary>
        private readonly Dictionary<string, List<Action<GameEventArgs>>> _registeredListeners =
            new Dictionary<string, List<Action<GameEventArgs>>>();

        /// <summary>
        /// 跟踪包装后的监听器引用，用于正确移除。
        /// </summary>
        private readonly Dictionary<Action<GameEventArgs>, Action<GameEventArgs>> _wrappedListeners =
            new Dictionary<Action<GameEventArgs>, Action<GameEventArgs>>();

        /// <summary>
        /// 事件系统线程锁对象。
        /// </summary>
        private readonly object _eventLock = new object();

        /// <summary>
        /// 标记资源是否加载完成。
        /// </summary>
        internal bool IsLoadDone = false;

        /// <summary>
        /// 标记UI是否已被销毁。
        /// </summary>
        internal bool IsDestroyed = false;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化界面参数。
        /// </summary>
        /// <param name="layer">界面层级。</param>
        /// <param name="curStackMode">当前堆栈模式。</param>
        /// <param name="needUpdate">是否需要更新。</param>
        /// <param name="isNoNotch">是否不关心刘海屏的安全区域。</param>
        /// <param name="isUseAdjust">是否使用双边刘海适配。</param>
        public void Init(ViewLayer layer, ViewStack curStackMode, bool needUpdate = false, bool isNoNotch = false, bool isUseAdjust = false)
        {
            Layer = layer;
            CurStackMode = curStackMode;
            NeedUpdate = needUpdate;
            IsNoNotch = isNoNotch;
            IsUseAdjust = isUseAdjust;
        }

        /// <summary>
        /// 初始化Logic，绑定View。
        /// </summary>
        /// <param name="view">View对象实例。</param>
        /// <param name="viewName">界面名称。</param>
        public virtual void InitLogic(UIView view, string viewName)
        {
            _View = view;
            Name = viewName;
            _Visible = false;
            OnInit();
        }

        /// <summary>
        /// 显示界面。
        /// </summary>
        /// <param name="args">传递给界面的参数。</param>
        public virtual void Show(params object[] args)
        {
            userDatas = args;
            ActiveShow(true);
        }

        /// <summary>
        /// 初始化显示界面。
        /// </summary>
        public void InitShow()
        {
            ActiveShow(true);
        }

        /// <summary>
        /// 隐藏界面。
        /// </summary>
        public virtual void Hide()
        {
            ActiveHide(true);
        }

        /// <summary>
        /// 删除界面。
        /// </summary>
        public virtual void Destroy()
        {
            _Visible = false;
            IsDestroyed = true;
            try
            {
                _loadCts?.Cancel();
                _loadCts?.Dispose();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _loadCts = null;
            }

            // 清空回调函数
            _prepareCallback = null;
            RemoveAllUIEvent();

            // 清空事件队列以避免内存泄漏
            ClearEventQueue();

            // 清空缓存的事件消息
            _cachedEventMessages.Clear();

            OnDestroy();
        }

        /// <summary>
        /// 界面实时更新（每帧调用）。
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// 同步战斗UpdateLogic的算法。
        /// </summary>
        /// <param name="delta">时间增量。</param>
        public virtual void UpdateLogic(int delta)
        {
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置界面可见性。
        /// </summary>
        /// <param name="visible">true显示，false隐藏。</param>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// 返回界面是否可见。
        /// </summary>
        /// <returns>true表示可见，false表示不可见。</returns>
        public bool IsVisible()
        {
            return _Visible;
        }

        /// <summary>
        /// 判断是否需要更新。
        /// </summary>
        /// <returns>true表示需要更新，false表示不需要。</returns>
        public bool CanUpdate()
        {
            return NeedUpdate;
        }

        #endregion

        #region 虚方法回调

        /// <summary>
        /// 初始化回调，界面需重写此方法进行自定义初始化。
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
            userDatas = null;
            RemoveAllUIEvent();
        }

        /// <summary>
        /// 按返回键时界面回调，需重写。
        /// </summary>
        /// <returns>返回回退状态。</returns>
        public virtual ViewEscape OnEscape()
        {
            return ViewEscape.Hide;
        }

        /// <summary>
        /// 界面删除时回调。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 激活显示界面（内部方法，具体界面请勿调用）。
        /// </summary>
        /// <param name="isActive">是否激活堆栈操作。</param>
        public void ActiveShow(bool isActive)
        {
            GUIManager.Instance.PushOrderStack(this);
            if (isActive)
            {
                GUIManager.Instance.PushView(this);
            }

            if (IsAudio && isActive && Layer < ViewLayer.Pop)
            {
                // PlayUISound(AudioDefine.Button_ViewOpen);
            }

            _Visible = true;
            if (_View != null)
            {
                _View.SetActive(true);
                _View.SetInteractable(true);
            }

            GUIManager.Instance.RegisterUpdateView(this);

            OnShow();
        }

        /// <summary>
        /// 激活隐藏界面（内部方法，具体界面请勿调用）。
        /// </summary>
        /// <param name="isActive">是否激活堆栈操作。</param>
        public void ActiveHide(bool isActive)
        {
            GUIManager.Instance.PopOrderStack(this);

            _Visible = false;
            GUIManager.Instance.UnregisterUpdateView(this);
            OnHide();

            if (_View != null)
            {
                _View.SetActive(false);
            }

            if (isActive)
            {
                GUIManager.Instance.PopView(this);
            }
        }

        #endregion

        #region UI事件

        /// <summary>
        /// 注册事件（由子类重写）。
        /// </summary>
        public virtual void RegisterEvent()
        {
        }

        /// <summary>
        /// 订阅事件。
        /// </summary>
        /// <param name="eventId">事件ID。</param>
        /// <param name="listener">事件监听回调。</param>
        protected void AddListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null)
            {
                Logger.Warning($"[{Name}] 事件订阅失败: 事件ID或监听器为空！事件ID: {eventId}");
                return;
            }

            lock (_eventLock)
            {
                try
                {
                    // 检查是否已注册过相同监听器
                    if (_registeredListeners.TryGetValue(eventId, out var listeners))
                    {
                        if (listeners.Contains(listener))
                        {
                            Logger.Log($"[{Name}] 事件监听器已存在，跳过重复注册: {eventId}");
                            return;
                        }
                    }
                    else
                    {
                        _registeredListeners[eventId] = new List<Action<GameEventArgs>>();
                    }

                    if (!IsLoadDone)
                    {
                        var queuedEvent = new QueuedEventListener(listener, eventId);
                        _eventMessageQueue.Enqueue(queuedEvent);
                        Logger.Log($"[{Name}] UI未加载完成，事件监听器已排队: {eventId}");
                    }

                    // 创建包装监听器
                    Action<GameEventArgs> wrappedListener = (msg) =>
                    {
                        if (!IsLoadDone)
                        {
                            // UI未加载完成时，缓存接收到的事件（使用快照避免引用参数在加载过程中被修改）
                            lock (_eventLock)
                            {
                                if (!_cachedEventMessages.TryGetValue(msg.Id, out var messageList))
                                {
                                    messageList = new List<GameEventArgs>();
                                    _cachedEventMessages[msg.Id] = messageList;
                                }

                                messageList.Add(msg.CreateSnapshot());
                            }

                            Logger.Log($"[{Name}] UI未加载完成，缓存外部事件: {msg.Id}");
                        }
                        else
                        {
                            try
                            {
                                listener(msg);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"[{Name}] 事件处理异常 {msg.Id}: {ex.Message}");
                            }
                        }
                    };

                    // 保存包装监听器的引用映射
                    _wrappedListeners[listener] = wrappedListener;

                    LogicEventDispatcher.Instance.AddListener(eventId, wrappedListener);
                    _registeredListeners[eventId].Add(listener);

                    Logger.Log($"[{Name}] 成功订阅事件: {eventId}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[{Name}] 事件订阅失败 {eventId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="eventId">事件ID。</param>
        /// <param name="listener">事件监听回调。</param>
        protected void RemoveListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null)
            {
                Logger.Warning($"[{Name}] 取消订阅事件失败: 事件ID或监听器为空！事件ID: {eventId}");
                return;
            }

            lock (_eventLock)
            {
                try
                {
                    // 从本地跟踪中移除
                    if (_registeredListeners.TryGetValue(eventId, out var listeners))
                    {
                        listeners.Remove(listener);
                        if (listeners.Count == 0)
                        {
                            _registeredListeners.Remove(eventId);
                        }
                    }

                    // 从LogicEventDispatcher中移除监听器
                    if (_wrappedListeners.TryGetValue(listener, out var wrappedListener))
                    {
                        LogicEventDispatcher.Instance.RemoveListener(eventId, wrappedListener);
                        _wrappedListeners.Remove(listener);
                    }
                    else
                    {
                        // 如果没有包装监听器的记录，尝试直接移除原始监听器
                        LogicEventDispatcher.Instance.RemoveListener(eventId, listener);
                    }

                    Logger.Log($"[{Name}] 成功取消订阅事件: {eventId}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[{Name}] 取消订阅事件失败 {eventId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 发送事件（便捷方法）。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="args">事件参数。</param>
        protected void SendEvent(object sender, GameEventArgs args)
        {
            if (args == null)
            {
                Logger.Warning($"[{Name}] 发送事件失败: 事件参数为空！");
                return;
            }

            try
            {
                LogicEventDispatcher.Instance.Send(args);
                Logger.Log($"[{Name}] 成功发送事件: {args.Id}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[{Name}] 发送事件失败 {args.Id}: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除所有UI事件。
        /// </summary>
        private void RemoveAllUIEvent()
        {
            lock (_eventLock)
            {
                try
                {
                    // 移除所有注册的事件监听器
                    foreach (var kvp in _registeredListeners)
                    {
                        string eventId = kvp.Key;
                        foreach (var listener in kvp.Value)
                        {
                            try
                            {
                                if (_wrappedListeners.TryGetValue(listener, out var wrappedListener))
                                {
                                    LogicEventDispatcher.Instance.RemoveListener(eventId, wrappedListener);
                                    _wrappedListeners.Remove(listener);
                                }
                                else
                                {
                                    LogicEventDispatcher.Instance.RemoveListener(eventId, listener);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"[{Name}] 移除事件监听器失败 {eventId}: {ex.Message}");
                            }
                        }
                    }

                    // 清空所有事件相关的数据结构
                    _registeredListeners.Clear();
                    _wrappedListeners.Clear();
                    _cachedEventMessages.Clear();
                    _eventMessageQueue.Clear();

                    Logger.Log($"[{Name}] 成功移除所有UI事件");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[{Name}] 移除所有UI事件失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 清空事件队列
        /// </summary>
        private void ClearEventQueue()
        {
            lock (_eventLock)
            {
                // 清空队列中的所有事件监听器
                while (_eventMessageQueue.Count > 0)
                {
                    _eventMessageQueue.Dequeue();
                }

                Logger.Log($"[{Name}] 事件队列已清空");
            }
        }

        /// <summary>
        /// 执行队列中的事件监听器
        /// </summary>
        public void ExecuteEvents()
        {
            lock (_eventLock)
            {
                if (_eventMessageQueue.Count == 0) return;

                Logger.Log($"[{Name}] 执行队列中的事件监听器: {_eventMessageQueue.Count}");

                var processedListeners = new List<QueuedEventListener>();

                // 1. 处理所有排队的监听器
                while (_eventMessageQueue.Count > 0)
                {
                    var queuedListener = _eventMessageQueue.Dequeue();
                    if (queuedListener.Listener != null)
                    {
                        processedListeners.Add(queuedListener);
                    }
                }

                // 2. 派发缓存的事件给处理过的监听器（同一 EventId 可能有多个监听器，需全部派发后再清理缓存）
                foreach (var listenerInfo in processedListeners)
                {
                    if (_cachedEventMessages.TryGetValue(listenerInfo.EventId, out var cachedMessages))
                    {
                        foreach (var cachedMessage in cachedMessages)
                        {
                            try
                            {
                                Logger.Log($"[{Name}] 重新派发缓存事件: {cachedMessage.Id} -> {listenerInfo.EventId}");
                                listenerInfo.Listener.Invoke(cachedMessage);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"[{Name}] 重新派发事件失败: {listenerInfo.EventId}, 错误: {ex.Message}");
                            }
                        }
                    }
                }

                // 3. 清理已处理事件ID的缓存（收集所有涉及的 EventId 后再统一清理）
                var processedEventIds = new HashSet<string>();
                foreach (var listenerInfo in processedListeners)
                {
                    processedEventIds.Add(listenerInfo.EventId);
                }

                foreach (var eventId in processedEventIds)
                {
                    _cachedEventMessages.Remove(eventId);
                }
            }
        }

        #endregion

        /// <summary>
        /// 加载UI资源（同步加载）
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        public void LoadUIView(string viewName, Action<UIViewLogic, GameObject> prepareCallback, object[] args)
        {
            InternalLoad(viewName, prepareCallback, args);
        }

        /// <summary>
        /// 异步加载UI资源
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        public void LoadUIViewAsync(string viewName, Action<UIViewLogic, GameObject> prepareCallback, object[] args)
        {
            InternalLoadAsync(viewName, prepareCallback, args).Forget();
        }

        /// <summary>
        /// 协程加载UI资源
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        public void LoadUIViewCoroutine(string viewName, Action<UIViewLogic, GameObject> prepareCallback, object[] args)
        {
            UIRoot.Instance.StartCoroutine(InternalLoadIE(viewName, prepareCallback, args));
        }

        /// <summary>
        /// 加载UI资源-同步方法
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        public void InternalLoad(string viewName, Action<UIViewLogic, GameObject> prepareCallback,
            object[] args)
        {
            // 参数验证
            if (string.IsNullOrEmpty(viewName))
            {
                Logger.Error($"[{Name}] 同步加载UI资源失败: 视图名称不能为空");
                return;
            }

            Name = viewName;
            _prepareCallback = prepareCallback;
            this.userDatas = args;

            GameObject gameObject = Resources.Load<GameObject>(viewName);

            if (gameObject == null)
            {
                Logger.Error($"[{Name}] 同步加载UI资源失败: 未找到资源 '{viewName}'");
                Handle_Completed(null);
                return;
            }

            Handle_Completed(gameObject);
        }

        /// <summary>
        /// 加载UI资源-异步方法
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        public async UniTaskVoid InternalLoadAsync(string viewName, Action<UIViewLogic, GameObject> prepareCallback,
            object[] args)
        {
            // 参数验证
            if (string.IsNullOrEmpty(viewName))
            {
                Logger.Error($"[{Name}] 异步加载UI资源失败: 视图名称不能为空");
                return;
            }

            Name = viewName;
            _prepareCallback = prepareCallback;
            this.userDatas = args;

            try
            {
                _loadCts?.Cancel();
                _loadCts?.Dispose();
                _loadCts = new CancellationTokenSource();

                ResourceRequest request = Resources.LoadAsync<GameObject>(viewName);

                // 等待资源加载完成
                await request.ToUniTask(cancellationToken: _loadCts.Token);

                GameObject gameObject = request.asset as GameObject;

                if (gameObject == null)
                {
                    Logger.Error($"[{Name}] 异步加载UI资源失败: 未找到资源 '{viewName}'");
                    Handle_Completed(null);
                    return;
                }

                Handle_Completed(gameObject);
            }
            catch (OperationCanceledException)
            {
                // Destroy/切场景导致取消加载：不视为错误
                Handle_Completed(null);
            }
            catch (System.Exception ex)
            {
                Logger.Error($"[{Name}] 异步加载UI资源时发生异常: {ex}");
                Handle_Completed(null);
            }
        }

        /// <summary>
        /// 加载UI资源-协程
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="prepareCallback">准备完成回调</param>
        /// <param name="args">用户数据参数</param>
        /// <returns></returns>
        public IEnumerator InternalLoadIE(string viewName, Action<UIViewLogic, GameObject> prepareCallback,
            object[] args)
        {
            // 参数验证
            if (string.IsNullOrEmpty(viewName))
            {
                Logger.Error($"[{Name}] 协程加载UI资源失败: 视图名称不能为空");
                yield break;
            }

            Name = viewName;
            _prepareCallback = prepareCallback;
            this.userDatas = args;

            ResourceRequest request = Resources.LoadAsync<GameObject>(viewName);

            // 等待资源加载完成
            while (!request.isDone)
            {
                yield return null;
            }

            GameObject gameObject = request.asset as GameObject;

            if (gameObject == null)
            {
                Logger.Error($"[{Name}] 协程加载UI资源失败: 未找到资源 '{viewName}'");
                Handle_Completed(null);
                yield break;
            }

            Handle_Completed(gameObject);
        }

        private void Handle_Completed(GameObject panel)
        {
            if (panel == null)
            {
                // 资源加载失败，但仍然设置加载完成标志，以便后续逻辑能正确处理错误状态
                IsLoadDone = true;
                Logger.Warning($"[{Name}] UI资源加载失败，无法创建界面");

                // 即使面板为空也调用回调，允许上层逻辑处理错误情况
                _prepareCallback?.Invoke(this, null);
                return;
            }

            IsLoadDone = true;
            if (IsDestroyed)
            {
                Logger.Warning($"[{Name}] UI在加载完成前已被销毁");
                return;
            }

            _prepareCallback?.Invoke(this, panel);
        }
    }

    /// <summary>
    /// 泛型UI逻辑基类。
    /// 支持直接访问具体的View类型。
    /// </summary>
    /// <typeparam name="T">具体的UIView类型。</typeparam>
    public abstract class UIViewLogic<T> : UIViewLogic where T : UIView
    {
        /// <summary>
        /// 获取具体类型的View实例。
        /// </summary>
        protected T View => _View as T;
    }
}