using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Dynamis.Behaviours.Editor.Views;

namespace Dynamis.Behaviours.Editor.Events
{
    public class NodeCanvasEventHandler
    {
        private readonly NodeCanvasPanel _canvas;
        
        // 拖拽相关常量
        private const float DragThreshold = 5f;
        private const float SnapThreshold = 30f; // 新增：吸附阈值
        private const int RightMouseButton = 1;
        private const int LeftMouseButton = 0;
        
        // 拖拽状态
        private bool _isDraggingNode;
        private bool _isDraggingCanvas;
        private bool _isDraggingConnection; // 新增：连线拖拽状态
        private NodeElement _draggedNode;
        private Vector2 _dragStartPosition;
        private Vector2 _lastMousePosition;
        
        // 右键状态追踪
        private bool _rightMousePressed;
        
        // 连线拖拽相关
        private Port _draggingFromPort;
        private Port _snapTargetPort; // 新增：当前吸附的目标端口

        public NodeCanvasEventHandler(NodeCanvasPanel canvas)
        {
            _canvas = canvas;
        }

        public void RegisterEvents()
        {
            _canvas.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _canvas.RegisterCallback<MouseUpEvent>(OnMouseUp);
            _canvas.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _canvas.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            _canvas.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        #region 主要事件处理

        private void OnMouseDown(MouseDownEvent evt)
        {
            var mousePosition = evt.localMousePosition;
            
            // 检查是否点击在端口上
            var portAtPosition = GetPortAtPosition(mousePosition);
            var nodeAtPosition = _canvas.GetNodeAtPosition(mousePosition);
            
            // 初始化拖拽起始位置
            _dragStartPosition = mousePosition;
            _lastMousePosition = mousePosition;
            
            Debug.Log($"OnMouseDown at {mousePosition}, Port: {portAtPosition?.name}, Node: {nodeAtPosition?.name}");

            switch (evt.button)
            {
                case LeftMouseButton:
                    HandleLeftMouseDown(nodeAtPosition, portAtPosition, evt.altKey);
                    break;
                case RightMouseButton:
                    HandleRightMouseDown(mousePosition, nodeAtPosition);
                    break;
            }

            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            switch (evt.button)
            {
                case LeftMouseButton:
                    HandleLeftMouseUp(evt.localMousePosition);
                    break;
                case RightMouseButton:
                    HandleRightMouseUp(evt.localMousePosition);
                    break;
            }

            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            var mousePosition = evt.localMousePosition;
            var mouseDelta = mousePosition - _lastMousePosition;
            
            UpdateHoverState(mousePosition);
            HandleDragOperations(evt, mousePosition, mouseDelta);
            
            _lastMousePosition = mousePosition;
            evt.StopPropagation();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _canvas.MouseHoveredNode = null;
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Delete:
                    DeleteSelectedNodes();
                    break;
                case KeyCode.Escape:
                    CancelOperations();
                    break;
            }

            evt.StopPropagation();
        }

        #endregion

        #region 左键操作处理

        private void HandleLeftMouseDown(NodeElement nodeAtPosition, Port portAtPosition, bool isAltPressed)
        {
            _canvas.HideContextMenu();

            // 优先处理端口点击
            if (portAtPosition != null)
            {
                HandlePortClick(portAtPosition, isAltPressed);
            }
            else if (nodeAtPosition != null)
            {
                HandleNodeSelection(nodeAtPosition, isAltPressed);
                StartNodeDrag(nodeAtPosition);
            }
            else
            {
                HandleEmptyAreaClick(isAltPressed);
            }
        }

        private void HandleLeftMouseUp(Vector2 mousePosition)
        {
            if (_isDraggingConnection)
            {
                EndConnectionDrag(mousePosition);
            }
            else if (_isDraggingNode)
            {
                EndNodeDrag();
            }
            
            ResetDragStates();
            _canvas.ReleaseMouse();
        }

        private void HandlePortClick(Port port, bool isAltPressed)
        {
            if (isAltPressed)
            {
                // Alt+点击端口：删除该端口的所有连线
                _canvas.RemovePortConnections(port);
            }
            else if (port.Type == PortType.Output)
            {
                // 左键点击输出端口：开始拖拽连线
                StartConnectionDrag(port);
            }
            // 输入端口的左键点击暂时不处理，可以后续扩展
        }

        private void HandleNodeSelection(NodeElement node, bool isCtrlPressed)
        {
            if (isCtrlPressed)
            {
                ToggleNodeSelection(node);
            }
            else
            {
                SelectSingleNode(node);
            }
        }

        private void ToggleNodeSelection(NodeElement node)
        {
            if (_canvas.SelectedNodes.Contains(node))
            {
                _canvas.RemoveFromSelection(node);
            }
            else
            {
                _canvas.AddToSelection(node);
            }
        }

        private void SelectSingleNode(NodeElement node)
        {
            if (!_canvas.SelectedNodes.Contains(node))
            {
                _canvas.ClearSelection();
                _canvas.AddToSelection(node);
            }
        }

        private void HandleEmptyAreaClick(bool isCtrlPressed)
        {
            if (!isCtrlPressed)
            {
                _canvas.ClearSelection();
            }
        }

        #endregion

        #region 右键操作处理

        private void HandleRightMouseDown(Vector2 mousePosition, NodeElement _)
        {
            _canvas.SetLastRightClickPosition(mousePosition);
            _rightMousePressed = true;
            _canvas.HideContextMenu();
        }

        private void HandleRightMouseUp(Vector2 mousePosition)
        {
            _rightMousePressed = false;
            
            if (_isDraggingCanvas)
            {
                EndCanvasDrag();
            }
            else
            {
                TryShowContextMenu(mousePosition);
            }
        }

        private void TryShowContextMenu(Vector2 mousePosition)
        {
            var dragDistance = Vector2.Distance(mousePosition, _dragStartPosition);
            
            if (dragDistance < DragThreshold)
            {
                var nodeAtStartPosition = _canvas.GetNodeAtPosition(_dragStartPosition);
                if (nodeAtStartPosition == null)
                {
                    _canvas.ShowContextMenu(mousePosition);
                }
            }
        }

        #endregion

        #region 连线拖拽处理

        private void StartConnectionDrag(Port fromPort)
        {
            _isDraggingConnection = true;
            _draggingFromPort = fromPort;
            _snapTargetPort = null; // 重置吸附目标
            _canvas.CaptureMouse();
            
            // 通知canvas开始连线拖拽
            _canvas.OnPortPressed(fromPort, false);
        }

        private void HandleConnectionDrag(Vector2 mousePosition)
        {
            if (_isDraggingConnection && _draggingFromPort != null)
            {
                // 检查是否有可吸附的端口
                var snapTarget = FindSnapTarget(mousePosition);
                
                if (snapTarget != _snapTargetPort)
                {
                    // 更新吸附目标
                    UpdateSnapTarget(snapTarget);
                }

                // 更新虚拟连线的终点位置
                UpdateDraggingConnection(mousePosition, snapTarget);
            }
        }

        private void EndConnectionDrag(Vector2 mousePosition)
        {
            if (!_isDraggingConnection || _draggingFromPort == null)
            {
                return;
            }

            // 优先使用吸附目标，如果没有则检查鼠标位置的端口
            var targetPort = _snapTargetPort ?? GetPortAtPosition(mousePosition);
            
            if (IsValidConnectionTarget(targetPort))
            {
                // 创建实际连线
                CreateConnection(_draggingFromPort, targetPort);
            }
            
            // 清理拖拽状态
            CleanupConnectionDrag();
        }

        private Port FindSnapTarget(Vector2 mousePosition)
        {
            var canvasPosition = mousePosition - _canvas.GetCanvasOffset();
            
            Port closestPort = null;
            float closestDistance = float.MaxValue;
            
            // 遍历所有节点的输入端口，寻找最近的可连接端口
            foreach (var node in _canvas.GetAllNodes())
            {
                if (node.InputPort != null)
                {
                    var port = node.InputPort;
                    
                    // 检查是否是有效的连接目标
                    if (IsValidConnectionTarget(port))
                    {
                        var portPosition = port.GetConnectionPoint();
                        var distance = Vector2.Distance(canvasPosition, portPosition);
                        
                        // 在吸附范围内且距离最近
                        if (distance <= SnapThreshold && distance < closestDistance)
                        {
                            closestPort = port;
                            closestDistance = distance;
                        }
                    }
                }
            }
            
            return closestPort;
        }

        private void UpdateSnapTarget(Port newSnapTarget)
        {
            // 清除之前的吸附高亮
            if (_snapTargetPort != null)
            {
                SetPortSnapHighlight(_snapTargetPort, false);
            }
            
            // 设置新的吸附目标
            _snapTargetPort = newSnapTarget;
            
            // 高亮新的吸附目标
            if (_snapTargetPort != null)
            {
                SetPortSnapHighlight(_snapTargetPort, true);
            }
        }

        private void SetPortSnapHighlight(Port port, bool highlight)
        {
            // 通知canvas设置端口的吸附高亮状态
            _canvas.SetPortSnapHighlight(port, highlight);
        }

        private void UpdateDraggingConnection(Vector2 mousePosition, Port snapTarget)
        {
            Vector2 targetPosition;
            
            if (snapTarget != null)
            {
                // 如果有吸附目标，使用目标端口的位置
                targetPosition = snapTarget.GetConnectionPoint();
            }
            else
            {
                // 否则使用鼠标位置
                targetPosition = mousePosition - _canvas.GetCanvasOffset();
            }
            
            // 通知canvas更新虚拟连线终点
            _canvas.UpdateDraggingConnectionEndPoint(targetPosition);
        }

        private bool IsValidConnectionTarget(Port targetPort)
        {
            if (targetPort == null || _draggingFromPort == null)
                return false;
                
            // 检查端口类型：输出端口只能连接到输入端口
            if (targetPort.Type != PortType.Input)
                return false;
                
            // 检查是否是同一个节点的端口
            if (targetPort.ParentNode == _draggingFromPort.ParentNode)
                return false;
                
            // 检查输入端口是否已经有连线（通常输入端口只能有一个连线）
            if (_canvas.HasConnectionToPort(targetPort))
                return false;
                
            return true;
        }

        private void CreateConnection(Port outputPort, Port inputPort)
        {
            var connection = new Connection(outputPort, inputPort);
            _canvas.AddConnection(connection);
        }

        private void CleanupConnectionDrag()
        {
            // 清除吸附高亮
            if (_snapTargetPort != null)
            {
                SetPortSnapHighlight(_snapTargetPort, false);
                _snapTargetPort = null;
            }
            
            _isDraggingConnection = false;
            _draggingFromPort = null;
            
            // 通知canvas清理虚拟连线
            _canvas.ClearDraggingConnection();
        }

        #endregion

        #region 拖拽操作处理

        private void HandleDragOperations(MouseMoveEvent evt, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (_isDraggingConnection)
            {
                HandleConnectionDrag(mousePosition);
            }
            else if (_isDraggingNode)
            {
                HandleNodeDrag(mouseDelta);
            }
            else if (_isDraggingCanvas)
            {
                HandleCanvasDrag(mouseDelta);
            }
            else
            {
                CheckForCanvasDragStart(evt, mousePosition);
            }
        }

        private void CheckForCanvasDragStart(MouseMoveEvent evt, Vector2 mousePosition)
        {
            // 检查右键是否按住且移动距离超过阈值
            if (_rightMousePressed && IsRightMouseButtonPressed(evt))
            {
                var dragDistance = Vector2.Distance(mousePosition, _dragStartPosition);
                if (dragDistance > DragThreshold)
                {
                    var nodeAtStart = _canvas.GetNodeAtPosition(_dragStartPosition);
                    if (nodeAtStart == null)
                    {
                        StartCanvasDrag();
                    }
                }
            }
        }

        private bool IsRightMouseButtonPressed(MouseMoveEvent evt)
        {
            return (evt.pressedButtons & 2) == 2; // 检查右键位
        }

        private void StartNodeDrag(NodeElement node)
        {
            _isDraggingNode = true;
            _draggedNode = node;
            _canvas.CaptureMouse();

            // 确保拖拽的节点被选中
            if (!_canvas.SelectedNodes.Contains(node))
            {
                _canvas.ClearSelection();
                _canvas.AddToSelection(node);
            }
        }

        private void HandleNodeDrag(Vector2 mouseDelta)
        {
            if (_draggedNode == null) return;

            // 移动所有选中的节点
            foreach (var selectedNode in _canvas.SelectedNodes)
            {
                selectedNode.CanvasPosition += mouseDelta;
            }

            _canvas.RefreshConnections();
        }

        private void EndNodeDrag()
        {
            _isDraggingNode = false;
            _draggedNode = null;
        }

        private void StartCanvasDrag()
        {
            _isDraggingCanvas = true;
            _canvas.CaptureMouse();
            _canvas.HideContextMenu();
        }

        private void HandleCanvasDrag(Vector2 mouseDelta)
        {
            _canvas.MoveCanvas(mouseDelta);
        }

        private void EndCanvasDrag()
        {
            _isDraggingCanvas = false;
            _canvas.ReleaseMouse();
        }

        #endregion

        #region 辅助方法

        private void UpdateHoverState(Vector2 mousePosition)
        {
            if (!_isDraggingNode && !_isDraggingCanvas)
            {
                var nodeAtPosition = _canvas.GetNodeAtPosition(mousePosition);
                _canvas.MouseHoveredNode = nodeAtPosition;
            }
        }

        private void DeleteSelectedNodes()
        {
            var selectedNodes = _canvas.SelectedNodes.ToArray();
            foreach (var node in selectedNodes)
            {
                _canvas.RemoveNode(node);
            }
        }

        private void CancelOperations()
        {
            _canvas.ClearSelection();
            _canvas.HideContextMenu();
        }

        private Port GetPortAtPosition(Vector2 mousePosition)
        {
            // 转换鼠标位置到画布坐标系
            var canvasPosition = mousePosition - _canvas.GetCanvasOffset();
            
            // 遍历所有节点，检查端口
            foreach (var node in _canvas.GetAllNodes())
            {
                // 检查输入端口
                if (node.InputPort != null && IsPointInPort(node.InputPort, canvasPosition))
                {
                    return node.InputPort;
                }

                // 检查输出端口
                if (node.OutputPort != null && IsPointInPort(node.OutputPort, canvasPosition))
                {
                    return node.OutputPort;
                }
            }

            return null;
        }

        private bool IsPointInPort(Port port, Vector2 position)
        {
            // var portWorldPos = port.GetConnectionPoint();
            // var portRect = new Rect(portWorldPos.x - 8, portWorldPos.y - 8, 16, 16);
            return port.GetBoundingBox().Contains(position);
            // return portRect.Contains(position);
        }

        private void ResetDragStates()
        {
            // 清除吸附状态
            if (_snapTargetPort != null)
            {
                SetPortSnapHighlight(_snapTargetPort, false);
                _snapTargetPort = null;
            }
            
            _isDraggingCanvas = false;
            _isDraggingNode = false;
            _isDraggingConnection = false;
            _draggedNode = null;
            _draggingFromPort = null;
            _rightMousePressed = false;
        }

        #endregion
    }
}

