using System;
using GameLogic;
using UnityEngine.Scripting;

namespace Sun.Runtime.UniEvent.Runtime
{
    /// <summary>
    /// 事件分发器接口。
    /// 提供全局事件分发功能。
    /// </summary>
    [Preserve]
    public interface IEventDispatcher
    {
        /// <summary>
        /// 添加事件监听器。
        /// </summary>
        /// <param name="eventId">事件ID。</param>
        /// <param name="listener">事件监听回调。</param>
        void AddListener(string eventId, Action<GameEventArgs> listener);

        /// <summary>
        /// 移除事件监听器。
        /// </summary>
        /// <param name="eventId">事件ID。</param>
        /// <param name="listener">事件监听回调。</param>
        void RemoveListener(string eventId, Action<GameEventArgs> listener);

        /// <summary>
        /// 发送事件消息。
        /// </summary>
        /// <param name="args">事件参数。</param>
        void Send(GameEventArgs args);

    }
}