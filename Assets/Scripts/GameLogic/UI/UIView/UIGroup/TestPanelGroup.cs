using UnityEngine.UI;

namespace GameLogic
{
    public partial class TestPanel : UIGroup
    {
        public Button btn_close;
        public Button btn_send;
        public Button btn_remove;

        protected override void OnInit()
        {
            base.OnInit();
            btn_close = GetChildCompByObj<Button>("TestPanel/btn_close");
            btn_close = GetChildCompByObj<Button>("btn_close");
            btn_send = GetChildCompByObj<Button>("TestPanel/btn_send");
            btn_remove = GetChildCompByObj<Button>("TestPanel/btn_remove");
            btn_send = GetChildCompByObj<Button>("btn_send");
            btn_remove = GetChildCompByObj<Button>("btn_remove");

            btn_close.onClick.AddListener(OnClose);
            btn_send.onClick.AddListener(OnSend);
            btn_remove.onClick.AddListener(OnRemove);
        }

        /// <summary>
        /// 当显示时回调。
        /// </summary>
        protected override void OnShow()
        {
        }

        private void OnClose()
        {
        }

        private void OnSend()
        {
        }

        private void OnRemove()
        {
        }


        /// <summary>
        /// 当隐藏时回调。
        /// </summary>
        protected override void OnHide()
        {
        }

        /// <summary>
        /// 当删除时调用。
        /// </summary>
        protected override void OnDestroy()
        {
        }
    }
}