using UnityEngine.UI;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 自动生成脚本请勿修改！！！！！如果需要新增组件获取，请在Test2View.cs中添加
    /// </summary>
    public partial class Test2View : UIView
    {
    
        public Button btn_close;

        
        protected override void AutoInit()
        {
            base.AutoInit();
            btn_close = GetChildCompByObj<Button>("btn_close");

        }
    }
}