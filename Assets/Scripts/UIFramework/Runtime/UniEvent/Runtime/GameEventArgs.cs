using GameLogic;
using UnityEngine.Scripting;

namespace Sun.Runtime.UniEvent.Runtime
{
    /// <summary>
    /// 游戏事件参数基类。
    /// 提供事件的基本信息和数据传递功能。
    /// </summary>
    [Preserve]
    public abstract class GameEventArgs : IEventMessage
    {
        /// <summary>
        /// 事件ID。
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// 创建事件参数的快照副本。
        /// 用于在 UI 异步加载时缓存事件，避免发送方在加载过程中修改引用对象导致 UI 收到过期数据。
        /// 子类若包含可能被外部修改的引用类型字段，应重写此方法并返回数据副本；否则默认返回自身。
        /// </summary>
        /// <returns>事件参数的快照副本，执行时使用此副本可保证数据为缓存时刻的状态。</returns>
        public virtual GameEventArgs CreateSnapshot()
        {
            return this;
        }
    }

}