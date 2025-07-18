using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public abstract class EventHandler<T> where T : VisualElement
    {
        private T _target;

        public T Target
        {
            get => _target;
            set
            {
                if (_target != null)
                {
                    UnregisterCallbacks(_target);
                }

                _target = value;

                if (_target != null)
                {
                    RegisterCallbacks(_target);
                }
            }
        }

        protected abstract void RegisterCallbacks(T target);
        protected abstract void UnregisterCallbacks(T target);
    }
    
    public class NodeEventHandler : EventHandler<NodeCanvasPanel>
    {
        private const float SqrDragThreshold = 0.05f;
        
        private bool _dragRecording;
        private bool _dragged;
        private BehaviourNode _draggingNode;
        
        protected override void RegisterCallbacks(NodeCanvasPanel target)
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(HandleDeleteKeyDown);
        }
        
        protected override void UnregisterCallbacks(NodeCanvasPanel target)
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(HandleDeleteKeyDown);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            var hoveredNode = Target.GetNodeAtPosition(evt.localMousePosition);
            Target.SetHoveredNode(hoveredNode);
            
            if (hoveredNode == null)
            {
                return;
            }
            
            _dragRecording = true;
            _draggingNode = hoveredNode;
            _draggingNode.StartDragging(evt.localMousePosition);
            Target.SetSelectedNode(hoveredNode);
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            var hoveredNode = Target.GetNodeAtPosition(evt.localMousePosition);
            Target.SetHoveredNode(hoveredNode);
            
            if (!_dragRecording)
            {
                return;
            }

            if (_draggingNode != null)
            {
                _draggingNode.UpdateDragging(evt.localMousePosition);
                evt.StopPropagation();
            }

            if (evt.mouseDelta.sqrMagnitude > SqrDragThreshold)
            {
                _dragged = true;
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }
            
            _dragRecording = false;
            _draggingNode?.EndDragging();
            _draggingNode = null;
            
            if (_dragged)
            {
                _dragged = false;
                evt.StopPropagation();
            }
        }
        
        private void HandleDeleteKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Delete)
            {
                return;
            }
            
            Target.RemoveNode(Target.GetSelectedNode());
            
            evt.StopPropagation();
        }
    }

    public class DragCanvasHandler : EventHandler<NodeCanvasPanel>
    {
        private const float SqrDragThreshold = 0.05f;
        
        private bool _dragRecording;
        private bool _dragged;
        
        protected override void RegisterCallbacks(NodeCanvasPanel target)
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        protected override void UnregisterCallbacks(NodeCanvasPanel target)
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 1)
            {
                return;
            }
            
            _dragRecording = true;
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_dragRecording)
            {
                return;
            }
            
            Target.MoveCanvas(evt.mouseDelta);

            if (evt.mouseDelta.sqrMagnitude > SqrDragThreshold)
            {
                _dragged = true;
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button != 1)
            {
                return;
            }
            
            _dragRecording = false;
            
            if (_dragged)
            {
                _dragged = false;
                evt.StopPropagation();
            }
        }
    }
}