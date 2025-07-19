using Dynamis.Behaviours.Editor.Views;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Events
{
    public class NodeCanvasEventHandler
    {
        public NodeCanvasPanel Context { get; private set; }
        
        public NodeCanvasEventHandler(NodeCanvasPanel context)
        {
            Context = context;
        }
        
        public void RegisterEvents()
        {
            Context.RegisterCallback<MouseDownEvent>(OnMouseDown);
            Context.RegisterCallback<MouseUpEvent>(OnMouseUp);
            Context.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            Context.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }
        
        public void UnregisterEvents()
        {
            Context.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            Context.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            Context.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            Context.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
        }
        
        private void OnKeyDown(KeyDownEvent evt)
        {
        }
    }
}