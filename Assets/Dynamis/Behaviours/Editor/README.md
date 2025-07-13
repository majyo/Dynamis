# Behaviour Editor Window - 开发者文档

## 概述

Behaviour Editor Window 是一个基于Unity UIElements构建的行为树编辑器窗口，提供可视化的节点编辑界面。该编辑器采用模块化设计，便于扩展和维护。

## 架构概览

### 文件结构
```
Assets/Dynamis/Behaviours/Editor/
├── BehaviourEditorWindow.cs        # 主窗口类
└── Views/
    ├── BehaviourEditorToolbar.cs   # 工具栏组件
    ├── TwoColumnLayout.cs          # 两列布局组件
    ├── NodeCanvasPanel.cs          # 节点画布面板
    ├── BehaviourNode.cs            # 行为树节点UI组件
    ├── TwoColumnLayout.uxml        # 布局UXML文件
    └── TwoColumnLayout.uss         # 布局样式文件
```

### 组件层次结构
```
BehaviourEditorWindow (EditorWindow)
├── BehaviourEditorToolbar
└── TwoColumnLayout
    ├── LeftPanel (预留用于节点列表/属性面板)
    └── RightPanel
        └── NodeCanvasPanel
            └── BehaviourNode(s)
```

## 核心组件详解

### 1. BehaviourEditorWindow
**文件路径**: `BehaviourEditorWindow.cs`  
**职责**: 主窗口类，负责整体布局和组件初始化

#### 关键方法
- `ShowWindow()`: 静态方法，用于打开编辑器窗口
- `CreateGUI()`: 创建UI界面，设置组件层次
- `SetupNodeCanvas()`: 初始化节点画布

#### 扩展点
- 可在`CreateGUI()`中添加新的UI组件
- 可通过`Instance`静态属性访问窗口实例
- 支持添加菜单项和快捷键

### 2. BehaviourEditorToolbar
**文件路径**: `Views/BehaviourEditorToolbar.cs`  
**职责**: 提供编辑器工具栏功能

#### 功能分组
- **文件操作**: New, Open, Save
- **编辑操作**: Undo, Redo, Delete
- **视图控制**: Fit All, Grid
- **运行控制**: Play, Pause, Stop
- **帮助**: Help

#### 添加新按钮
```csharp
// 在对应的Create*Section()方法中添加
var customButton = CreateToolbarButton("Custom", "Custom functionality");
customButton.clicked += OnCustomClicked;
Add(customButton);

// 添加对应的事件处理方法
private void OnCustomClicked() 
{
    // 自定义功能实现
}
```

#### 样式自定义
- 修改`SetupToolbar()`中的样式设置
- 使用`CreateToolbarButton()`创建统一样式的按钮
- 通过`CreateSeparator()`添加分组分隔线

### 3. TwoColumnLayout
**文件路径**: `Views/TwoColumnLayout.cs`  
**职责**: 提供可拖拽调整的两列布局

#### 核心特性
- 支持拖拽调整左右面板宽度
- 最小面板宽度限制（200px）
- 实时响应窗口大小变化

#### 访问面板内容
```csharp
// 通过公共属性访问面板内容区域
var leftContent = twoColumnLayout.LeftContent;
var rightContent = twoColumnLayout.RightContent;

// 添加内容到面板
leftContent?.Add(newElement);
rightContent?.Add(newElement);
```

#### 自定义布局
- 修改`MinPanelWidth`和`SplitterWidth`常量
- 在`OnGlobalMouseMove()`中调整拖拽逻辑
- 可扩展支持多列布局

### 4. NodeCanvasPanel
**文件路径**: `Views/NodeCanvasPanel.cs`  
**职责**: 节点画布容器，管理节点显示和交互

#### 当前功能
- 深色背景画布
- 示例节点展示
- 节点位置管理

#### 扩展功能建议
```csharp
// 添加节点管理方法
public void AddNode(BehaviourNode node, Vector2 position)
{
    node.SetPosition(position);
    Add(node);
}

public void RemoveNode(BehaviourNode node)
{
    Remove(node);
}

// 添加网格显示
private void DrawGrid()
{
    // 网格绘制逻辑
}

// 添加缩放和平移支持
private void HandlePanAndZoom()
{
    // 缩放平移逻辑
}
```

### 5. BehaviourNode
**文件路径**: `Views/BehaviourNode.cs`  
**职责**: 单个行为树节点的UI表示

#### 节点组成
- **头部区域**: 蓝色背景，显示节点名称
- **内容区域**: 显示节点描述
- **连接点**: 顶部输入点和底部输出点

#### 自定义节点类型
```csharp
// 继承BehaviourNode创建特定类型节点
public class ActionNode : BehaviourNode
{
    public ActionNode(string name, string description) : base(name, description)
    {
        // 设置Action节点特有样式
        var header = this.Q<VisualElement>("node-header");
        header.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // 绿色
    }
}

public class ConditionNode : BehaviourNode
{
    public ConditionNode(string name, string description) : base(name, description)
    {
        // 设置Condition节点特有样式
        var header = this.Q<VisualElement>("node-header");
        header.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 1f); // 橙色
    }
}
```

#### 添加节点属性编辑
```csharp
// 在节点中添加属性字段
private void CreatePropertyFields()
{
    var propertyContainer = new VisualElement();
    
    var floatField = new FloatField("Duration");
    var stringField = new TextField("Target Name");
    
    propertyContainer.Add(floatField);
    propertyContainer.Add(stringField);
    
    this.Q<VisualElement>("node-content").Add(propertyContainer);
}
```

## 开发指南

### 添加新功能

#### 1. 添加左侧面板内容
```csharp
// 在BehaviourEditorWindow中
private void SetupLeftPanel()
{
    var leftContent = twoColumnLayout.LeftContent;
    if (leftContent != null)
    {
        var nodeLibrary = new NodeLibraryPanel();
        leftContent.Add(nodeLibrary);
    }
}
```

#### 2. 实现节点连接线
```csharp
// 在NodeCanvasPanel中添加连接线管理
public class NodeConnection : VisualElement
{
    private BehaviourNode fromNode;
    private BehaviourNode toNode;
    
    public void DrawConnection()
    {
        // 使用Painter2D或VisualElement绘制连接线
    }
}
```

#### 3. 添加上下文菜单
```csharp
// 在NodeCanvasPanel中添加右键菜单
private void SetupContextMenu()
{
    this.RegisterCallback<ContextualMenuPopulateEvent>(OnContextMenuPopulate);
}

private void OnContextMenuPopulate(ContextualMenuPopulateEvent evt)
{
    evt.menu.AppendAction("Add Action Node", action => CreateActionNode());
    evt.menu.AppendAction("Add Condition Node", action => CreateConditionNode());
}
```

### 样式自定义

#### 修改主题颜色
```csharp
// 在各组件的样式设置中修改颜色值
public static class EditorTheme
{
    public static readonly Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public static readonly Color PanelColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public static readonly Color AccentColor = new Color(0.2f, 0.4f, 0.8f, 1f);
}
```

#### 使用USS样式表
```css
/* 在TwoColumnLayout.uss中添加自定义样式 */
.custom-node {
    background-color: rgb(64, 64, 64);
    border-radius: 8px;
    border-width: 2px;
    border-color: rgb(128, 128, 128);
}
```

### 性能优化建议

1. **节点数量优化**: 当节点数量较多时，考虑使用虚拟化或分页
2. **连接线渲染**: 使用Painter2D进行高效的连接线绘制
3. **事件处理**: 避免在高频事件中进行复杂计算
4. **内存管理**: 及时清理不再使用的节点和连接

### 调试技巧

#### 使用Debug输出
```csharp
// 在关键位置添加调试信息
Debug.Log($"Node created: {nodeName} at position {position}");
```

#### UI Debugger
使用Unity的UI Toolkit Debugger查看UI元素层次和样式：
- Window > UI Toolkit > Debugger

#### 事件监听
```csharp
// 监听UI事件进行调试
element.RegisterCallback<MouseDownEvent>(evt => 
{
    Debug.Log($"Mouse down at: {evt.mousePosition}");
});
```

## 扩展示例

### 实现节点拖拽
```csharp
public class DraggableNode : BehaviourNode
{
    private bool isDragging = false;
    private Vector2 dragOffset;
    
    public DraggableNode(string name, string description) : base(name, description)
    {
        RegisterCallback<MouseDownEvent>(OnMouseDown);
        RegisterCallback<MouseMoveEvent>(OnMouseMove);
        RegisterCallback<MouseUpEvent>(OnMouseUp);
    }
    
    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 0)
        {
            isDragging = true;
            dragOffset = evt.localMousePosition;
            this.CaptureMouse();
        }
    }
    
    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (isDragging)
        {
            var newPosition = evt.mousePosition - dragOffset;
            SetPosition(newPosition);
        }
    }
    
    private void OnMouseUp(MouseUpEvent evt)
    {
        if (isDragging)
        {
            isDragging = false;
            this.ReleaseMouse();
        }
    }
}
```

### 实现节点选择系统
```csharp
public class SelectableNode : BehaviourNode
{
    public bool IsSelected { get; private set; }
    
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        UpdateSelectionVisual();
    }
    
    private void UpdateSelectionVisual()
    {
        if (IsSelected)
        {
            style.borderTopColor = Color.yellow;
            style.borderBottomColor = Color.yellow;
            style.borderLeftColor = Color.yellow;
            style.borderRightColor = Color.yellow;
        }
        else
        {
            // 恢复默认边框颜色
        }
    }
}
```

## 常见问题

### Q: 如何添加新的节点类型？
A: 继承`BehaviourNode`类，重写构造函数并自定义样式和行为。

### Q: 如何修改布局比例？
A: 在`TwoColumnLayout`的拖拽事件中调整百分比计算逻辑。

### Q: 如何添加键盘快捷键？
A: 在`BehaviourEditorWindow`中注册键盘事件回调。

### Q: 如何保存和加载行为树数据？
A: 实现序列化系统，将节点数据转换为可保存的格式。

## 版本历史

- **v1.0**: 基础框架实现，包含工具栏、布局和示例节点
- **待开发**: 节点连接、拖拽、保存加载功能

## 贡献指南

1. 遵循现有的代码风格和命名约定
2. 为新功能添加相应的文档说明
3. 确保新代码通过编译检查
4. 测试新功能的兼容性和性能

---

*最后更新: 2025年7月*
