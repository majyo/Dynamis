using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Dynamis.Behaviours.Editor.Events;

namespace Dynamis.Behaviours.Editor.Views
{
    public class NodeCanvasPanel : VisualElement
    {
        private Vector2 _canvasOffset = Vector2.zero;
        private VisualElement _contentContainer;

        private BehaviourNode _mouseHoveredNode;
        private readonly HashSet<BehaviourNode> _selectedNodes = new();

        // 连线系统相关字段
        private ConnectionRenderer _connectionRenderer;
        private readonly List<Connection> _connections = new();

        // 悬浮连线相关字段
        private Port _draggingFromPort;
        private DummyPort _draggingToPort;
        private Connection _draggingConnection;

        // 右键菜单相关字段
        private CustomPopupMenu _contextMenu;
        private Vector2 _lastRightClickPosition;
        
        private NodeCanvasEventHandler _eventHandler;

        public BehaviourNode MouseHoveredNode
        {
            get => _mouseHoveredNode;
            set
            {
                if (_mouseHoveredNode == value)
                {
                    return;
                }

                if (_mouseHoveredNode != null)
                {
                    _mouseHoveredNode.IsHovered = false;
                }

                _mouseHoveredNode = value;

                if (_mouseHoveredNode != null)
                {
                    _mouseHoveredNode.IsHovered = true;
                }
            }
        }
        
        public IReadOnlyCollection<BehaviourNode> SelectedNodes => _selectedNodes;

        public NodeCanvasPanel()
        {
            SetupPanel();
            SetupEventHandling();
        }

        public bool AddToSelection(BehaviourNode node)
        {
            var succeed = _selectedNodes.Add(node);
            
            if (succeed)
            {
                node.IsSelected = true;
            }
            
            return succeed;
        }

        public bool RemoveFromSelection(BehaviourNode node)
        {
            var succeed = _selectedNodes.Remove(node);
            
            if (succeed)
            {
                node.IsSelected = false;
            }
            
            return succeed;
        }

        public void ClearSelection()
        {
            foreach (var node in _selectedNodes)
            {
                node.IsSelected = false;
            }
            
            _selectedNodes.Clear();
        }

        public void MoveCanvas(Vector2 delta)
        {
            _canvasOffset += delta;
            _contentContainer.transform.position = _canvasOffset;
            RefreshConnections();
        }

        public BehaviourNode GetNodeAtPosition(Vector2 mousePosition)
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

        public void RemoveNode(BehaviourNode node)
        {
            RemoveNodeConnections(node);
            _contentContainer.Remove(node);
            RemoveFromSelection(node);
            RefreshConnections();
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

            _draggingToPort = new DummyPort();
        }

        private void SetupEventHandling()
        {
            _eventHandler = new NodeCanvasEventHandler(this);
            _eventHandler.RegisterEvents();
        }

        private void RemoveNodeConnections(BehaviourNode node)
        {
            var connectionsToRemove = _connections
                .Where(connection =>
                {
                    if (connection.InputPort is not Port inputPort)
                    {
                        return false;
                    }

                    if (connection.OutputPort is not Port outputPort)
                    {
                        return false;
                    }

                    return inputPort.ParentNode == node || outputPort.ParentNode == node;
                })
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                RemoveConnection(connection);
            }
        }

        // 获取指定位置的端口
        private Port GetPortAtPosition(Vector2 mousePosition)
        {
            // 转换鼠标位置到内容容器的本地坐标
            var localPosition = mousePosition - _canvasOffset;

            // 遍历所有节点，检查端口
            for (var i = _contentContainer.childCount - 1; i >= 0; i--)
            {
                var child = _contentContainer[i];

                if (child is BehaviourNode node)
                {
                    // 检查输入端口
                    if (node.InputPort != null && IsPointInPort(node.InputPort, localPosition))
                    {
                        return node.InputPort;
                    }

                    // 检查输出端口
                    if (node.OutputPort != null && IsPointInPort(node.OutputPort, localPosition))
                    {
                        return node.OutputPort;
                    }
                }
            }

            return null;
        }

        private bool IsPointInPort(Port port, Vector2 position)
        {
            var portWorldPos = port.GetConnectionPoint();
            var portRect = new Rect(portWorldPos.x - 8, portWorldPos.y - 8, 16, 16);
            return portRect.Contains(position);
        }

        public void AddNode(BehaviourNode node, Vector2 position)
        {
            node.CanvasPosition = position;
            node.onPositionChanged = RefreshConnections;

            BindNodeEvents(node);

            _contentContainer.Add(node);
        }

        public void AddConnection(Connection connection)
        {
            _connections.Add(connection);
            _connectionRenderer.AddConnection(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            _connections.Remove(connection);
            _connectionRenderer.RemoveConnection(connection);
        }

        public void RefreshConnections()
        {
            _connectionRenderer.RefreshConnections();
        }

        // 处理port点击事件
        public void OnPortPressed(Port port, bool isAltPressed)
        {
            if (isAltPressed)
            {
                // Alt+点击port，删除该port相关的所有连线
                RemovePortConnections(port);
            }
            else if (port.Type == PortType.Output)
            {
                // 只有输出端口才能开始拖拽连线
                _draggingFromPort = port;
                _draggingToPort.Position = port.GetConnectionPoint();
                _draggingConnection = new Connection(_draggingFromPort, _draggingToPort);
                _connectionRenderer.AddConnection(_draggingConnection);
                _connectionRenderer.RefreshConnections();

                this.CaptureMouse();
            }
        }

        // 删除指定port的所有连线
        public void RemovePortConnections(Port port)
        {
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

        // 右键菜单相关方法
        public void ShowContextMenu(Vector2 localPosition)
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
            _contextMenu.style.left = localPosition.x;
            _contextMenu.style.top = localPosition.y;

            // 显示菜单
            _contextMenu.Show(localPosition);
            _contextMenu.BringToFront();
        }

        public void HideContextMenu()
        {
            if (_contextMenu is { IsVisible: true })
            {
                _contextMenu.Hide();
            }
        }

        private void SetupContextMenuItems()
        {
            // 控制节点组
            var controlGroup = _contextMenu.AddGroup("控制节点", false);
            controlGroup.AddChild("Root", () => CreateNode("Root", "Behaviour tree root node", false, true));
            controlGroup.AddChild("Selector", () => CreateNode("Selector", "Select first successful child", true, true));
            controlGroup.AddChild("Sequence", () => CreateNode("Sequence", "Execute children in order", true, true));
            controlGroup.AddChild("Parallel", () => CreateNode("Parallel", "Execute children in parallel", true, true));

            // _contextMenu.AddSeparator();

            // 条件节点组
            var conditionGroup = _contextMenu.AddGroup("条件节点", false);
            conditionGroup.AddChild("Check Health", () => CreateNode("Check Health", "Check if health is above threshold", true, true));
            conditionGroup.AddChild("Has Target", () => CreateNode("Has Target", "Check if target exists", true, true));
            conditionGroup.AddChild("In Range", () => CreateNode("In Range", "Check if target is in range", true, true));

            // _contextMenu.AddSeparator();

            // 行为节点组
            var actionGroup = _contextMenu.AddGroup("行为节点", false);
            actionGroup.AddChild("Move To Target", () => CreateNode("Move To Target", "Move character to target position", true, false));
            actionGroup.AddChild("Attack Enemy", () => CreateNode("Attack Enemy", "Perform attack on enemy target", true, false));
            actionGroup.AddChild("Patrol", () => CreateNode("Patrol", "Patrol between waypoints", true, false));
            actionGroup.AddChild("Wait", () => CreateNode("Wait", "Wait for specified duration", true, false));
            actionGroup.AddChild("Play Animation", () => CreateNode("Play Animation", "Play character animation", true, false));

            // _contextMenu.AddSeparator();

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

            // 隐藏菜单
            HideContextMenu();
        }

        private void BindNodeEvents(BehaviourNode node)
        {
            if (node.InputPort != null)
            {
                node.InputPort.onPortPressed = OnPortPressed;
            }

            if (node.OutputPort != null)
            {
                node.OutputPort.onPortPressed = OnPortPressed;
            }
        }

        public void SetLastRightClickPosition(Vector2 position)
        {
            _lastRightClickPosition = position;
        }
    }
}