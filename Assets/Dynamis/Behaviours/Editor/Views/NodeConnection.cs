using UnityEngine;

namespace Dynamis.Behaviours.Editor.Views
{
    public class NodeConnection
    {
        public enum DirectionType
        {
            Horizontal,
            Vertical,
        }
        
        public Port OutputPort { get; private set; }
        public Port InputPort { get; private set; }
        public Color ConnectionColor { get; set; } = Color.white;
        public float LineWidth { get; set; } = 2f;
        public DirectionType Direction { get; set; } = DirectionType.Vertical;
        
        public NodeConnection(Port outputPort, Port inputPort)
        {
            OutputPort = outputPort;
            InputPort = inputPort;
        }
        
        public Vector2 GetStartPoint()
        {
            return OutputPort.GetConnectionPoint();
        }
        
        public Vector2 GetEndPoint()
        {
            return InputPort.GetConnectionPoint();
        }
    }
}
