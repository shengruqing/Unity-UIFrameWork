using System;
using System.Collections.Generic;
using Sun.Runtime.UniEvent.Runtime;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 逻辑事件分发器。
    /// 单例模式，负责全局事件的分发和管理。
    /// </summary>
    public class LogicEventDispatcher : IEventDispatcher
    {
        private static LogicEventDispatcher _instance;
        private readonly Dictionary<string, List<Action<GameEventArgs>>> _listeners = new();

        /// <summary>
        /// 获取事件分发器单例实例
        /// </summary>
        public static LogicEventDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogicEventDispatcher();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 添加事件监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="listener">监听器回调</param>
        public void AddListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null)
                return;

            if (!_listeners.TryGetValue(eventId, out var listeners))
            {
                listeners = new List<Action<GameEventArgs>>();
                _listeners[eventId] = listeners;
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// 移除事件监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="listener">要移除的监听器回调</param>
        public void RemoveListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null)
                return;

            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                listeners.Remove(listener);

                if (listeners.Count == 0)
                {
                    _listeners.Remove(eventId);
                }
            }
        }

        /// <summary>
        /// 发送事件，通知所有注册的监听器
        /// </summary>
        /// <param name="args">事件参数</param>
        public void Send(GameEventArgs args)
        {
            if (args == null)
                return;

            if (!_listeners.TryGetValue(args.Id, out var listeners))
                return;

            if (listeners.Count == 0)
                return;

            // 创建监听器副本以避免在迭代过程中修改集合
            var listenersCopy = new List<Action<GameEventArgs>>(listeners);

            foreach (var listener in listenersCopy)
            {
                try
                {
                    listener?.Invoke(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"事件处理异常 [{args.Id}]: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// 发送事件并确保目标视图已打开
        /// 如果视图未显示或未加载，则先显示视图再发送事件
        /// </summary>
        /// <typeparam name="T">目标视图类型</typeparam>
        /// <param name="args">事件参数</param>
        public void SendToOpenViewWithArgs<T>(GameEventArgs args) where T : UIView
        {
            if (args == null)
                return;

            if (!GUIManager.Instance.IsViewVisible<T>() && !GUIManager.Instance.IsLoadingCache<T>())
            {
                GUIManager.Instance.ShowView<T>();
            }

            Send(args);
        }

        /// <summary>
        /// 发送事件并确保目标视图已打开
        /// 如果视图未显示或未加载，则先显示视图再发送事件
        /// </summary>
        /// <param name="viewName">界面名称</param>
        /// <param name="args">事件参数</param>
        public void SendToOpenViewWithArgs(string viewName, GameEventArgs args)
        {
            if (args == null)
                return;

            if (!GUIManager.Instance.IsViewVisible(viewName) && !GUIManager.Instance.IsLoadingCache(viewName))
            {
                GUIManager.Instance.ShowView(viewName);
            }

            Send(args);
        }

        /// <summary>
        /// 移除指定事件的所有监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        public void RemoveAllListeners(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;

            _listeners.Remove(eventId);
        }

        /// <summary>
        /// 清空所有事件的监听器
        /// 通常在场景切换或系统重置时调用
        /// </summary>
        public void ClearAllListeners()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// 获取指定事件的监听器数量
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>监听器数量，如果事件不存在返回0</returns>
        public int GetListenerCount(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return 0;

            if (_listeners.TryGetValue(eventId, out var listeners))
            {
                return listeners.Count;
            }

            return 0;
        }

        /// <summary>
        /// 检查是否存在指定事件的监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>是否存在监听器</returns>
        public bool HasListeners(string eventId)
        {
            return GetListenerCount(eventId) > 0;
        }

        /// <summary>
        /// 销毁事件分发器，清空所有资源
        /// </summary>
        public void Dispose()
        {
            ClearAllListeners();
            _instance = null;
        }
    }
}