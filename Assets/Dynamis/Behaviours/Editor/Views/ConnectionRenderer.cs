using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class ConnectionRenderer : VisualElement
    {
        private readonly List<Connection> _connections = new();
        
        // private bool _isDraggingConnection;
        // private Port _dragFromPort;
        // private Vector2 _dragEndPosition;
        
        public ConnectionRenderer()
        {
            name = "connection-renderer";
            
            // 设置为全屏覆盖，用于绘制连线
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            
            // 确保不会阻挡鼠标事件
            pickingMode = PickingMode.Ignore;
            
            // 注册绘制回调
            generateVisualContent += OnGenerateVisualContent;
        }
        
        // public void SetDragConnection(Port fromPort, Vector2 endPosition)
        // {
        //     _isDraggingConnection = true;
        //     _dragFromPort = fromPort;
        //     _dragEndPosition = endPosition;
        //     MarkDirtyRepaint();
        // }
        //
        // public void UpdateDragConnection(Vector2 endPosition)
        // {
        //     if (_isDraggingConnection)
        //     {
        //         _dragEndPosition = endPosition;
        //         MarkDirtyRepaint();
        //     }
        // }
        //
        // public void ClearDragConnection()
        // {
        //     _isDraggingConnection = false;
        //     _dragFromPort = null;
        //     MarkDirtyRepaint();
        // }
        
        public void AddConnection(Connection connection)
        {
            _connections.Add(connection);
            MarkDirtyRepaint();
        }
        
        public void RemoveConnection(Connection connection)
        {
            _connections.Remove(connection);
            MarkDirtyRepaint();
        }
        
        public void ClearConnections()
        {
            _connections.Clear();
            MarkDirtyRepaint();
        }
        
        public void RefreshConnections()
        {
            MarkDirtyRepaint();
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            
            // 绘制已有的连线
            foreach (var connection in _connections)
            {
                DrawConnection(painter, connection);
            }
            
            // 绘制悬浮中的连线
            // if (_isDraggingConnection && _dragFromPort != null)
            // {
            //     DrawDragConnection(painter, _dragFromPort.GetConnectionPoint(), _dragEndPosition);
            // }
        }
        
        // private void DrawDragConnection(Painter2D painter, Vector2 startPoint, Vector2 endPoint)
        // {
        //     var color = new Color(0.3f, 0.8f, 0.3f, 0.8f); // 半透明绿色
        //     var lineWidth = 2f;
        //     
        //     var arrowDirection = Vector2.up;
        //     var arrowSize = Mathf.Max(8f, lineWidth * 2f);
        //     DrawArrow(painter, arrowDirection, endPoint, arrowSize, color, lineWidth);
        //     endPoint.y -= arrowSize;
        //     
        //     // 计算贝塞尔曲线的切线长度
        //     var distance = Vector2.Distance(startPoint, endPoint);
        //     var tangentLength = Mathf.Max(30f, distance * 0.3f);
        //     
        //     // 计算切线点
        //     var startTangent = startPoint + Vector2.up * tangentLength;
        //     var endTangent = endPoint + Vector2.down * tangentLength;
        //     
        //     DrawBezierCurve(painter, startPoint, endPoint, startTangent, endTangent, color, lineWidth);
        // }
        
        private static void DrawConnection(Painter2D painter, Connection connection)
        {
            var startPoint = connection.GetStartPoint();
            var endPoint = connection.GetEndPoint();
            
            var arrowDirection = Vector2.up;
            var arrowSize = Mathf.Max(8f, connection.LineWidth * 2f);
            
            DrawArrow(painter, arrowDirection, endPoint, arrowSize, connection.ConnectionColor, connection.LineWidth);

            endPoint.y -= arrowSize;
            
            // 计算贝塞尔曲线的切线长度，基于连接距离
            var distance = Vector2.Distance(startPoint, endPoint);
            var tangentLength = Mathf.Max(30f, distance * 0.3f);
            
            // 计算切线点
            var startTangent = startPoint + Vector2.up * tangentLength;
            var endTangent = endPoint + Vector2.down * tangentLength;
            
            DrawBezierCurve(painter, startPoint, endPoint, startTangent, endTangent, connection.ConnectionColor, connection.LineWidth);
        }
        
        private static void DrawBezierCurve(Painter2D painter, Vector2 startPoint, Vector2 endPoint, 
            Vector2 startTangent, Vector2 endTangent, Color color, float lineWidth)
        {
            // 设置绘制样式
            painter.strokeColor = color;
            painter.lineWidth = lineWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            
            // 开始绘制路径
            painter.BeginPath();
            painter.MoveTo(startPoint);
            
            // 绘制贝塞尔曲线
            painter.BezierCurveTo(startTangent, endTangent, endPoint);
            
            // 执行描边
            painter.Stroke();
        }
        
        private static void DrawArrow(Painter2D painter, Vector2 direction, Vector2 endPoint, float arrowSize, Color color, float lineWidth)
        {
            var perpendicular = new Vector2(-direction.y, direction.x);
            
            var arrowTip = endPoint;
            var arrowLeft = endPoint - direction * arrowSize + perpendicular * (arrowSize * 0.5f);
            var arrowRight = endPoint - direction * arrowSize - perpendicular * (arrowSize * 0.5f);
            
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(arrowTip);
            painter.LineTo(arrowLeft);
            painter.LineTo(arrowRight);
            painter.ClosePath();
            painter.Fill();
        }
    }
}
