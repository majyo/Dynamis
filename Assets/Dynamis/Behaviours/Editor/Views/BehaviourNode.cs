using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class BehaviourNode : VisualElement
    {
        private Label _nameLabel;
        private Label _descriptionLabel;
        
        // 端口相关字段
        private Port _inputPort;
        private Port _outputPort;
        
        // 状态相关字段
        private bool _isHovered;
        private bool _isSelected;
        
        // 连线更新回调
        public System.Action onPositionChanged;
        
        public string NodeName { get; set; }
        public string Description { get; set; }
        public Port InputPort => _inputPort;
        public Port OutputPort => _outputPort;

        // 悬浮状态属性
        public bool IsHovered 
        { 
            get => _isHovered; 
            set 
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    UpdateNodeStyles();
                }
            }
        }

        // 选中状态属性
        public bool IsSelected 
        { 
            get => _isSelected; 
            set 
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    UpdateNodeStyles();
                }
            }
        }

        // 基于transform的节点位置计算，用于替代基于style的计算，尚不确定是否会引入布局问题
        public Vector2 CanvasPosition
        {
            get => new(transform.position.x, transform.position.y);
            set
            {
                transform.position = new Vector3(value.x, value.y, 0);
                onPositionChanged?.Invoke();
            }
        }

        public Vector2 CanvasSize => new(resolvedStyle.width, resolvedStyle.height);

        public BehaviourNode(string nodeName = "Node", string description = "Node Description", bool hasInput = true, bool hasOutput = true)
        {
            NodeName = nodeName;
            Description = description;
            SetupNode();
            SetupPorts(hasInput, hasOutput);
        }

        public void Move(Vector2 delta)
        {
            CanvasPosition += delta;
                
            // 通知端口更新位置
            _inputPort?.UpdateWorldPosition();
            _outputPort?.UpdateWorldPosition();
                
            // 通知连线更新
            onPositionChanged?.Invoke();
        }
        
        // 检查点是否在节点内
        public new bool ContainsPoint(Vector2 point)
        {
            var rect = new Rect(CanvasPosition.x, CanvasPosition.y, CanvasSize.x, CanvasSize.y);
            return rect.Contains(point);
        }
        
        public void SetPosition(Vector2 position)
        {
            CanvasPosition = position;
        }
        
        public void UpdateContent()
        {
            _nameLabel.text = NodeName;
            _descriptionLabel.text = Description;
        }
        
        private void SetupNode()
        {
            // 设置节点基本样式
            name = "behaviour-node";
            
            style.position = Position.Absolute;
            style.width = 180;
            style.minHeight = 80;
            style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f); // 深灰色背景
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f); // 边框颜色
            style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderTopLeftRadius = 8;
            style.borderTopRightRadius = 8;
            style.borderBottomLeftRadius = 8;
            style.borderBottomRightRadius = 8;
            
            // 创建头部区域（类似UE4的蓝色头部）
            var header = new VisualElement
            {
                name = "node-header",
                style = {
                    backgroundColor = new Color(0.1f, 0.3f, 0.6f, 1f), // 蓝色头部
                    height = 30,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center
                }
            };
            
            // 节点名称标签
            _nameLabel = new Label(NodeName)
            {
                name = "node-name-label",
                style = {
                    color = Color.white,
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            header.Add(_nameLabel);
            Add(header);
            
            // 创建内容区域
            var content = new VisualElement
            {
                name = "node-content",
                style = {
                    flexGrow = 1,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 12,
                    paddingRight = 12
                }
            };
            
            // 描述标签
            _descriptionLabel = new Label(Description)
            {
                name = "node-description-label",
                style = {
                    color = new Color(0.8f, 0.8f, 0.8f, 1f),
                    fontSize = 10,
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperLeft
                }
            };
            content.Add(_descriptionLabel);
            Add(content);
        }
        
        private void SetupPorts(bool hasInput, bool hasOutput)
        {
            if (hasInput)
            {
                _inputPort = new Port(PortType.Input, this);
                Add(_inputPort);
            }
            
            if (hasOutput)
            {
                _outputPort = new Port(PortType.Output, this);
                Add(_outputPort);
            }
        }

        private void UpdateNodeStyles()
        {
            // 根据四种状态组合设置不同的边框样式，保持背景颜色不变用于区分节点种类
            if (_isSelected && _isHovered)
            {
                // 选中且悬浮：金黄色粗边框 + 发光效果
                style.borderTopColor = new Color(1f, 0.8f, 0.3f, 1f);
                style.borderBottomColor = new Color(1f, 0.8f, 0.3f, 1f);
                style.borderLeftColor = new Color(1f, 0.8f, 0.3f, 1f);
                style.borderRightColor = new Color(1f, 0.8f, 0.3f, 1f);
                style.borderTopWidth = 4;
                style.borderBottomWidth = 4;
                style.borderLeftWidth = 4;
                style.borderRightWidth = 4;
            }
            else if (_isSelected)
            {
                // 仅选中：橙色粗边框
                style.borderTopColor = new Color(1f, 0.6f, 0.2f, 1f);
                style.borderBottomColor = new Color(1f, 0.6f, 0.2f, 1f);
                style.borderLeftColor = new Color(1f, 0.6f, 0.2f, 1f);
                style.borderRightColor = new Color(1f, 0.6f, 0.2f, 1f);
                style.borderTopWidth = 3;
                style.borderBottomWidth = 3;
                style.borderLeftWidth = 3;
                style.borderRightWidth = 3;
            }
            else if (_isHovered)
            {
                // 仅悬浮：白色中等边框
                style.borderTopColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                style.borderBottomColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                style.borderLeftColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                style.borderRightColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                style.borderTopWidth = 2;
                style.borderBottomWidth = 2;
                style.borderLeftWidth = 2;
                style.borderRightWidth = 2;
            }
            else
            {
                // 默认状态：原始边框样式
                style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                style.borderTopWidth = 2;
                style.borderBottomWidth = 2;
                style.borderLeftWidth = 2;
                style.borderRightWidth = 2;
            }
        }
    }
}
