# UIManager 架构指南

## 概述

UIManager 是一个基于 MVC 模式的 Unity UI 管理系统，提供界面创建、显示、隐藏、销毁和生命周期管理的完整解决方案。系统采用 View-Controller 分离架构，支持界面堆栈管理、事件分发、异步加载等核心功能。

## 🚀 快速入门

### 5分钟创建第一个UI界面

#### 步骤1: 创建UI预制体
1. 在Unity中创建Canvas
2. 添加按钮、文本等UI组件
3. 保存为预制体到 `Resources/` 目录，如 `Resources/QuickStartView`

#### 步骤2: 创建View脚本
```csharp
// QuickStartView.cs
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class QuickStartView : UIView
    {
        public Button btnStart;
        public Text txtMessage;
        
        protected override void OnInit()
        {
            btnStart = Find<Button>("BtnStart");
            txtMessage = Find<Text>("TxtMessage");
        }
    }
}
```

#### 步骤3: 创建Logic脚本
```csharp
// QuickStartLogic.cs
using UnityEngine;

namespace GameLogic
{
    [UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly)]
    public class QuickStartLogic : UIViewLogic<QuickStartView>
    {
        protected override void OnShow()
        {
            Debug.Log("快速开始界面显示");
            View.txtMessage.text = "欢迎使用UIManager!";
        }
        
        public override void RegisterEvent()
        {
            // 可以在这里添加事件监听
        }
    }
}
```

#### 步骤4: 显示界面
```csharp
// 在任何地方调用
GUIManager.Instance.ShowView<QuickStartLogic>();
```

#### 步骤5: 隐藏界面
```csharp
// 隐藏界面
GUIManager.Instance.HideView<QuickStartLogic>();

// 或者直接调用Logic的Hide方法
var logic = GUIManager.Instance.GetView<QuickStartLogic>();
if (logic != null) logic.Hide();
```

✅ **恭喜！你已经成功创建了第一个UIManager界面！**

## 📖 详细指南

## 📑 目录
- [核心架构](#核心架构)
- [使用方法](#使用方法)
- [高级特性](#高级特性)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)
- [调试技巧](#调试技巧)
- [扩展开发](#扩展开发)
- [API参考](#api参考)

## 核心架构

### 1. 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      GUIManager (Controller)                │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐    │
│  │ ViewList │ │ UIStack  │ │ OrderStack│ │ LoadingCache│    │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘    │
└─────────────┬───────────────────────────────────────────────┘
              │ 管理所有UI界面生命周期
┌─────────────▼───────────────────────────────────────────────┐
│                  UIViewLogic (Controller)                   │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐    │
│  │ 业务逻辑  │ │ 事件处理 │ │ 生命周期  │ │ View绑定      │    │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘    │
└─────────────┬───────────────────────────────────────────────┘
              │ 绑定具体View对象
┌─────────────▼───────────────────────────────────────────────┐
│                      UIView (View)                          │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐    │
│  │ 组件获取  │ │ UI显示   │ │ 交互控制 │ │ 适配调整       │    │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘    │
└─────────────┬───────────────────────────────────────────────┘
              │ 依赖基础组件
┌─────────────▼───────────────────────────────────────────────┐
│               ControlContainer/UIRoot/UIGroup               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐    │
│  │ 组件管理  │ │ 层级管理 │ │ 坐标转换  │ │ 组管理工具    │    │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 2. 核心组件

#### GUIManager - 全局管理器
- **职责**: 管理所有UI界面的生命周期、堆栈和显示顺序
- **特性**: 线程安全的单例模式，支持同步/异步调用
- **关键数据结构**:
  - `viewList`: 存储所有已创建的UIViewLogic实例
  - `uiStack`: 界面堆栈，记录界面打开顺序
  - `orderViewStack`: 顺序堆栈，用于ESC键返回处理
  - `_loadingCache`: 缓存正在加载的界面
  - `viewObjects`: 存储UIView对象实例

#### UIViewLogic - UI逻辑基类
- **职责**: 处理UI的业务逻辑、生命周期和事件管理
- **特性**: 支持事件订阅、异步加载、生命周期回调
- **核心方法**:
  - `Show()`: 显示界面
  - `Hide()`: 隐藏界面
  - `Destroy()`: 销毁界面
  - `Update()`: 每帧更新
  - `RegisterEvent()`: 注册事件监听

#### UIView - UI视图基类
- **职责**: 处理UI组件的获取、显示和交互控制
- **特性**: 自动组件查找、刘海屏适配、层级管理
- **核心方法**:
  - `InitContainer()`: 初始化View容器
  - `AppLayer()`: 应用层级设置
  - `AdjustResolution()`: 分辨率适配
  - `SetInteractable()`: 设置交互性

#### ControlContainer - 控件容器基类
- **职责**: 提供UI组件的基础操作和事件管理功能
- **特性**: 组件查找、克隆操作、Transform管理
- **核心方法**:
  - `GetChildObj()`: 获取子对象
  - `GetChildCompByObj()`: 获取子组件
  - `CloneTemplate()`: 克隆模板对象
  - `Transform_SetParent()`: 设置父节点

#### UIRoot - UI根节点管理器
- **职责**: 管理UI根节点、UI相机、Canvas等基础组件
- **特性**: 单例模式、坐标转换、分辨率适配
- **核心方法**:
  - `Screen2Canvas()`: 屏幕坐标转Canvas坐标
  - `Canvas2Screen()`: Canvas坐标转屏幕坐标
  - `World2Canvas()`: 世界坐标转Canvas坐标

#### UIGroup - 控件组管理
- **职责**: 管理UI组件组，提供组级别的显示/隐藏控制
- **特性**: 组生命周期管理、组件分组
- **核心方法**:
  - `Show()`: 显示组件组
  - `Hide()`: 隐藏组件组
  - `Destroy()`: 销毁组件组

#### TypeCacheManager - 类型缓存管理器
- **职责**: 提供高效的类型查找和缓存功能
- **特性**: 线程安全、自动缓存、性能优化
- **核心方法**:
  - `FindViewType()`: 查找View类型
  - `PreloadTypes()`: 预加载类型
  - `ClearCache()`: 清空缓存

#### MonoSingleton - MonoBehaviour单例基类
- **职责**: 提供MonoBehaviour的单例实现
- **特性**: 自动实例化、线程安全、生命周期管理
- **使用**: 继承此类创建需要挂载在GameObject上的单例

## 使用方法

### 1. 创建自定义UI界面

#### 步骤0: 了解组件查找机制
系统提供了强大的组件查找功能，通过`ControlContainer`基类实现：

```csharp
// 在View中查找组件
public class MyView : UIView
{
    public Button btnSubmit;
    public Text txtTitle;
    public Image imgIcon;
    
    protected override void OnInit()
    {
        // 方法1: 使用泛型方法查找
        btnSubmit = Find<Button>("BtnSubmit");
        txtTitle = Find<Text>("TitleText");
        imgIcon = Find<Image>("IconImage");
        
        // 方法2: 使用路径查找子对象
        GameObject scoreObj = GetChildObj("HUD/ScorePanel");
        
        // 方法3: 使用路径查找子组件
        Text scoreText = GetChildCompByObj<Text>("HUD/ScorePanel/ScoreText");
        
        // 克隆UI模板
        GameObject template = GetChildObj("ItemTemplate");
        for (int i = 0; i < 5; i++)
        {
            GameObject item = CloneTemplate(template, resetLocalTransform: true);
            item.name = $"Item_{i}";
        }
    }
}
```

#### 步骤1: 创建View类
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// 登录界面View
    /// </summary>
    public class LoginView : UIView
    {
        // UI组件声明
        public Button btnLogin;
        public InputField inputUsername;
        public InputField inputPassword;
        public Text txtError;
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        protected override void OnInit()
        {
            // 自动查找组件
            btnLogin = Find<Button>("BtnLogin");
            inputUsername = Find<InputField>("InputUsername");
            inputPassword = Find<InputField>("InputPassword");
            txtError = Find<Text>("TxtError");
            
            // 绑定事件
            btnLogin.onClick.AddListener(OnLoginClick);
        }
        
        private void OnLoginClick()
        {
            // View只处理UI交互，业务逻辑交给Logic处理
            // 通常通过事件或直接调用Logic方法
        }
    }
}
```

#### 步骤2: 创建Logic类
```csharp
using System;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 登录界面逻辑
    /// </summary>
    [UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly, needUpdate: false)]
    public class LoginLogic : UIViewLogic<LoginView>
    {
        private string username;
        private string password;
        
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnInit()
        {
            Debug.Log("LoginLogic初始化完成");
        }
        
        /// <summary>
        /// 显示时回调
        /// </summary>
        protected override void OnShow()
        {
            Debug.Log("登录界面显示");
            
            // 获取传递的参数
            if (userDatas != null && userDatas.Length > 0)
            {
                username = userDatas[0] as string;
                View.inputUsername.text = username;
            }
            
            // 注册事件
            RegisterEvent();
        }
        
        /// <summary>
        /// 注册事件监听
        /// </summary>
        public override void RegisterEvent()
        {
            // 订阅网络响应事件
            AddListener(EventDefine.LoginSuccess, OnLoginSuccess);
            AddListener(EventDefine.LoginFailed, OnLoginFailed);
        }
        
        /// <summary>
        /// 隐藏时回调
        /// </summary>
        protected override void OnHide()
        {
            Debug.Log("登录界面隐藏");
            username = null;
            password = null;
        }
        
        /// <summary>
        /// 返回键处理
        /// </summary>
        public override ViewEscape OnEscape()
        {
            // 如果正在登录，等待登录完成
            if (isLoggingIn)
                return ViewEscape.Wait;
            
            // 否则隐藏界面
            Hide();
            return ViewEscape.Hide;
        }
        
        /// <summary>
        /// 登录按钮点击（由View调用）
        /// </summary>
        public void OnLoginButtonClicked()
        {
            username = View.inputUsername.text;
            password = View.inputPassword.text;
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                View.txtError.text = "用户名和密码不能为空";
                return;
            }
            
            // 发送登录请求
            SendEvent(this, new LoginRequestArgs(username, password));
            View.txtError.text = "登录中...";
            isLoggingIn = true;
        }
        
        /// <summary>
        /// 登录成功回调
        /// </summary>
        private void OnLoginSuccess(GameEventArgs args)
        {
            isLoggingIn = false;
            var loginResult = args as LoginResultArgs;
            
            Debug.Log($"登录成功: {loginResult.UserName}");
            
            // 关闭登录界面
            Hide();
            
            // 打开主界面
            GUIManager.Instance.ShowView<MainLogic>();
        }
        
        /// <summary>
        /// 登录失败回调
        /// </summary>
        private void OnLoginFailed(GameEventArgs args)
        {
            isLoggingIn = false;
            var errorArgs = args as LoginErrorArgs;
            
            View.txtError.text = errorArgs.ErrorMessage;
            Debug.LogError($"登录失败: {errorArgs.ErrorMessage}");
        }
        
        private bool isLoggingIn = false;
    }
}
```

#### 步骤3: 在场景中创建UI预制体
1. 创建UI Canvas
2. 添加必要的UI组件
3. 保存为预制体，路径如: `Resources/LoginView`

### 2. 基础组件使用

#### UIGroup - 管理UI组件组
```csharp
// 创建自定义UI组
public class InventoryGroup : UIGroup
{
    public Button btnClose;
    public GridLayoutGroup itemGrid;
    public List<InventoryItem> items = new List<InventoryItem>();
    
    protected override void OnInit()
    {
        // 初始化组件
        btnClose = GetChildCompByObj<Button>("BtnClose");
        itemGrid = GetChildCompByObj<GridLayoutGroup>("ItemGrid");
        
        // 绑定事件
        btnClose.onClick.AddListener(OnCloseClick);
        
        // 初始化物品列表
        InitItems();
    }
    
    private void InitItems()
    {
        GameObject itemTemplate = GetChildObj("ItemTemplate");
        for (int i = 0; i < 10; i++)
        {
            GameObject itemObj = CloneTemplate(itemTemplate);
            var item = new InventoryItem();
            item.InitContainerByOwner(this, itemObj);
            items.Add(item);
        }
    }
    
    private void OnCloseClick()
    {
        Hide();
    }
    
    protected override void OnShow()
    {
        Debug.Log("背包组显示");
        // 刷新物品显示
        RefreshItems();
    }
    
    protected override void OnHide()
    {
        Debug.Log("背包组隐藏");
    }
    
    private void RefreshItems()
    {
        // 刷新物品数据
    }
}

// 在View中使用UIGroup
public class GameView : UIView
{
    public InventoryGroup inventoryGroup;
    
    protected override void OnInit()
    {
        // 初始化背包组
        GameObject groupObj = GetChildObj("InventoryPanel");
        inventoryGroup = new InventoryGroup();
        inventoryGroup.InitContainer(groupObj);
        
        // 显示/隐藏组
        inventoryGroup.Show();
        // inventoryGroup.Hide();
    }
}
```

#### 坐标转换 - UIRoot使用
```csharp
// 屏幕坐标转UI坐标
Vector2 screenPos = Input.mousePosition;
Vector2 canvasPos = UIRoot.Instance.Screen2Canvas(screenPos);

// UI坐标转屏幕坐标（用于世界空间UI）
Vector2 uiPos = new Vector2(100, 50);
Vector2 screenPos2 = UIRoot.Instance.Canvas2Screen(uiPos);

// 世界坐标转UI坐标（3D物体到UI的转换）
Vector3 worldPos = enemy.transform.position;
Vector2 uiPos2 = UIRoot.Instance.World2Canvas(worldPos);

// 在UI上显示3D物体位置
public class EnemyHUDView : UIView
{
    public RectTransform enemyMarker;
    public Transform enemyTarget;
    
    public void Update()
    {
        if (enemyTarget != null)
        {
            // 将3D世界坐标转换为UI坐标
            Vector2 uiPos = UIRoot.Instance.World2Canvas(enemyTarget.position);
            enemyMarker.anchoredPosition = uiPos;
        }
    }
}
```

### 3. 界面管理操作

#### 显示界面
```csharp
// 无参数显示
GUIManager.Instance.ShowView<LoginLogic>();

// 带参数显示
GUIManager.Instance.ShowView<LoginLogic>("defaultUser");

// 显示弹窗（叠加模式）
[UIAttribute(ViewLayer.Pop, ViewStack.OverLay)]
public class MessageBoxLogic : UIViewLogic<MessageBoxView>
{
    // ...
}
GUIManager.Instance.ShowView<MessageBoxLogic>("确认要退出吗？");
```

#### 隐藏界面
```csharp
// 隐藏指定界面
GUIManager.Instance.HideView<LoginLogic>();

// 判断界面是否可见
bool isVisible = GUIManager.Instance.IsViewVisible<LoginLogic>();
```

#### 获取界面实例
```csharp
LoginLogic loginLogic = GUIManager.Instance.GetView<LoginLogic>();
if (loginLogic != null)
{
    // 调用Logic方法
    loginLogic.UpdateUsername("newUser");
}
```

#### 销毁界面
```csharp
// 销毁单个界面
GUIManager.Instance.DestroyView<LoginLogic>();

// 销毁所有界面（切换场景时使用）
GUIManager.Instance.DestroyAllView();
```

### 3. 事件系统

#### 发送事件
```csharp
// 创建事件参数
public class UserLoginArgs : GameEventArgs
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    
    public UserLoginArgs(string userId, string userName)
    {
        Id = "USER_LOGIN";
        UserId = userId;
        UserName = userName;
    }
}

// 发送事件
var args = new UserLoginArgs("123", "玩家1");
SendEvent(this, args);
```

#### 订阅事件
```csharp
// 在Logic的RegisterEvent方法中订阅
public override void RegisterEvent()
{
    AddListener("USER_LOGIN", OnUserLogin);
    AddListener("ITEM_CHANGED", OnItemChanged);
}

private void OnUserLogin(GameEventArgs args)
{
    var loginArgs = args as UserLoginArgs;
    Debug.Log($"用户登录: {loginArgs.UserName}");
}

private void OnItemChanged(GameEventArgs args)
{
    // 处理物品变化
}
```

#### 取消订阅
```csharp
// 自动在Hide时取消所有订阅
// 也可以手动取消特定订阅
RemoveListener("USER_LOGIN", OnUserLogin);
```

## 高级特性

### 1. 界面层级管理

#### 层级定义
```csharp
public enum ViewLayer
{
    Bottom = 0,     // 底层界面
    Default = 1,    // 默认层
    Layer1 = 10,    // 普通界面
    Layer2 = 20,    // 重要界面
    Pop = 50,       // 弹窗层
    Tip = 70,       // 提示层
    Loading = 90,   // 加载层
    // ... 更多层级
}
```

#### 层级使用
```csharp
// 通过UIAttribute设置层级
[UIAttribute(ViewLayer.Pop, ViewStack.OverLay)]
public class MessageBoxLogic : UIViewLogic<MessageBoxView>

[UIAttribute(ViewLayer.Loading, ViewStack.OverLay)]
public class LoadingLogic : UIViewLogic<LoadingView>
```

### 2. 堆栈模式

#### FullOnly（全屏模式）
- 显示时会隐藏其他所有界面
- 适用于主界面、设置界面等
```csharp
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly)]
```

#### OverMain（覆盖主界面）
- 覆盖在主界面上方显示
- 适用于商店、背包等界面
```csharp
[UIAttribute(ViewLayer.Layer2, ViewStack.OverMain)]
```

#### OverLay（叠加模式）
- 一直向上叠加显示
- 适用于弹窗、提示等
```csharp
[UIAttribute(ViewLayer.Pop, ViewStack.OverLay)]
```

### 3. 刘海屏适配

#### 自动适配
```csharp
// 在View中自动处理刘海屏适配
public void AdjustResolution()
{
    var notchHeight = SDKManager.Instance.GetNotchHeight();
    if (_Logic != null && !_Logic.IsNoNotch && notchHeight > 0)
    {
        // 根据屏幕方向调整
        if (GUIManager.Instance.GetCurOrientation() == ScreenOrientation.LandscapeLeft)
        {
            ContentTrans.offsetMin = new Vector2(notchHeight, 0);
        }
        else
        {
            ContentTrans.offsetMax = new Vector2(-notchHeight, 0);
        }
    }
}
```

#### 禁用适配
```csharp
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly, isNoNotch: true)]
public class FullScreenLogic : UIViewLogic<FullScreenView>
```

### 4. 异步加载

#### 协程加载（默认）
```csharp
// UIViewLogic中的LoadUIView方法默认使用协程加载
UIRoot.Instance.StartCoroutine(InternalLoadIE(viewName, prepareCallback, args));
```

#### 异步任务加载
```csharp
// 如果需要使用UniTask异步加载
public void LoadUIView(string viewName, Action<UIViewLogic, GameObject> prepareCallback, object[] args)
{
    InternalLoadAsync(viewName, prepareCallback, args).Forget();
}
```

#### 同步加载
```csharp
// 同步加载（适用于预加载）
public void LoadUIView(string viewName, Action<UIViewLogic, GameObject> prepareCallback, object[] args)
{
    InternalLoad(viewName, prepareCallback, args);
}
```

### 5. 类型缓存管理

#### TypeCacheManager使用
```csharp
// 查找View类型（自动缓存）
Type viewType = TypeCacheManager.FindViewType("LoginView");
if (viewType != null)
{
    Debug.Log($"找到View类型: {viewType.FullName}");
}

// 预加载常用类型（游戏启动时调用）
void PreloadCommonViews()
{
    string[] commonViews = {
        "LoginView",
        "MainView", 
        "SettingView",
        "InventoryView"
    };
    
    TypeCacheManager.PreloadTypes(commonViews);
    Debug.Log("常用View类型预加载完成");
}

// 获取缓存统计
string cacheStats = TypeCacheManager.GetCacheStats();
Debug.Log(cacheStats); // 输出: "类型缓存: 4 个, 程序集缓存: 12 个"

// 清空缓存（切换场景时调用）
void OnSceneChanged()
{
    TypeCacheManager.ClearCache();
    Debug.Log("类型缓存已清空");
}
```

#### MonoSingleton使用
```csharp
// 创建需要挂载在GameObject上的单例
public class AudioManager : MonoSingleton<AudioManager>
{
    public void PlaySound(string soundName)
    {
        Debug.Log($"播放音效: {soundName}");
    }
    
    public void SetVolume(float volume)
    {
        Debug.Log($"设置音量: {volume}");
    }
}

// 使用单例
void PlayGameSound()
{
    // 自动创建GameObject并添加AudioManager组件
    AudioManager.Instance.PlaySound("click");
    AudioManager.Instance.SetVolume(0.8f);
}

// 继承MonoSingleton的类会自动处理销毁
public class NetworkManager : MonoSingleton<NetworkManager>
{
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Debug.Log("NetworkManager已销毁");
    }
}
```

### 6. 界面更新机制

#### 注册更新
```csharp
// 在UIAttribute中设置需要更新
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly, needUpdate: true)]
public class GameHUDLogic : UIViewLogic<GameHUDView>
{
    protected override void OnInit()
    {
        // 自动注册到更新列表
    }
    
    public override void Update()
    {
        // 每帧调用
        UpdateHUD();
    }
}
```

#### 手动注册/注销
```csharp
// 动态控制更新
if (needUpdate)
{
    GUIManager.Instance.RegisterUpdateView(this);
}
else
{
    GUIManager.Instance.UnregisterUpdateView(this);
}
```

## 最佳实践

### 1. 界面设计原则 - View与Logic分层规范

#### 架构分层原则
```
┌─────────────────────────────────────────────────────────┐
│                     GUIManager                           │
│              (界面生命周期和堆栈管理)                     │
└───────────────────────┬─────────────────────────────────┘
                        │
        ┌───────────────┴───────────────┐
        │                               │
┌───────▼────────┐           ┌──────────▼──────────┐
│  UIViewLogic   │           │      UIView          │
│  (业务逻辑层)   │           │   (UI显示层)        │
│                 │           │                       │
│ ✓ 业务逻辑处理   │           │ ✓ UI组件获取         │
│ ✓ 数据处理       │           │ ✓ UI显示更新         │
│ ✓ 事件管理       │           │ ✓ 用户交互事件转发    │
│ ✓ 通过View公开方法│           │ ✓ 提供公开方法供     │
│   操作UI         │           │   Logic调用          │
│                 │           │                       │
│ ✗ 直接操作UI组件 │           │ ✗ 编写业务逻辑       │
└─────────────────┘           └───────────────────────┘
```

#### View层职责规范

**View层应该做什么：**
1. **组件获取**：在`OnInit()`中通过自动生成或手动查找获取所有UI组件
2. **UI显示**：提供公开方法供Logic层调用，用于更新UI显示
3. **事件绑定**：在`OnInit()`或`BindEvents()`中绑定UI交互事件
4. **事件转发**：将UI交互事件转发给Logic层处理
5. **UI适配**：处理刘海屏适配、分辨率调整等UI相关逻辑

**View层代码示例：**
```csharp
public class LoginView : UIView
{
    // UI组件（通过自动生成或手动获取）
    public Button btnLogin;
    public InputField inputUsername;
    public Text txtError;
    
    protected override void OnInit()
    {
        base.OnInit();
        // 1. 获取UI组件（通过自动生成代码或手动查找）
        // btnLogin = GetChildCompByObj<Button>("BtnLogin");
        // inputUsername = GetChildCompByObj<InputField>("InputUsername");
        // txtError = GetChildCompByObj<Text>("TxtError");
        
        // 2. 绑定UI交互事件
        BindEvents();
    }
    
    /// <summary>
    /// 绑定UI交互事件
    /// </summary>
    private void BindEvents()
    {
        // 使用基类提供的方法绑定事件
        SetButtonClick(btnLogin, OnLoginButtonClicked);
    }
    
    /// <summary>
    /// UI事件转发给Logic层
    /// </summary>
    private void OnLoginButtonClicked()
    {
        if (_Logic is LoginLogic logic)
        {
            logic.OnLoginButtonClicked();
        }
    }
    
    // ==================== 公开方法供Logic层调用 ====================
    
    /// <summary>
    /// 设置错误提示文本
    /// </summary>
    public void SetErrorText(string message)
    {
        SetText(txtError, message);
    }
    
    /// <summary>
    /// 设置登录按钮交互状态
    /// </summary>
    public void SetLoginButtonInteractable(bool interactable)
    {
        SetButtonInteractable(btnLogin, interactable);
    }
    
    /// <summary>
    /// 获取用户名输入
    /// </summary>
    public string GetUsername()
    {
        return inputUsername?.text ?? string.Empty;
    }
}
```

**View层不应该做什么：**
```csharp
// ❌ 错误示例：在View中编写业务逻辑
private void OnLoginButtonClicked()
{
    // 不要在这里写登录逻辑！
    string username = inputUsername.text;
    if (string.IsNullOrEmpty(username))
    {
        txtError.text = "用户名不能为空";
        return;
    }
    // 发送网络请求...
    // 这应该在Logic层处理！
}
```

#### Logic层职责规范

**Logic层应该做什么：**
1. **业务逻辑**：处理所有业务逻辑，如登录、数据计算等
2. **数据处理**：管理界面数据状态
3. **事件管理**：注册和处理游戏事件
4. **UI操作**：通过View层提供的公开方法来操作UI
5. **生命周期**：管理界面的生命周期回调

**Logic层代码示例：**
```csharp
[UIAttribute(ViewLayer.Layer1, ViewStack.FullOnly)]
public class LoginLogic : UIViewLogic<LoginView>
{
    private bool isLoggingIn = false;
    
    protected override void OnInit()
    {
        base.OnInit();
        // ✅ 正确：不要在这里直接访问View的UI组件
        // ❌ 错误：不要写 View.btnLogin.onClick.AddListener(...)
    }
    
    protected override void OnShow()
    {
        base.OnShow();
        // ✅ 正确：通过View的公开方法操作UI
        // View.SetErrorText(string.Empty);
    }
    
    /// <summary>
    /// 登录按钮点击 - 由View层调用
    /// </summary>
    public void OnLoginButtonClicked()
    {
        if (isLoggingIn) return;
        
        // ✅ 正确：通过View的公开方法获取数据
        string username = View.GetUsername();
        
        if (string.IsNullOrEmpty(username))
        {
            // ✅ 正确：通过View的公开方法更新UI
            View.SetErrorText("用户名不能为空");
            return;
        }
        
        // 业务逻辑处理
        isLoggingIn = true;
        View.SetLoginButtonInteractable(false);
        View.SetErrorText("登录中...");
        
        // 发送登录事件或网络请求
        SendEvent(this, new LoginRequestArgs(username));
    }
    
    public override void RegisterEvent()
    {
        base.RegisterEvent();
        AddListener(EventDefine.LoginSuccess, OnLoginSuccess);
        AddListener(EventDefine.LoginFailed, OnLoginFailed);
    }
    
    private void OnLoginSuccess(GameEventArgs args)
    {
        isLoggingIn = false;
        View.SetLoginButtonInteractable(true);
        Hide();
        GUIManager.Instance.ShowView<MainLogic>();
    }
    
    private void OnLoginFailed(GameEventArgs args)
    {
        isLoggingIn = false;
        var errorArgs = args as LoginErrorArgs;
        View.SetLoginButtonInteractable(true);
        View.SetErrorText(errorArgs.ErrorMessage);
    }
}
```

**Logic层不应该做什么：**
```csharp
// ❌ 错误示例：直接操作UI组件
protected override void OnInit()
{
    // 不要直接访问View的UI组件！
    View.btnLogin.onClick.AddListener(OnLogin);
    View.inputUsername.text = "default";
}

// ❌ 错误示例：直接设置UI组件属性
private void UpdateUI()
{
    View.txtMessage.text = "Hello"; // 应该通过View.SetMessageText("Hello")
    View.btnSubmit.interactable = true; // 应该通过View.SetSubmitInteractable(true)
}
```

#### 事件驱动通信流程

**UI交互事件流：**
```
用户点击按钮
    ↓
View.btnLogin.onClick (Unity事件)
    ↓
View.OnLoginButtonClicked() (View层处理)
    ↓
Logic.OnLoginButtonClicked() (转发给Logic层)
    ↓
执行业务逻辑
    ↓
View.SetErrorText() / View.SetButtonInteractable() (通过公开方法更新UI)
```

**业务数据更新UI流：**
```
业务数据变化
    ↓
Logic层检测到变化
    ↓
Logic调用View的公开方法
    ↓
View.UpdateSomeUI(data)
    ↓
UI显示更新
```

#### 单一职责原则检查清单

**View层检查清单：**
- [ ] 所有UI组件都在OnInit中获取
- [ ] 所有UI交互事件都转发给Logic层
- [ ] 提供了清晰的公开方法供Logic层调用
- [ ] 没有编写任何业务逻辑代码
- [ ] 没有直接访问游戏数据或网络请求

**Logic层检查清单：**
- [ ] 所有UI操作都通过View的公开方法进行
- [ ] 没有直接访问View的UI组件（如View.btn、View.text）
- [ ] 所有业务逻辑都在Logic层处理
- [ ] 正确使用事件系统与其他模块通信
- [ ] 在RegisterEvent中注册事件监听

### 2. 事件驱动架构
- 使用事件系统进行模块间通信
- View → Logic: 通过方法调用
- Logic → View: 通过View的公共方法
- Logic ↔ 其他模块: 通过事件系统

#### 资源管理
- 及时销毁不再使用的界面
- 使用对象池管理频繁显示的界面
- 预加载常用界面资源

### 2. 性能优化

#### 减少Draw Calls
- 合并UI图集
- 减少透明重叠
- 使用适当的Canvas层级
- **代码示例**:
```csharp
// 优化前：每个UI元素单独Canvas
// 优化后：合并到同一个Canvas
public class OptimizedView : UIView
{
    protected override void OnInit()
    {
        // 使用同一个Canvas下的UI元素
        // 避免为每个按钮创建单独的Canvas
    }
}
```

#### 内存管理
- 及时取消事件订阅
- 清理缓存数据
- 使用WeakReference避免内存泄漏
- **代码示例**:
```csharp
public class MemorySafeLogic : UIViewLogic
{
    private WeakReference<PlayerData> playerDataRef;
    
    protected override void OnInit()
    {
        // 使用WeakReference避免强引用导致内存泄漏
        playerDataRef = new WeakReference<PlayerData>(GameManager.PlayerData);
    }
    
    protected override void OnDestroy()
    {
        // 清理所有引用
        playerDataRef = null;
        base.OnDestroy();
    }
}
```

#### 加载优化
- 异步加载UI资源
- 预加载常用界面
- 使用资源卸载策略
- **代码示例**:
```csharp
public class LoadingOptimizer
{
    // 预加载常用界面
    public async UniTask PreloadCommonViews()
    {
        var tasks = new List<UniTask>();
        
        // 并行预加载
        tasks.Add(PreloadViewAsync("LoginView"));
        tasks.Add(PreloadViewAsync("MainView"));
        tasks.Add(PreloadViewAsync("SettingView"));
        
        await UniTask.WhenAll(tasks);
        Debug.Log("常用界面预加载完成");
    }
    
    private async UniTask PreloadViewAsync(string viewName)
    {
        // 使用Addressables异步加载
        var handle = Addressables.LoadAssetAsync<GameObject>(viewName);
        await handle.ToUniTask();
        
        // 缓存资源
        ResourceCache.Cache(viewName, handle.Result);
    }
}
```

#### 对象池优化
```csharp
public class UIPoolManager
{
    private Dictionary<string, Queue<GameObject>> pool = new();
    
    public GameObject GetUI(string prefabName)
    {
        if (pool.TryGetValue(prefabName, out var queue) && queue.Count > 0)
        {
            var obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        
        // 创建新对象
        var prefab = Resources.Load<GameObject>(prefabName);
        return GameObject.Instantiate(prefab);
    }
    
    public void ReturnUI(string prefabName, GameObject obj)
    {
        obj.SetActive(false);
        
        if (!pool.ContainsKey(prefabName))
            pool[prefabName] = new Queue<GameObject>();
            
        pool[prefabName].Enqueue(obj);
    }
}
```

### 3. 错误处理

#### 加载失败处理
```csharp
private void Handle_Completed(GameObject panel)
{
    if (panel == null)
    {
        IsLoadDone = true;
        Debug.LogWarning($"[{Name}] UI资源加载失败");
        
        // 调用回调允许上层处理错误
        _prepareCallback?.Invoke(this, null);
        return;
    }
    
    // 正常处理...
}
```

#### 事件处理异常
```csharp
try
{
    listener(msg);
}
catch (Exception ex)
{
    Debug.LogError($"[{Name}] 事件处理异常 {msg.Id}: {ex.Message}");
}
```

#### 线程安全
```csharp
// 使用锁保护共享资源
lock (_eventLock)
{
    // 操作事件相关数据
}

lock (_updateListLock)
{
    // 操作更新列表
}
```

## 常见问题

### Q1: 界面显示黑屏或位置不正确
**解决方案**:
1. 检查预制体是否有Canvas组件
2. 确认RectTransform的锚点设置正确
3. 检查UIRoot是否在场景中正确实例化
4. 验证屏幕方向适配逻辑

### Q2: 事件监听不生效
**解决方案**:
1. 确认在RegisterEvent方法中正确调用AddListener
2. 检查事件ID是否匹配
3. 验证事件发送时机是否在监听注册之后
4. 查看是否在Hide时被自动取消订阅

### Q3: 内存泄漏
**解决方案**:
1. 确保在Destroy时取消所有事件订阅
2. 检查是否有循环引用
3. 使用DestroyAllView清理所有界面
4. 监控事件监听器数量

### Q4: 异步加载卡顿
**解决方案**:
1. 使用Addressables或AssetBundle替代Resources.Load
2. 实现加载进度显示
3. 使用对象池复用界面实例
4. 优化资源大小和格式

## 调试技巧

### 1. 日志输出
系统内置详细的日志输出，可通过Debug.Log查看：
- 界面加载状态
- 事件订阅/发送记录
- 堆栈变化
- 更新循环状态

### 2. 性能监控
```csharp
// 查看缓存统计
string stats = TypeCacheManager.GetCacheStats();
Debug.Log($"类型缓存: {stats}");

// 监控界面数量
int viewCount = GUIManager.Instance.GetViewCount();
Debug.Log($"当前界面数量: {viewCount}");
```

### 3. 内存分析
使用Unity Profiler监控：
- UI Canvas数量
- Draw Calls
- 内存占用
- 事件监听器数量

## 扩展开发

### 1. 自定义加载策略
```csharp
public class AddressablesUILoader : IUILoader
{
    public async UniTask<GameObject> LoadUIAsync(string viewName)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(viewName);
        await handle.ToUniTask();
        return handle.Result;
    }
}
```

### 2. 添加动画系统
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

### 3. 国际化支持
```csharp
public class LocalizedView : UIView
{
    protected override void OnInit()
    {
        // 自动应用本地化文本
        ApplyLocalization();
    }
    
    private void ApplyLocalization()
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

## 版本更新记录

### v1.0.0 - 基础版本
- 实现MVC架构
- 支持界面堆栈管理
- 基础事件系统
- 异步加载支持

### v1.1.0 - 增强版本
- 添加刘海屏适配
- 优化事件队列机制
- 增加性能监控
- 改进错误处理

### v1.2.0 - 优化版本
- 添加类型缓存管理器
- 优化内存管理
- 增强调试功能
- 改进文档

---

## 📚 API参考

### GUIManager 核心API

| 方法 | 说明 | 示例 |
|------|------|------|
| `ShowView<T>(params)` | 显示界面 | `ShowView<LoginLogic>("user")` |
| `HideView<T>()` | 隐藏界面 | `HideView<LoginLogic>()` |
| `GetView<T>()` | 获取界面实例 | `var logic = GetView<LoginLogic>()` |
| `IsViewVisible<T>()` | 判断界面是否可见 | `bool visible = IsViewVisible<LoginLogic>()` |
| `DestroyView<T>(bool)` | 销毁界面 | `DestroyView<LoginLogic>(true)` |
| `DestroyAllView()` | 销毁所有界面 | `DestroyAllView()` |
| `HideAllOverLayView()` | 隐藏所有叠加界面 | `HideAllOverLayView()` |
| `IsAnyLoading()` | 是否有界面正在加载 | `bool loading = IsAnyLoading()` |

### UIViewLogic 生命周期

| 方法 | 调用时机 | 可重写 |
|------|----------|--------|
| `OnInit()` | 初始化时 | ✅ |
| `OnShow()` | 显示时 | ✅ |
| `OnHide()` | 隐藏时 | ✅ |
| `OnDestroy()` | 销毁时 | ✅ |
| `OnEscape()` | 返回键按下 | ✅ |
| `Update()` | 每帧更新 | ✅ |
| `RegisterEvent()` | 注册事件监听 | ✅ |

### UIView 组件操作

| 方法 | 说明 | 示例 |
|------|------|------|
| `Find<T>(path)` | 查找组件 | `btn = Find<Button>("BtnOK")` |
| `GetChildObj(path)` | 获取子对象 | `obj = GetChildObj("Panel/Item")` |
| `GetChildCompByObj<T>(path)` | 获取子组件 | `text = GetChildCompByObj<Text>("Label")` |
| `CloneTemplate(obj, reset)` | 克隆模板 | `CloneTemplate(template, true)` |
| `SetInteractable(bool)` | 设置交互性 | `SetInteractable(false)` |
| `SetActive(bool)` | 设置激活状态 | `SetActive(true)` |

### 事件系统API

| 方法 | 说明 | 示例 |
|------|------|------|
| `AddListener(id, callback)` | 添加事件监听 | `AddListener("LOGIN", OnLogin)` |
| `RemoveListener(id, callback)` | 移除事件监听 | `RemoveListener("LOGIN", OnLogin)` |
| `SendEvent(sender, args)` | 发送事件 | `SendEvent(this, new LoginArgs())` |

### UIRoot 坐标转换

| 方法 | 说明 | 示例 |
|------|------|------|
| `Screen2Canvas(pos)` | 屏幕→Canvas坐标 | `Screen2Canvas(Input.mousePosition)` |
| `Canvas2Screen(pos)` | Canvas→屏幕坐标 | `Canvas2Screen(new Vector2(100, 50))` |
| `World2Canvas(pos)` | 世界→Canvas坐标 | `World2Canvas(enemy.position)` |

## 🔧 实用工具

### 调试工具
```csharp
// 查看所有界面状态
void DebugAllViews()
{
    foreach (var kvp in GUIManager.Instance.GetAllViews())
    {
        Debug.Log($"{kvp.Key}: Visible={kvp.Value.IsVisible()}");
    }
}

// 监控事件系统
void MonitorEvents()
{
    // 添加事件日志
    LogicEventDispatcher.Instance.OnEventSent += (id, args) => 
    {
        Debug.Log($"事件发送: {id}");
    };
}
```

### 性能监控
```csharp
// 监控UI性能
public class UIPerformanceMonitor : MonoBehaviour
{
    private float lastCheckTime;
    
    void Update()
    {
        if (Time.time - lastCheckTime > 5f)
        {
            lastCheckTime = Time.time;
            
            // 检查界面数量
            int viewCount = GUIManager.Instance.GetViewCount();
            Debug.Log($"当前界面数量: {viewCount}");
            
            // 检查缓存状态
            string cacheStats = TypeCacheManager.GetCacheStats();
            Debug.Log($"缓存状态: {cacheStats}");
        }
    }
}
```

## 🎯 最佳实践总结

### Do's ✅
- ✅ 使用UIAttribute设置界面属性
- ✅ 在View的OnInit中查找组件
- ✅ 在Logic的RegisterEvent中订阅事件
- ✅ 及时取消事件订阅避免内存泄漏
- ✅ 使用异步加载提升用户体验
- ✅ 预加载常用界面减少卡顿

### Don'ts ❌
- ❌ 不要在View中编写业务逻辑
- ❌ 不要在Logic中直接操作UI组件
- ❌ 不要忘记在Hide/Destroy时清理资源
- ❌ 不要在同一界面重复调用ShowView
- ❌ 不要阻塞主线程进行UI加载
- ❌ 不要创建过多的Canvas层级

## 📞 技术支持

### 遇到问题？
1. **查看日志**: 系统内置详细日志输出
2. **检查配置**: 确认UIRoot在场景中正确实例化
3. **验证资源**: 确认UI预制体路径正确
4. **调试事件**: 使用事件监控工具检查事件流

### 需要定制？
系统设计为可扩展架构，支持：
- 自定义加载策略（Addressables/YooAsset等）
- 自定义动画系统
- 自定义国际化支持
- 自定义皮肤/主题系统

---
