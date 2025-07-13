using UnityEngine;

namespace Dynamis.Behaviours.Editor.Views
{
    public class NodeConnection
    {
        public Port OutputPort { get; private set; }
        public Port InputPort { get; private set; }
        public Color ConnectionColor { get; set; } = Color.white;
        public float LineWidth { get; set; } = 2f;
        
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
        
        // 计算贝塞尔曲线的控制点
        public void GetBezierPoints(out Vector2 startPoint, out Vector2 endPoint, 
                                   out Vector2 startTangent, out Vector2 endTangent)
        {
            startPoint = GetStartPoint();
            endPoint = GetEndPoint();
            
            // 计算贝塞尔曲线的切线长度，基于连接距离
            float distance = Vector2.Distance(startPoint, endPoint);
            float tangentLength = Mathf.Max(50f, distance * 0.3f);
            
            // 输出端口向右，输入端口向左
            startTangent = startPoint + Vector2.right * tangentLength;
            endTangent = endPoint + Vector2.left * tangentLength;
        }
    }
}
