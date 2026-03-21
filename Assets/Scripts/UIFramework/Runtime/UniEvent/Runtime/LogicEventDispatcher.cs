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
        private readonly object _lock = new object();

        /// <summary>
        /// 获取事件分发器单例实例。
        /// </summary>
        public static LogicEventDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(LogicEventDispatcher))
                    {
                        if (_instance == null)
                            _instance = new LogicEventDispatcher();
                    }
                }

                return _instance;
            }
        }

        public void AddListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null) return;

            lock (_lock)
            {
                if (!_listeners.ContainsKey(eventId))
                {
                    _listeners[eventId] = new List<Action<GameEventArgs>>();
                }

                if (!_listeners[eventId].Contains(listener))
                {
                    _listeners[eventId].Add(listener);
                }
            }
        }

        public void RemoveListener(string eventId, Action<GameEventArgs> listener)
        {
            if (string.IsNullOrEmpty(eventId) || listener == null) return;

            lock (_lock)
            {
                if (_listeners.ContainsKey(eventId))
                {
                    _listeners[eventId].Remove(listener);
                    if (_listeners[eventId].Count == 0)
                    {
                        _listeners.Remove(eventId);
                    }
                }
            }
        }

        public void Send(GameEventArgs args)
        {
            if (args == null) return;

            lock (_lock)
            {
                if (_listeners.TryGetValue(args.Id, out var listeners))
                {
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
                            Debug.LogError($"事件处理异常 [{args.Id}]: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}