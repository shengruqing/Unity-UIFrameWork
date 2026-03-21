using UnityEngine.Scripting;

namespace Sun.Runtime.UniEvent.Runtime
{
    /// <summary>
    /// 事件消息接口。
    /// 所有事件消息类都应实现此接口。
    /// </summary>
    [Preserve]
    public interface IEventMessage
    {
        /// <summary>
        /// 获取事件ID。
        /// </summary>
        string Id { get; }
    }
}