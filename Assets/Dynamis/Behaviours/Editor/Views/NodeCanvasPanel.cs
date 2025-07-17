using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Dynamis.Behaviours.Editor.Manipulators;
using ContextualMenuManipulator = UnityEngine.UIElements.ContextualMenuManipulator;

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

        public NodeCanvasPanel()
        {
            SetupPanel();
            SetupEventHandling();
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

            this.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            switch (evt.button)
            {
                case 0:
                    OnLeftMouseDown(evt);
                    break;
                case 1:
                    OnRightMouseDown(evt);
                    break;
            }
        }

        // 节点拖拽和选择的处理
        private void OnLeftMouseDown(MouseDownEvent evt)
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

            // 点击空白区域，取消选择
            SetSelectedNode(null);
        }
        
        // 弹出菜单和画布拖拽的处理
        private void OnRightMouseDown(MouseDownEvent evt)
        {
            // 检查是否点击在节点上
            var clickedNode = GetNodeAtPosition(evt.localMousePosition);
                
            if (clickedNode != null)
            {
                // 如果点击在节点上，不显示菜单，继续处理画布拖拽
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
            if (_isCanvasDragging && (evt.button == 1))
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

        // 处理port点击事件
        private void OnPortClicked(Port port, bool isAltPressed)
        {
            if (isAltPressed)
            {
                // Alt+点击port，删除该port相关的所有连线
                RemovePortConnections(port);
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

        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            var localPosition = evt.localMousePosition;
            
            // 控制节点组
            evt.menu.AppendAction("控制节点/Root", _ => CreateNode("Root", "Behaviour tree root node", false, true, localPosition));
            evt.menu.AppendAction("控制节点/Selector", _ => CreateNode("Selector", "Select first successful child", true, true, localPosition));
            evt.menu.AppendAction("控制节点/Sequence", _ => CreateNode("Sequence", "Execute children in order", true, true, localPosition));
            evt.menu.AppendAction("控制节点/Parallel", _ => CreateNode("Parallel", "Execute children in parallel", true, true, localPosition));
            
            evt.menu.AppendSeparator();
            
            // 条件节点组
            evt.menu.AppendAction("条件节点/Check Health", _ => CreateNode("Check Health", "Check if health is above threshold", true, true, localPosition));
            evt.menu.AppendAction("条件节点/Has Target", _ => CreateNode("Has Target", "Check if target exists", true, true, localPosition));
            evt.menu.AppendAction("条件节点/In Range", _ => CreateNode("In Range", "Check if target is in range", true, true, localPosition));
            
            evt.menu.AppendSeparator();
            
            // 行为节点组
            evt.menu.AppendAction("行为节点/Move To Target", _ => CreateNode("Move To Target", "Move character to target position", true, false, localPosition));
            evt.menu.AppendAction("行为节点/Attack Enemy", _ => CreateNode("Attack Enemy", "Perform attack on enemy target", true, false, localPosition));
            evt.menu.AppendAction("行为节点/Patrol", _ => CreateNode("Patrol", "Patrol between waypoints", true, false, localPosition));
            evt.menu.AppendAction("行为节点/Wait", _ => CreateNode("Wait", "Wait for specified duration", true, false, localPosition));
            evt.menu.AppendAction("行为节点/Play Animation", _ => CreateNode("Play Animation", "Play character animation", true, false, localPosition));
            
            evt.menu.AppendSeparator();
            
            // 装饰器节点组
            evt.menu.AppendAction("装饰器节点/Inverter", _ => CreateNode("Inverter", "Invert child result", true, true, localPosition));
            evt.menu.AppendAction("装饰器节点/Repeater", _ => CreateNode("Repeater", "Repeat child execution", true, true, localPosition));
            evt.menu.AppendAction("装饰器节点/Cooldown", _ => CreateNode("Cooldown", "Add cooldown to child", true, true, localPosition));
        }
        
        private void CreateNode(string nodeName, string description, bool hasInput, bool hasOutput, Vector2 mousePosition)
        {
            // 转换右键点击位置到画布坐标
            var canvasPosition = mousePosition - _canvasOffset;
            
            // 创建新节点
            var newNode = new BehaviourNode(nodeName, description, hasInput, hasOutput);
            AddNode(newNode, canvasPosition);
            
            // 选中新创建的节点
            SetSelectedNode(newNode);
        }
    }
}
