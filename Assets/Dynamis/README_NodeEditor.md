# Unity节点编辑器使用指南

## 功能特性

这个基于Unity UI Elements的通用节点编辑器提供了以下功能：

### 核心功能
- **可视化节点编辑**：拖拽创建和连接节点
- **多种节点类型**：数学运算、输入输出、字符串处理等
- **图形执行**：可以执行节点图并查看结果
- **保存/加载**：支持将图形保存为JSON文件
- **搜索窗口**：快速创建节点的搜索界面

### 节点类型

1. **Input Node（输入节点）**
   - 提供数值输入
   - 只有输出端口
   - 可以设置浮点数值

2. **Math Node（数学节点）**
   - 支持基本数学运算：加、减、乘、除
   - 两个输入端口（Input A、Input B）
   - 一个输出端口（Result）
   - 下拉菜单选择运算类型

3. **Output Node（输出节点）**
   - 显示计算结果
   - 只有输入端口
   - 实时显示连接的数值

4. **String Node（字符串节点）**
   - 处理字符串数据
   - 支持字符串连接
   - 输入和输出端口

## 使用方法

### 1. 打开节点编辑器
- 在Unity菜单栏选择：`Tools > Dynamis > Node Editor`

### 2. 创建节点
- **右键菜单**：在图形视图中右键点击，选择要创建的节点类型
- **搜索窗口**：按空格键或在空白区域双击打开搜索窗口

### 3. 连接节点
- 将输出端口（绿色圆圈）拖拽到输入端口（蓝色圆圈）
- 只有兼容的端口类型才能连接

### 4. 设置参数
- 选择节点后在Inspector面板中设置参数
- 或直接在节点上的UI控件中输入值

### 5. 执行图形
- 点击工具栏的"Execute"按钮
- 查看Console面板的执行结果

### 6. 保存和加载
- **保存**：点击"Save"或"Save As"按钮
- **加载**：点击"Load"按钮选择JSON文件
- **新建**：点击"New"按钮清空当前图形

## 扩展开发

### 创建自定义节点

1. 继承`BaseNode`类：
```csharp
public class CustomNode : BaseNode
{
    public CustomNode() : base()
    {
        NodeTitle = "Custom Node";
        AddToClassList("custom-node");
    }
    
    public CustomNode(Vector2 position) : base(position, "Custom Node")
    {
        AddToClassList("custom-node");
    }
    
    protected override void CreatePorts()
    {
        // 创建输入输出端口
        CreateInputPort("Input", typeof(float));
        CreateOutputPort("Output", typeof(float));
    }
}
```

2. 在`NodeSearchWindow.cs`中添加到搜索树
3. 在`NodeGraphView.cs`的右键菜单中添加
4. 在`GraphExecutor.cs`中添加执行逻辑

### 自定义样式

编辑`NodeEditorStyles.uss`文件来自定义节点外观：
```css
.custom-node {
    border-color: #your-color;
}

.custom-node .title {
    background-color: #your-title-color;
}
```

## 技术架构

- **BaseNode**：所有节点的基类
- **NodeGraphView**：图形视图主类，处理节点创建和连接
- **GraphWindow**：编辑器窗口，提供UI界面
- **NodeSearchWindow**：搜索窗口，快速创建节点
- **GraphExecutor**：图形执行器，按依赖顺序执行节点
- **USS样式文件**：定义节点和界面的视觉样式

## 注意事项

1. 确保Unity版本支持UI Elements
2. 节点执行按照依赖关系自动排序
3. 循环依赖会导致执行失败
4. 保存的JSON文件包含完整的图形信息
5. 自定义节点需要实现序列化支持

这个节点编辑器为您提供了一个强大的可视化编程基础，可以根据具体需求进行扩展和定制。
