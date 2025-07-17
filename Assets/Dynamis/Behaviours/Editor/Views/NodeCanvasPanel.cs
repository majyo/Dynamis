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
        
        // 统一的节点事件绑定方法
        private void BindNodeEvents(BehaviourNode node)
        {
            // 绑定port点击事件
            if (node.InputPort != null)
            {
                node.InputPort.onPortClicked = OnPortClicked;
            }
            
            if (node.OutputPort != null)
            {
                node.OutputPort.onPortClicked = OnPortClicked;
            }
            
            // 未来可以在这里添加更多事件绑定
            // node.onRightClick = OnNodeRightClick;
            // node.onDoubleClick = OnNodeDoubleClick;
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
    }
}