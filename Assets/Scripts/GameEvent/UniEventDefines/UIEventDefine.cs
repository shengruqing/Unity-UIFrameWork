using Sun.Runtime.UniEvent.Runtime;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 更新UI
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class UpdateUI : GameEventArgs
    {
        public string str;

        public static readonly string EventId = typeof(UpdateUI).FullName;
        public override string Id => EventId;

        public override GameEventArgs CreateSnapshot()
        {
            return new UpdateUI { str = this.str };
        }

        public static void SendEventMessage(string _str)
        {
            var msg = new UpdateUI
            {
                str = _str
            };
            LogicEventDispatcher.Instance.Send(msg);
        }
    }


    [UnityEngine.Scripting.Preserve]
    public class DropBomb : GameEventArgs
    {
        public int count;

        public DropBomb(int count)
        {
            this.count = count;
        }

        public static readonly string EventId = typeof(DropBomb).FullName;
        public override string Id => EventId;

        public override GameEventArgs CreateSnapshot()
        {
            return new DropBomb(this.count);
        }
    }
}