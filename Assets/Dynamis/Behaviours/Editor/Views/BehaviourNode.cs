using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class BehaviourNode : VisualElement
    {
        private Label _nameLabel;
        private Label _descriptionLabel;
        
        public string NodeName { get; set; }
        public string Description { get; set; }
        
        public BehaviourNode(string nodeName = "Node", string description = "Node Description")
        {
            NodeName = nodeName;
            Description = description;
            SetupNode();
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
                style =
                {
                    backgroundColor = new Color(0.2f, 0.4f, 0.8f, 1f), // 蓝色头部
                    height = 28,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4,
                    justifyContent = Justify.Center
                }
            };
            
            // 节点名称标签
            _nameLabel = new Label(NodeName)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 12,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis
                }
            };
            header.Add(_nameLabel);
            
            // 创建内容区域
            var content = new VisualElement
            {
                name = "node-content",
                style =
                {
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 6,
                    paddingBottom = 6,
                    flexGrow = 1,
                    justifyContent = Justify.Center
                }
            };
            
            // 描述标签
            _descriptionLabel = new Label(Description)
            {
                style =
                {
                    color = new Color(0.8f, 0.8f, 0.8f, 1f), // 浅灰色文字
                    fontSize = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    whiteSpace = WhiteSpace.Normal, // 允许换行
                    overflow = Overflow.Hidden
                }
            };
            content.Add(_descriptionLabel);
            
            // 添加连接点（输入和输出）
            CreateConnectionPoints();
            
            Add(header);
            Add(content);
            
            // 添加悬停效果
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        
        private void CreateConnectionPoints()
        {
            // 输入连接点（顶部）
            var inputPoint = new VisualElement
            {
                name = "input-connection",
                style =
                {
                    position = Position.Absolute,
                    top = -6,
                    left = new Length(50, LengthUnit.Percent),
                    marginLeft = -6,
                    width = 12,
                    height = 12,
                    backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f)
                }
            };
            
            // 输出连接点（底部）
            var outputPoint = new VisualElement
            {
                name = "output-connection",
                style =
                {
                    position = Position.Absolute,
                    bottom = -6,
                    left = new Length(50, LengthUnit.Percent),
                    marginLeft = -6,
                    width = 12,
                    height = 12,
                    backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f),
                    borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f)
                }
            };
            
            Add(inputPoint);
            Add(outputPoint);
        }
        
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            style.borderTopColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            style.borderBottomColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            style.borderLeftColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            style.borderRightColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        }
        
        public void UpdateNodeContent(string nodeName, string description)
        {
            NodeName = nodeName;
            Description = description;
            _nameLabel.text = nodeName;
            _descriptionLabel.text = description;
        }
        
        public void SetPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }
    }
}
