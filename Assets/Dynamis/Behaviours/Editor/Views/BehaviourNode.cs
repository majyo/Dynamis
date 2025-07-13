using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class BehaviourNode : VisualElement
    {
        private Label _nameLabel;
        private Label _descriptionLabel;
        
        // 拖拽相关字段
        private bool _isDragging;
        private Vector2 _startMousePosition;
        private Vector2 _startNodePosition;
        
        public string NodeName { get; set; }
        public string Description { get; set; }
        
        public BehaviourNode(string nodeName = "Node", string description = "Node Description")
        {
            NodeName = nodeName;
            Description = description;
            SetupNode();
            SetupDragging();
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
        
        private void SetupDragging()
        {
            // 注册拖拽相关的鼠标事件
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
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
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // 左键
            {
                _isDragging = true;
                _startMousePosition = evt.mousePosition;
                _startNodePosition = new Vector2(style.left.value.value, style.top.value.value);
                
                // 捕获鼠标，确保即使鼠标移出节点也能继续拖拽
                this.CaptureMouse();
                
                // 将节点置于最前显示
                BringNodeToFront();
                
                // 阻止事件传播，避免影响画布的其他交互
                evt.StopPropagation();
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                // 计算新位置
                var deltaPosition = evt.mousePosition - _startMousePosition;
                var newNodePosition = _startNodePosition + deltaPosition;
                
                // 限制节点不能拖拽到画布外部（可选）
                // newPosition = ClampToCanvas(newPosition);
                
                // 设置新位置
                SetPosition(newNodePosition);
                
                evt.StopPropagation();
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (_isDragging && evt.button == 0)
            {
                _isDragging = false;
                this.ReleaseMouse();
                
                // 触发位置变更事件（用于后续扩展，如撤销/重做）
                OnPositionChanged();
                
                evt.StopPropagation();
            }
        }
        
        private Vector2 ClampToCanvas(Vector2 position)
        {
            // 获取画布面板
            var canvas = GetFirstAncestorOfType<NodeCanvasPanel>();
            if (canvas == null) return position;
            
            // 获取画布和节点的尺寸
            var canvasRect = canvas.localBound;
            var nodeWidth = style.width.value.value;
            var nodeHeight = resolvedStyle.height;
            
            // 限制节点位置在画布范围内
            position.x = Mathf.Clamp(position.x, 0, canvasRect.width - nodeWidth);
            position.y = Mathf.Clamp(position.y, 0, canvasRect.height - nodeHeight);
            
            return position;
        }
        
        private void BringNodeToFront()
        {
            // 将节点移到父容器的最后，使其显示在最前面
            var parent = this.parent;
            if (parent != null)
            {
                parent.Remove(this);
                parent.Add(this);
            }
        }
        
        private void OnPositionChanged()
        {
            // 位置变更事件，可用于撤销/重做、自动保存等功能
            // 目前为空实现，后续可以扩展
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
        
        // 添加一个方法来获取当前是否正在拖拽
        public bool IsDragging => _isDragging;
        
        // 添加一个方法来获取拖拽开始位置
        public Vector2 StartNodePosition => _startNodePosition;
    }
}
