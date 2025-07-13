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
- **拖拽支持**: 完整的鼠标拖拽交互功能

#### 拖拽功能特性
- **完整交互**: 支持鼠标左键拖拽节点位置
- **边界限制**: 自动限制节点在画布范围内移动
- **层级管理**: 拖拽时自动将节点置于最前显示
- **平滑体验**: 鼠标捕获确保拖拽过程不会意外中断
- **事件隔离**: 阻止拖拽事件向父容器传播

#### 拖拽实现详解
```csharp
// 拖拽状态管理
private bool _isDragging = false;
private Vector2 _dragOffset;
private Vector2 _startPosition;

// 关键方法
public bool IsDragging => _isDragging;  // 查询拖拽状态
public Vector2 StartPosition => _startPosition;  // 获取拖拽开始位置

// 边界限制
private Vector2 ClampToCanvas(Vector2 position)
{
    // 自动计算画布边界，防止节点移出可视区域
}

// 层级管理
private void BringNodeToFront()
{
    // 将当前节点移至显示层级最前
}
```

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

### 已实现功能

#### 节点拖拽系统
当前已完全实现的拖拽功能包括：

1. **基础拖拽**: 鼠标左键点击并拖拽节点
2. **边界约束**: 节点无法拖出画布边界
3. **视觉反馈**: 拖拽时节点自动置前显示
4. **事件处理**: 完善的鼠标事件管理和状态追踪

使用方法：
```csharp
// 检查节点是否正在被拖拽
if (node.IsDragging)
{
    // 执行拖拽相关逻辑
}

// 获取拖拽开始位置（用于撤销等功能）
var startPos = node.StartPosition;
```

#### 扩展拖拽功能
基于当前实现，可以轻松添加以下功能：

1. **多选拖拽**:
```csharp
// 在NodeCanvasPanel中实现
private List<BehaviourNode> selectedNodes = new List<BehaviourNode>();

private void DragSelectedNodes(Vector2 deltaPosition)
{
    foreach (var node in selectedNodes)
    {
        if (!node.IsDragging) // 避免重复处理主拖拽节点
        {
            var currentPos = new Vector2(node.style.left.value.value, node.style.top.value.value);
            node.SetPosition(currentPos + deltaPosition);
        }
    }
}
```

2. **网格对齐**:
```csharp
// 在BehaviourNode中重写位置设置
public void SetPosition(Vector2 position)
{
    // 网格对齐逻辑
    if (snapToGrid)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
    }
    
    style.left = position.x;
    style.top = position.y;
}
```

3. **拖拽历史记录**:
```csharp
// 在OnPositionChanged中实现
private void OnPositionChanged()
{
    // 记录位置变更用于撤销/重做
    var command = new MoveNodeCommand(this, StartPosition, GetCurrentPosition());
    EditorHistory.ExecuteCommand(command);
}
```

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

#### 4. 实现拖拽增强功能
```csharp
// 添加拖拽辅助功能
public class EnhancedDragHandler
{
    private bool snapToGrid = true;
    private float gridSize = 20f;
    private List<BehaviourNode> selectedNodes = new List<BehaviourNode>();
    
    public void EnableGridSnap(bool enable, float size = 20f)
    {
        snapToGrid = enable;
        gridSize = size;
    }
    
    public void HandleMultiNodeDrag(BehaviourNode primaryNode, Vector2 deltaPosition)
    {
        foreach (var node in selectedNodes)
        {
            if (node != primaryNode)
            {
                var currentPos = new Vector2(node.style.left.value.value, node.style.top.value.value);
                node.SetPosition(currentPos + deltaPosition);
            }
        }
    }
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
   - 拖拽过程中的边界检查已优化，仅在必要时计算
   - 使用事件捕获机制减少不必要的事件传播
4. **内存管理**: 及时清理不再使用的节点和连接
5. **拖拽优化**: 
   - 拖拽过程中暂停不必要的UI更新
   - 使用增量位置计算减少重复计算

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

### 节点拖拽功能（已实现）
当前的BehaviourNode类已经完全实现了拖拽功能，包括：

```csharp
// 当前实现的关键特性
public class BehaviourNode : VisualElement
{
    // 拖拽状态管理
    private bool _isDragging = false;
    private Vector2 _dragOffset;
    private Vector2 _startPosition;
    
    // 事件处理
    private void OnMouseDown(MouseDownEvent evt)
    {
        // 开始拖拽，记录初始状态
        // 捕获鼠标，确保拖拽连续性
        // 将节点置于最前显示
    }
    
    private void OnMouseMove(MouseMoveEvent evt)
    {
        // 实时更新节点位置
        // 应用边界限制
    }
    
    private void OnMouseUp(MouseUpEvent evt)
    {
        // 结束拖拽，释放鼠标捕获
        // 触发位置变更事件
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
A: 继承`BehaviourNode`类，重写构造函数并自定义样式和行为。拖拽功能会自动继承。

### Q: 如何禁用某个节点的拖拽功能？
A: 在节点构造函数中不调用`SetupDragging()`方法，或者重写拖拽事件处理方法。

### Q: 如何实现拖拽时的网格对齐？
A: 重写`SetPosition`方法，在其中添加网格对齐逻辑。

### Q: 如何获取节点的拖拽状态？
A: 使用`node.IsDragging`属性查询当前拖拽状态，使用`node.StartPosition`获取拖拽开始位置。

### Q: 拖拽性能如何优化？
A: 当前实现已包含基础优化，如事件捕获、边界预计算等。对于大量节点，可考虑实现拖拽区域检测和延迟更新。

## 版本历史

- **v1.0**: 基础框架实现，包含工具栏、布局和示例节点
- **v1.1**: 实现完整的节点拖拽功能，包含边界限制和层级管理
- **待开发**: 节点连接、多选、保存加载功能

## 贡献指南

1. 遵循现有的代码风格和命名约定
2. 为新功能添加相应的文档说明
3. 确保新代码通过编译检查
4. 测试新功能的兼容性和性能

---

*最后更新: 2025年7月*
