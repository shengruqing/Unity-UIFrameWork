# UIManager 快速参考

## 核心类速查

### 1. GUIManager (单例)
```csharp
// 显示界面
GUIManager.Instance.ShowView<LoginLogic>("参数");

// 隐藏界面  
GUIManager.Instance.HideView<LoginLogic>();

// 获取界面
LoginLogic logic = GUIManager.Instance.GetView<LoginLogic>();

// 判断可见性
bool visible = GUIManager.Instance.IsViewVisible<LoginLogic>();

// 销毁所有界面
GUIManager.Instance.DestroyAllView();
```

### 2. 创建自定义界面

#### View类 (UI显示)
```csharp
public class MyView : UIView
{
    public Button btnAction;
    public Text txtInfo;
    
    protected override void OnInit()
    {
        btnAction = Find<Button>("BtnAction");
        txtInfo = Find<Text>("TxtInfo");
    }
}
```

#### Logic类 (业务逻辑)
```csharp
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly, needUpdate: false)]
public class MyLogic : UIViewLogic<MyView>
{
    protected override void OnShow()
    {
        View.txtInfo.text = "界面已显示";
    }
    
    public override void RegisterEvent()
    {
        AddListener("MY_EVENT", OnMyEvent);
    }
    
    private void OnMyEvent(GameEventArgs args)
    {
        // 处理事件
    }
}
```

### 3. 事件系统

#### 发送事件
```csharp
public class MyEventArgs : GameEventArgs
{
    public string Message { get; set; }
    
    public MyEventArgs(string message)
    {
        Id = "MY_EVENT";
        Message = message;
    }
}

// 发送
SendEvent(this, new MyEventArgs("Hello"));
```

#### 接收事件
```csharp
// 在Logic中订阅
AddListener("MY_EVENT", OnMyEvent);

private void OnMyEvent(GameEventArgs args)
{
    var myArgs = args as MyEventArgs;
    Debug.Log($"收到事件: {myArgs.Message}");
}

// 自动在Hide时取消订阅
```

### 4. 界面属性配置

#### UIAttribute 参数
```csharp
// 基本配置
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly)]

// 完整配置
[UIAttribute(
    layer: ViewLayer.Pop,           // 层级
    curStackMode: ViewStack.OverLay, // 堆栈模式
    needUpdate: true,               // 是否需要每帧更新
    isNoNotch: false,               // 是否忽略刘海屏
    isUseAdjust: true               // 是否使用双边刘海适配
)]
```

#### 层级定义 (ViewLayer)
- `Bottom`(0), `Default`(1) - 底层界面
- `Layer1`(10)-`Layer4`(40) - 中间层级
- `Pop`(50), `Pop2`(55) - 弹窗层
- `Top`(60), `Top2`(65) - 顶层
- `Tip`(70) - 提示层
- `Loading`(90) - 加载层

#### 堆栈模式 (ViewStack)
- `FullOnly` - 全屏模式，隐藏其他界面
- `OverMain` - 覆盖主界面模式
- `OverLay` - 叠加模式，一直向上叠加

### 5. 组件查找工具

#### 查找组件
```csharp
// 基本查找
Button btn = Find<Button>("BtnOK");

// 路径查找
Text scoreText = GetChildCompByObj<Text>("HUD/ScorePanel/Text");

// 获取子对象
GameObject panel = GetChildObj("MainPanel");

// 克隆模板
GameObject item = CloneTemplate(template, resetLocalTransform: true);
```

### 6. 坐标转换 (UIRoot)
```csharp
// 屏幕→UI坐标
Vector2 uiPos = UIRoot.Instance.Screen2Canvas(Input.mousePosition);

// UI→屏幕坐标
Vector2 screenPos = UIRoot.Instance.Canvas2Screen(new Vector2(100, 50));

// 3D世界→UI坐标
Vector2 uiPos2 = UIRoot.Instance.World2Canvas(enemy.position);
```

### 7. UIGroup 组件组
```csharp
public class ItemGroup : UIGroup
{
    public Image icon;
    public Text nameText;
    
    protected override void OnInit()
    {
        icon = GetChildCompByObj<Image>("Icon");
        nameText = GetChildCompByObj<Text>("Name");
    }
    
    public void SetData(ItemData data)
    {
        icon.sprite = data.icon;
        nameText.text = data.name;
    }
}

// 使用
ItemGroup group = new ItemGroup();
group.InitContainer(itemObj);
group.Show();
group.SetData(itemData);
```

### 8. 异步加载

#### 协程加载 (默认)
```csharp
// 在UIViewLogic中自动处理
LoadUIView("MyView", callback, args);
```

#### 自定义加载策略
```csharp
public void LoadUIView(string viewName, Action<UIViewLogic, GameObject> callback, object[] args)
{
    // 使用Addressables
    // 或使用AssetBundle
    // 或使用Resources.LoadAsync
}
```

### 9. 性能优化提示

#### ✅ 应该做的
- 使用对象池复用频繁显示的界面
- 预加载常用界面资源
- 及时取消事件订阅
- 合并UI图集减少Draw Calls
- 使用异步加载避免卡顿

#### ❌ 不应该做的
- 不要在View中写业务逻辑
- 不要在Logic中直接操作UI组件
- 不要忘记在Destroy时清理资源
- 不要创建过多Canvas层级
- 不要阻塞主线程加载UI

### 10. 调试技巧

#### 查看界面状态
```csharp
// 添加调试代码
void DebugViews()
{
    Debug.Log($"当前界面数量: {GUIManager.Instance.GetViewCount()}");
    Debug.Log($"缓存状态: {TypeCacheManager.GetCacheStats()}");
}
```

#### 事件监控
```csharp
// 监听所有事件
LogicEventDispatcher.Instance.OnEventSent += (id, args) => 
{
    Debug.Log($"事件发送: {id}");
};
```

### 11. 常见问题解决

#### Q: 界面不显示
- ✅ 检查UIRoot是否在场景中
- ✅ 检查预制体路径是否正确
- ✅ 检查Canvas组件是否存在

#### Q: 事件不触发
- ✅ 检查事件ID是否匹配
- ✅ 检查订阅时机是否在发送之前
- ✅ 检查是否在Hide时被取消订阅

#### Q: 内存泄漏
- ✅ 确保Destroy时清理所有事件订阅
- ✅ 使用WeakReference避免循环引用
- ✅ 定期调用DestroyAllView清理

### 12. 扩展开发

#### 自定义动画
```csharp
public interface IUIAnimation
{
    UniTask PlayShowAnimation();
    UniTask PlayHideAnimation();
}

public class FadeAnimation : IUIAnimation
{
    private CanvasGroup canvasGroup;
    
    public async UniTask PlayShowAnimation()
    {
        canvasGroup.alpha = 0;
        await DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.3f);
    }
}
```

#### 国际化支持
```csharp
public class LocalizedView : UIView
{
    protected override void OnInit()
    {
        var texts = GetComponentsInChildren<Text>(true);
        foreach (var text in texts)
        {
            if (text.text.StartsWith("#"))
            {
                text.text = LocalizationManager.Get(text.text.Substring(1));
            }
        }
    }
}
```

---

## 📞 紧急帮助

### 快速排错流程
1. **检查日志** - 系统有详细日志输出
2. **验证场景** - UIRoot是否存在
3. **检查资源** - 预制体路径是否正确
4. **调试事件** - 使用事件监控工具
