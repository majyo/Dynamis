using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Dynamis.Behaviours.Editor.Views;

namespace Dynamis.Behaviours.Editor.Events
{
    public class NodeCanvasEventHandler
    {
        private readonly NodeCanvasPanel _canvas;
        
        // 拖拽状态
        private bool _isDraggingNode;
        private bool _isDraggingCanvas;
        private BehaviourNode _draggedNode;
        private Vector2 _dragStartPosition;
        private Vector2 _lastMousePosition;

        public NodeCanvasEventHandler(NodeCanvasPanel canvas)
        {
            _canvas = canvas;
        }

        public void RegisterEvents()
        {
            // 注册鼠标事件
            _canvas.RegisterCallback<MouseDownEvent>(OnMouseDown);
            _canvas.RegisterCallback<MouseUpEvent>(OnMouseUp);
            _canvas.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _canvas.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            
            // 注册键盘事件
            _canvas.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            var mousePosition = evt.localMousePosition;
            var nodeAtPosition = _canvas.GetNodeAtPosition(mousePosition);

            if (evt.button == 0) // 左键
            {
                HandleLeftMouseDown(mousePosition, nodeAtPosition, evt.ctrlKey);
            }
            else if (evt.button == 1) // 右键
            {
                HandleRightMouseDown(mousePosition, nodeAtPosition);
            }

            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0) // 左键
            {
                HandleLeftMouseUp(evt.localMousePosition);
            }
            else if (evt.button == 1) // 右键
            {
                HandleRightMouseUp(evt.localMousePosition);
            }

            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            var mousePosition = evt.localMousePosition;
            var mouseDelta = mousePosition - _lastMousePosition;
            
            // 更新鼠标悬浮状态
            UpdateHoverState(mousePosition);

            // 处理拖拽
            if (_isDraggingNode)
            {
                HandleNodeDrag(mouseDelta);
            }
            else if (_isDraggingCanvas)
            {
                HandleCanvasDrag(mouseDelta);
            }
            else
            {
                // 检查是否应该开始画布拖拽（右键按住且移动距离超过阈值）
                if (evt.pressedButtons == 0b10) // 右键按住
                {
                    var dragDistance = Vector2.Distance(mousePosition, _dragStartPosition);
                    if (dragDistance > 2f) // 超过阈值，开始拖拽画布
                    {
                        var nodeAtStart = _canvas.GetNodeAtPosition(_dragStartPosition);
                        if (nodeAtStart == null) // 确保开始位置没有节点
                        {
                            _isDraggingCanvas = true;
                            _canvas.CaptureMouse();
                            _canvas.HideContextMenu();
                        }
                    }
                }
            }

            _lastMousePosition = mousePosition;
            evt.StopPropagation();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            // 清除悬浮状态
            _canvas.MouseHoveredNode = null;
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete)
            {
                // 删除选中的节点
                var selectedNodes = _canvas.SelectedNodes.ToArray();
                foreach (var node in selectedNodes)
                {
                    _canvas.RemoveNode(node);
                }
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                // 取消选择
                _canvas.ClearSelection();
                _canvas.HideContextMenu();
            }

            evt.StopPropagation();
        }

        private void HandleLeftMouseDown(Vector2 mousePosition, BehaviourNode nodeAtPosition, bool isCtrlPressed)
        {
            // 隐藏右键菜单
            _canvas.HideContextMenu();

            if (nodeAtPosition != null)
            {
                // 点击在节点上
                HandleNodeSelection(nodeAtPosition, isCtrlPressed);
                StartNodeDrag(nodeAtPosition, mousePosition);
            }
            else
            {
                // 点击在空白处
                if (!isCtrlPressed)
                {
                    _canvas.ClearSelection();
                }
            }

            _dragStartPosition = mousePosition;
            _lastMousePosition = mousePosition;
        }

        private void HandleLeftMouseUp(Vector2 _)
        {
            if (_isDraggingNode)
            {
                EndNodeDrag();
            }
            
            _isDraggingCanvas = false;
            _isDraggingNode = false;
            _draggedNode = null;
            
            _canvas.ReleaseMouse();
        }

        private void HandleRightMouseDown(Vector2 mousePosition, BehaviourNode nodeAtPosition)
        {
            // 设置右键点击位置，用于菜单显示
            _canvas.SetLastRightClickPosition(mousePosition);
            _dragStartPosition = mousePosition;
            _lastMousePosition = mousePosition;
            
            if (nodeAtPosition == null)
            {
                // 右键点击空白处，隐藏菜单但不立即开始拖拽
                _canvas.HideContextMenu();
                // 注意：这里不立即设置 _isDraggingCanvas = true，等到移动时再判断
            }
            else
            {
                // 右键点击节点，隐藏菜单
                _canvas.HideContextMenu();
            }
        }

        private void HandleRightMouseUp(Vector2 mousePosition)
        {
            if (_isDraggingCanvas)
            {
                // 结束画布拖拽
                _isDraggingCanvas = false;
                _canvas.ReleaseMouse();
            }
            else
            {
                // 计算拖拽距离
                var dragDistance = Vector2.Distance(mousePosition, _dragStartPosition);
                
                // 只有在拖拽距离很小且右键按下时没有点击到节点的情况下才显示菜单
                if (dragDistance < 5f)
                {
                    var nodeAtPosition = _canvas.GetNodeAtPosition(_dragStartPosition);
                    if (nodeAtPosition == null)
                    {
                        Debug.Log($"Showing context menu at position: {mousePosition}, drag distance: {dragDistance}");
                        _canvas.ShowContextMenu(mousePosition);
                    }
                }
            }
        }

        private void UpdateHoverState(Vector2 mousePosition)
        {
            if (!_isDraggingNode && !_isDraggingCanvas)
            {
                var nodeAtPosition = _canvas.GetNodeAtPosition(mousePosition);
                _canvas.MouseHoveredNode = nodeAtPosition;
            }
        }

        private void HandleNodeSelection(BehaviourNode node, bool isCtrlPressed)
        {
            if (isCtrlPressed)
            {
                // Ctrl+点击：切换选择状态
                if (_canvas.SelectedNodes.Contains(node))
                {
                    _canvas.RemoveFromSelection(node);
                }
                else
                {
                    _canvas.AddToSelection(node);
                }
            }
            else
            {
                // 普通点击：单选
                if (!_canvas.SelectedNodes.Contains(node))
                {
                    _canvas.ClearSelection();
                    _canvas.AddToSelection(node);
                }
            }
        }

        private void StartNodeDrag(BehaviourNode node, Vector2 _)
        {
            _isDraggingNode = true;
            _draggedNode = node;
            _canvas.CaptureMouse();

            // 如果拖拽的节点不在选择集合中，先选中它
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
                var newPosition = selectedNode.CanvasPosition + mouseDelta;
                selectedNode.CanvasPosition = newPosition;
            }

            // 刷新连线
            _canvas.RefreshConnections();
        }

        private void HandleCanvasDrag(Vector2 mouseDelta)
        {
            _canvas.MoveCanvas(mouseDelta);
        }

        private void EndNodeDrag()
        {
            _isDraggingNode = false;
            _draggedNode = null;
        }
    }
}