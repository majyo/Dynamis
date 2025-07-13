using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class ConnectionRenderer : VisualElement
    {
        private readonly List<NodeConnection> _connections = new();
        
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
        
        public void AddConnection(NodeConnection connection)
        {
            _connections.Add(connection);
            MarkDirtyRepaint();
        }
        
        public void RemoveConnection(NodeConnection connection)
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
            if (_connections.Count == 0)
                return;
                
            var painter = context.painter2D;
            
            foreach (var connection in _connections)
            {
                DrawConnection(painter, connection);
            }
        }
        
        private static void DrawConnection(Painter2D painter, NodeConnection connection)
        {
            // 获取贝塞尔曲线的关键点
            connection.GetBezierPoints(out var startPoint, out var endPoint, 
                                     out var startTangent, out var endTangent);
            
            // 设置绘制样式
            painter.strokeColor = connection.ConnectionColor;
            painter.lineWidth = connection.LineWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            
            // 开始绘制路径
            painter.BeginPath();
            painter.MoveTo(startPoint);
            
            // 绘制贝塞尔曲线
            painter.BezierCurveTo(startTangent, endTangent, endPoint);
            
            // 执行描边
            painter.Stroke();
            
            // 绘制箭头
            DrawArrow(painter, endTangent, endPoint, connection.ConnectionColor, connection.LineWidth);
        }
        
        private static void DrawArrow(Painter2D painter, Vector2 tangentPoint, Vector2 endPoint, Color color, float lineWidth)
        {
            // 计算箭头方向
            Vector2 direction = (endPoint - tangentPoint).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            
            // 箭头大小
            float arrowSize = Mathf.Max(8f, lineWidth * 2f);
            
            // 计算箭头的三个点
            Vector2 arrowTip = endPoint;
            Vector2 arrowLeft = endPoint - direction * arrowSize + perpendicular * (arrowSize * 0.5f);
            Vector2 arrowRight = endPoint - direction * arrowSize - perpendicular * (arrowSize * 0.5f);
            
            // 绘制箭头
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
