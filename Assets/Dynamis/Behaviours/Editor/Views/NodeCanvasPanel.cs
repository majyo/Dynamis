using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace Dynamis.Behaviours.Editor.Views
{
    public class NodeCanvasPanel : VisualElement
    {
        // 画布拖拽相关字段
        private bool _isCanvasDragging;
        private Vector2 _startMousePosition;
        private Vector2 _canvasOffset = Vector2.zero;
        private VisualElement _contentContainer;

        // 节点拖拽相关字段
        private BehaviourNode _draggingNode;

        // 节点状态管理字段
        private BehaviourNode _hoveredNode;
        private BehaviourNode _selectedNode;

        // 连线系统相关字段
        private ConnectionRenderer _connectionRenderer;
        private readonly List<NodeConnection> _connections = new();
        
        // 右键菜单相关字段
        private CustomPopupMenu _contextMenu;
        private Vector2 _lastRightClickPosition;

        public NodeCanvasPanel()
        {
            SetupPanel();
            SetupEventHandling();
            AddSampleNodes();
            CreateSampleConnections();
        }

        private void SetupPanel()
        {
            // 设置面板名称
            name = "node-canvas-panel";

            // 设置面板样式 - 深灰色背景
            style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 深灰色
            style.flexGrow = 1; // 占满父容器
            style.minWidth = 200; // 最小宽度
            style.minHeight = 200; // 最小高度

            style.overflow = Overflow.Hidden;

            // 创建内容容器，用于承载所有节点
            _contentContainer = new VisualElement
            {
                name = "content-container",
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };
            Add(_contentContainer);

            // 创建连线渲染器
            _connectionRenderer = new ConnectionRenderer();
            _contentContainer.Add(_connectionRenderer);
        }

        private void SetupEventHandling()
        {
            // 注册鼠标事件
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            // 注册键盘事件
            RegisterCallback<KeyDownEvent>(HandleDeleteKeyDown);

            // 允许焦点，这样可以接收键盘事件
            focusable = true;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // 先隐藏可能存在的右键菜单
            HideContextMenu();
            
            // 处理右键点击 - 显示上下文菜单
            if (evt.button == 1) // 右键
            {
                // 检查是否点击在节点上
                var clickedNode = GetNodeAtPosition(evt.localMousePosition);
                if (clickedNode == null)
                {
                    // 点击在空白区域，显示创建节点菜单
                    _lastRightClickPosition = evt.localMousePosition;
                    ShowContextMenu(evt.localMousePosition);
                    evt.StopPropagation();
                    return;
                }
                // 如果点击在节点上，不显示菜单，继续处理画布拖拽
            }
            
            // 优先处理节点拖拽和选择（左键）
            if (evt.button == 0)
            {
                // 检查是否点击在节点上
                var clickedNode = GetNodeAtPosition(evt.localMousePosition);
                if (clickedNode != null)
                {
                    // 处理节点选择
                    SetSelectedNode(clickedNode);

                    _draggingNode = clickedNode;
                    _draggingNode.StartDragging(evt.mousePosition);
                    this.CaptureMouse();
                    evt.StopPropagation();
                    return;
                }
                else
                {
                    // 点击空白区域，取消选择
                    SetSelectedNode(null);
                }
            }

            // 处理画布拖拽（中键或右键）
            if (evt.button == 1 || evt.button == 2) // 中键=1, 右键=2
            {
                _isCanvasDragging = true;
                _startMousePosition = evt.localMousePosition;
                this.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            // 处理节点拖拽
            if (_draggingNode != null && _draggingNode.IsDragging)
            {
                _draggingNode.UpdateDragging(evt.mousePosition);
                evt.StopPropagation();
                return;
            }

            // 处理节点悬浮检测（仅在非拖拽状态下）
            if (!_isCanvasDragging)
            {
                var hoveredNode = GetNodeAtPosition(evt.localMousePosition);
                SetHoveredNode(hoveredNode);
            }

            // 处理画布拖拽
            if (_isCanvasDragging)
            {
                // 计算鼠标移动距离
                Vector2 mouseDelta = evt.localMousePosition - _startMousePosition;

                // 更新画布偏移
                _canvasOffset += mouseDelta;

                // 应用变换到内容容器
                _contentContainer.transform.position = _canvasOffset;

                // 更新起始位置
                _startMousePosition = evt.localMousePosition;

                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            // 处理节点拖拽结束
            if (_draggingNode != null && evt.button == 0)
            {
                _draggingNode.EndDragging();
                _draggingNode = null;
                this.ReleaseMouse();
                evt.StopPropagation();
                return;
            }

            // 处理画布拖拽结束
            if (_isCanvasDragging && (evt.button == 1 || evt.button == 2))
            {
                _isCanvasDragging = false;
                this.ReleaseMouse();
                evt.StopPropagation();
            }
        }

        private void HandleDeleteKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Delete)
            {
                return;
            }

            if (_selectedNode != null)
            {
                RemoveNodeConnections(_selectedNode);
                _contentContainer.Remove(_selectedNode);
                SetSelectedNode(null);
                RefreshConnections();
            }

            evt.StopPropagation();
        }

        private void RemoveNodeConnections(BehaviourNode node)
        {
            var connectionsToRemove = _connections
                .Where(connection =>
                    connection.OutputPort.ParentNode == node || connection.InputPort.ParentNode == node)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                RemoveConnection(connection);
            }
        }

        // 获取指定位置的节点
        private BehaviourNode GetNodeAtPosition(Vector2 mousePosition)
        {
            // 转换鼠标位置到内容容器的本地坐标
            var localPosition = mousePosition - _canvasOffset;

            // 逆序遍历所有子元素，找到包含该点的节点
            for (var i = _contentContainer.childCount - 1; i >= 0; i--)
            {
                var child = _contentContainer[i];

                if (child is BehaviourNode node && node.ContainsPoint(localPosition))
                {
                    return node;
                }
            }

            return null;
        }

        // 优化：为新添加的节点自动绑定事件
        public void AddNode(BehaviourNode node, Vector2 position)
        {
            node.CanvasPosition = position;
            node.onPositionChanged = RefreshConnections;
            
            // 统一的事件绑定方法
            BindNodeEvents(node);
            
            _contentContainer.Add(node);
        }

        public void AddConnection(NodeConnection connection)
        {
            _connections.Add(connection);
            _connectionRenderer.AddConnection(connection);
        }

        public void RemoveConnection(NodeConnection connection)
        {
            _connections.Remove(connection);
            _connectionRenderer.RemoveConnection(connection);
        }

        public void RefreshConnections()
        {
            _connectionRenderer.RefreshConnections();
        }
        
        private void BindNodeEvents(BehaviourNode node)
        {
            if (node.InputPort != null)
            {
                node.InputPort.onPortClicked = OnPortClicked;
            }
            
            if (node.OutputPort != null)
            {
                node.OutputPort.onPortClicked = OnPortClicked;
            }
        }
        
        private void AddSampleNodes()
        {
            // 创建几个示例节点来展示效果

            // Root节点 - 只有输出端口
            var rootNode = new BehaviourNode("Root", "Behaviour tree root node", false, true);
            AddNode(rootNode, new Vector2(200, 50));

            // Selector节点 - 有输入和输出端口
            var selectorNode = new BehaviourNode("Selector", "Select first successful child");
            AddNode(selectorNode, new Vector2(150, 180));

            // Sequence节点 - 有输入和输出端口
            var sequenceNode = new BehaviourNode("Sequence", "Execute children in order");
            AddNode(sequenceNode, new Vector2(250, 180));

            // Action节点 - 只有输入端口
            var actionNode1 = new BehaviourNode("Move To Target", "Move character to target position", true, false);
            AddNode(actionNode1, new Vector2(100, 310));

            var actionNode2 = new BehaviourNode("Attack Enemy", "Perform attack on enemy target", true, false);
            AddNode(actionNode2, new Vector2(200, 310));

            var actionNode3 = new BehaviourNode("Wait", "Wait for specified duration", true, false);
            AddNode(actionNode3, new Vector2(300, 310));

            // 存储节点引用以便创建连线
            _sampleNodes = new Dictionary<string, BehaviourNode>
            {
                ["root"] = rootNode,
                ["selector"] = selectorNode,
                ["sequence"] = sequenceNode,
                ["moveToTarget"] = actionNode1,
                ["attackEnemy"] = actionNode2,
                ["wait"] = actionNode3
            };
        }

        // 处理port点击事件
        private void OnPortClicked(Port port, bool isAltPressed)
        {
            if (isAltPressed)
            {
                // Alt+点击port，删除该port相关的所有连线
                RemovePortConnections(port);
            }
            else
            {
                // 普通点击，可以在这里添加其他逻辑，比如创建连线等
                // 目前暂时不处理
            }
        }

        // 删除指定port的所有连线
        private void RemovePortConnections(Port port)
        {
            // 查找所有与该port相关的连线
            var connectionsToRemove = _connections
                .Where(connection => connection.OutputPort == port || connection.InputPort == port)
                .ToList();

            // 删除找到的所有连线
            foreach (var connection in connectionsToRemove)
            {
                RemoveConnection(connection);
            }

            // 刷新连线显示
            RefreshConnections();
        }

        private Dictionary<string, BehaviourNode> _sampleNodes;

        private void CreateSampleConnections()
        {
            // 等待一帧确保节点位置已确定
            schedule.Execute(() =>
            {
                // Root 连接到 Selector
                var connection1 =
                    new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["selector"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                AddConnection(connection1);

                // Root 连接到 Sequence
                var connection2 =
                    new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["sequence"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                AddConnection(connection2);

                // Selector 连接到 Move To Target
                var connection3 = new NodeConnection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["moveToTarget"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection3);

                // Selector 连接到 Attack Enemy
                var connection4 = new NodeConnection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["attackEnemy"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection4);

                // Sequence 连接到 Wait
                var connection5 =
                    new NodeConnection(_sampleNodes["sequence"].OutputPort, _sampleNodes["wait"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.3f, 0.3f, 1f),
                        LineWidth = 2f
                    };
                AddConnection(connection5);
            }).ExecuteLater(100); // 延迟100ms执行
        }

        private void SetHoveredNode(BehaviourNode node)
        {
            if (_hoveredNode != node)
            {
                // 清除之前悬浮节点的状态
                if (_hoveredNode != null)
                {
                    _hoveredNode.IsHovered = false;
                }

                // 设置新的悬浮节点
                _hoveredNode = node;
                if (_hoveredNode != null)
                {
                    _hoveredNode.IsHovered = true;
                }
            }
        }

        private void SetSelectedNode(BehaviourNode node)
        {
            if (_selectedNode != node)
            {
                // 清除之前选中节点的状态
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = false;
                }

                // 设置新的选中节点
                _selectedNode = node;
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = true;
                }
            }
        }

        public BehaviourNode GetSelectedNode()
        {
            return _selectedNode;
        }

        public BehaviourNode GetHoveredNode()
        {
            return _hoveredNode;
        }

        // 右键菜单相关方法
        private void ShowContextMenu(Vector2 screenPosition)
        {
            // 创建上下文菜单
            if (_contextMenu == null)
            {
                _contextMenu = new CustomPopupMenu
                {
                    TitleContent = "创建节点"
                };
                SetupContextMenuItems();
                Add(_contextMenu);
            }
            
            // 清空并重新设置菜单项
            _contextMenu.ClearItems();
            SetupContextMenuItems();
            
            // 设置菜单位置
            _contextMenu.style.left = screenPosition.x;
            _contextMenu.style.top = screenPosition.y;
            
            // 显示菜单
            _contextMenu.Show(screenPosition);
            _contextMenu.BringToFront();
        }
        
        private void HideContextMenu()
        {
            if (_contextMenu != null && _contextMenu.IsVisible)
            {
                _contextMenu.Hide();
            }
        }
        
        private void SetupContextMenuItems()
        {
            // 控制节点组
            var controlGroup = _contextMenu.AddGroup("控制节点", true);
            controlGroup.AddChild("Root", () => CreateNode("Root", "Behaviour tree root node", false, true));
            controlGroup.AddChild("Selector", () => CreateNode("Selector", "Select first successful child", true, true));
            controlGroup.AddChild("Sequence", () => CreateNode("Sequence", "Execute children in order", true, true));
            controlGroup.AddChild("Parallel", () => CreateNode("Parallel", "Execute children in parallel", true, true));
            
            _contextMenu.AddSeparator();
            
            // 条件节点组
            var conditionGroup = _contextMenu.AddGroup("条件节点", true);
            conditionGroup.AddChild("Check Health", () => CreateNode("Check Health", "Check if health is above threshold", true, true));
            conditionGroup.AddChild("Has Target", () => CreateNode("Has Target", "Check if target exists", true, true));
            conditionGroup.AddChild("In Range", () => CreateNode("In Range", "Check if target is in range", true, true));
            
            _contextMenu.AddSeparator();
            
            // 行为节点组
            var actionGroup = _contextMenu.AddGroup("行为节点", true);
            actionGroup.AddChild("Move To Target", () => CreateNode("Move To Target", "Move character to target position", true, false));
            actionGroup.AddChild("Attack Enemy", () => CreateNode("Attack Enemy", "Perform attack on enemy target", true, false));
            actionGroup.AddChild("Patrol", () => CreateNode("Patrol", "Patrol between waypoints", true, false));
            actionGroup.AddChild("Wait", () => CreateNode("Wait", "Wait for specified duration", true, false));
            actionGroup.AddChild("Play Animation", () => CreateNode("Play Animation", "Play character animation", true, false));
            
            _contextMenu.AddSeparator();
            
            // 装饰器节点组
            var decoratorGroup = _contextMenu.AddGroup("装饰器节点", false);
            decoratorGroup.AddChild("Inverter", () => CreateNode("Inverter", "Invert child result", true, true));
            decoratorGroup.AddChild("Repeater", () => CreateNode("Repeater", "Repeat child execution", true, true));
            decoratorGroup.AddChild("Cooldown", () => CreateNode("Cooldown", "Add cooldown to child", true, true));
        }
        
        private void CreateNode(string nodeName, string description, bool hasInput, bool hasOutput)
        {
            // 转换右键点击位置到画布坐标
            var canvasPosition = _lastRightClickPosition - _canvasOffset;
            
            // 创建新节点
            var newNode = new BehaviourNode(nodeName, description, hasInput, hasOutput);
            AddNode(newNode, canvasPosition);
            
            // 选中新创建的节点
            SetSelectedNode(newNode);
            
            // 隐藏菜单
            HideContextMenu();
        }
    }
}
