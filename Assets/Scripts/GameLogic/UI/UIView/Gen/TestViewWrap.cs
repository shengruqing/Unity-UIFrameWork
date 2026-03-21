using UnityEngine.UI;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 自动生成脚本请勿修改！！！！！如果需要新增组件获取，请在TestView.cs中添加
    /// </summary>
    public partial class TestView : UIView
    {
    
        public Button btn_send;

        
        protected override void AutoInit()
        {
            base.AutoInit();
            btn_send = GetChildCompByObj<Button>("btn_send");

        }
    }
}