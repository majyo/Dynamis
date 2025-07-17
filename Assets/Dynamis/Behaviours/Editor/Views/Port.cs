using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public enum PortType
    {
        Input,
        Output
    }
    
    public class Port : VisualElement
    {
        public PortType Type { get; private set; }
        public BehaviourNode ParentNode { get; private set; }
        public Vector2 WorldPosition { get; private set; }
        
        private VisualElement _portCircle;
        
        public Port(PortType type, BehaviourNode parentNode)
        {
            Type = type;
            ParentNode = parentNode;
            SetupPort();
        }
        
        private void SetupPort()
        {
            name = $"port-{Type.ToString().ToLower()}";
            
            // 设置端口容器样式
            style.width = 16;
            style.height = 16;
            style.position = Position.Absolute;
            
            // 根据类型设置位置 - 上下排布
            if (Type == PortType.Input)
            {
                // 输入端口在节点顶部中央
                style.left = Length.Percent(50); // 水平居中
                style.marginLeft = -8; // 向左偏移自身宽度的一半以实现真正居中
                style.top = -8; // 输入端口在节点顶部
            }
            else
            {
                // 输出端口在节点底部中央
                style.left = Length.Percent(50); // 水平居中
                style.marginLeft = -8; // 向左偏移自身宽度的一半以实现真正居中
                style.bottom = -8; // 输出端口在节点底部
            }
            
            // 创建端口圆形
            _portCircle = new VisualElement
            {
                name = "port-circle",
                style = {
                    width = 12,
                    height = 12,
                    backgroundColor = Type == PortType.Input ? new Color(0.2f, 0.7f, 0.2f, 1f) : new Color(0.7f, 0.2f, 0.2f, 1f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = Color.white,
                    borderBottomColor = Color.white,
                    borderLeftColor = Color.white,
                    borderRightColor = Color.white,
                    position = Position.Absolute,
                    left = 2,
                    top = 2
                }
            };
            
            Add(_portCircle);
            
            // 注册回调以更新世界位置
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateWorldPosition();
        }
        
        public void UpdateWorldPosition()
        {
            if (ParentNode != null && parent != null)
            {
                // var localCenter = new Vector2(8, 8); // 端口中心点
                // var nodeLocalCenter = ParentNode.LocalToWorld(localCenter + ParentNode.CanvasPosition);
                
                // 根据端口类型调整位置
                if (Type == PortType.Input)
                {
                    // WorldPosition = new Vector2(ParentNode.CanvasPosition.x, ParentNode.CanvasPosition.y + 20);
                    WorldPosition = new Vector2(ParentNode.CanvasPosition.x + ParentNode.CanvasSize.x * 0.5f, ParentNode.CanvasPosition.y - 8);
                }
                else
                {
                    // WorldPosition = new Vector2(ParentNode.CanvasPosition.x + 180, ParentNode.CanvasPosition.y + 20);
                    WorldPosition = new Vector2(ParentNode.CanvasPosition.x + ParentNode.CanvasSize.x * 0.5f, ParentNode.CanvasPosition.y + ParentNode.CanvasSize.y + 8);
                }
            }
        }
        
        public Vector2 GetConnectionPoint()
        {
            UpdateWorldPosition();
            return WorldPosition;
        }
    }
}
