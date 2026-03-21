using UnityEngine;

namespace GameLogic
{
    [UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly, false, true, true)]
    public class TestViewLogic : UIViewLogic<TestView>
    {
        private string value = "test1";

        protected override void OnInit()
        {
            base.OnInit();
            View.btn_send.onClick.AddListener(OnSend);
        }

        protected override void OnShow()
        {
            base.OnShow();
        }



        private void OnSend()
        {
            GUIManager.Instance.ShowView<Test2ViewLogic>();
            LogicEventDispatcher.Instance.Send(new UpdateUI() { str = value });
        }

        public override void RegisterEvent()
        {
            base.RegisterEvent();
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