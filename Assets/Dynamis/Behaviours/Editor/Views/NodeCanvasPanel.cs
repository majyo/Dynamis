using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

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
        
        // 连线系统相关字段
        private ConnectionRenderer _connectionRenderer;
        private List<NodeConnection> _connections = new List<NodeConnection>();
        
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
                style = {
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
            
            // 允许焦点，这样可以接收键盘事件
            focusable = true;
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            // 优先处理节点拖拽（左键）
            if (evt.button == 0)
            {
                // 检查是否点击在节点上
                var clickedNode = GetNodeAtPosition(evt.localMousePosition);
                if (clickedNode != null)
                {
                    _draggingNode = clickedNode;
                    _draggingNode.StartDragging(evt.mousePosition);
                    this.CaptureMouse();
                    evt.StopPropagation();
                    return;
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
        
        // 获取指定位置的节点
        private BehaviourNode GetNodeAtPosition(Vector2 mousePosition)
        {
            // 转换鼠标位置到内容容器的本地坐标
            Vector2 localPosition = mousePosition - _canvasOffset;
            
            // 遍历所有子元素，找到包含该点的节点
            foreach (var child in _contentContainer.Children())
            {
                if (child is BehaviourNode node && node.ContainsPoint(localPosition))
                {
                    return node;
                }
            }
            
            return null;
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
            rootNode.CanvasPosition = new Vector2(200, 50);
            rootNode.onPositionChanged = RefreshConnections; // 绑定位置变化回调
            _contentContainer.Add(rootNode);
            
            // Selector节点 - 有输入和输出端口
            var selectorNode = new BehaviourNode("Selector", "Select first successful child");
            selectorNode.CanvasPosition = new Vector2(150, 180);
            selectorNode.onPositionChanged = RefreshConnections;
            _contentContainer.Add(selectorNode);
            
            // Sequence节点 - 有输入和输出端口
            var sequenceNode = new BehaviourNode("Sequence", "Execute children in order");
            sequenceNode.CanvasPosition = new Vector2(250, 180);
            sequenceNode.onPositionChanged = RefreshConnections;
            _contentContainer.Add(sequenceNode);
            
            // Action节点 - 只有输入端口
            var actionNode1 = new BehaviourNode("Move To Target", "Move character to target position", true, false);
            actionNode1.CanvasPosition = new Vector2(100, 310);
            actionNode1.onPositionChanged = RefreshConnections;
            _contentContainer.Add(actionNode1);
            
            var actionNode2 = new BehaviourNode("Attack Enemy", "Perform attack on enemy target", true, false);
            actionNode2.CanvasPosition = new Vector2(200, 310);
            actionNode2.onPositionChanged = RefreshConnections;
            _contentContainer.Add(actionNode2);
            
            var actionNode3 = new BehaviourNode("Wait", "Wait for specified duration", true, false);
            actionNode3.CanvasPosition = new Vector2(300, 310);
            actionNode3.onPositionChanged = RefreshConnections;
            _contentContainer.Add(actionNode3);
            
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
        
        private Dictionary<string, BehaviourNode> _sampleNodes;
        
        private void CreateSampleConnections()
        {
            // 等待一帧确保节点位置已确定
            schedule.Execute(() =>
            {
                // Root 连接到 Selector
                var connection1 = new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["selector"].InputPort)
                {
                    ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection1);
                
                // Root 连接到 Sequence
                var connection2 = new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["sequence"].InputPort)
                {
                    ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection2);
                
                // Selector 连接到 Move To Target
                var connection3 = new NodeConnection(_sampleNodes["selector"].OutputPort, _sampleNodes["moveToTarget"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection3);
                
                // Selector 连接到 Attack Enemy
                var connection4 = new NodeConnection(_sampleNodes["selector"].OutputPort, _sampleNodes["attackEnemy"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection4);
                
                // Sequence 连接到 Wait
                var connection5 = new NodeConnection(_sampleNodes["sequence"].OutputPort, _sampleNodes["wait"].InputPort)
                {
                    ConnectionColor = new Color(0.8f, 0.3f, 0.3f, 1f),
                    LineWidth = 2f
                };
                AddConnection(connection5);
                
            }).ExecuteLater(100); // 延迟100ms执行
        }
    }
}
