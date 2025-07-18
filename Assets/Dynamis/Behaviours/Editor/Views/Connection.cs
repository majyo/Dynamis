using UnityEngine;

namespace Dynamis.Behaviours.Editor.Views
{
    public class Connection
    {
        public enum DirectionType
        {
            Horizontal,
            Vertical,
        }
        
        public IEndPoint OutputPort { get; private set; }
        public IEndPoint InputPort { get; private set; }
        public Color ConnectionColor { get; set; } = Color.white;
        public float LineWidth { get; set; } = 2f;
        public DirectionType Direction { get; set; } = DirectionType.Vertical;
        
        public Connection(Port outputPort, Port inputPort)
        {
            OutputPort = outputPort;
            InputPort = inputPort;
        }
        
        public Vector2 GetStartPoint()
        {
            return OutputPort.Position;
        }
        
        public Vector2 GetEndPoint()
        {
            return InputPort.Position;
        }
    }
}
