using Sun.Runtime.UniEvent.Runtime;
using UnityEngine;

namespace GameLogic
{
    [UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly)]
    public class Test2ViewLogic : UIViewLogic<Test2View>
    {
        protected override void OnInit()
        {
            base.OnInit();
            View.btn_close.onClick.AddListener(OnClose);

        }

        protected override void OnShow()
        {
            base.OnShow();
        }
        

        private void OnClose()
        {
            Hide();
        }

        public override void RegisterEvent()
        {
            base.RegisterEvent();
            AddListener(UpdateUI.EventId, OnUpdateUI);
        }

        private void OnUpdateUI(GameEventArgs obj)
        {
            var args = (UpdateUI)obj;
            Logger.Log(args.str);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}