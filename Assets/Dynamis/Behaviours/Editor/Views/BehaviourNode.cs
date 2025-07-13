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
        
        // 端口相关字段
        private Port _inputPort;
        private Port _outputPort;
        
        // 连线更新回调
        public System.Action OnPositionChanged;
        
        public string NodeName { get; set; }
        public string Description { get; set; }
        public Port InputPort => _inputPort;
        public Port OutputPort => _outputPort;

        public Vector2 CanvasPosition
        {
            get => new(style.left.value.value, style.top.value.value);
            set
            {
                style.left = value.x;
                style.top = value.y;
            }
        }
        
        public BehaviourNode(string nodeName = "Node", string description = "Node Description", bool hasInput = true, bool hasOutput = true)
        {
            NodeName = nodeName;
            Description = description;
            SetupNode();
            SetupPorts(hasInput, hasOutput);
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
        
        private void SetupDragging()
        {
            // 注册鼠标事件
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            // 只响应左键
            if (evt.button == 0)
            {
                _isDragging = true;
                // _startMousePosition = evt.localMousePosition;
                _startMousePosition = evt.mousePosition;
                _startNodePosition = CanvasPosition;
                this.CaptureMouse();
                evt.StopPropagation();
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                // Vector2 mouseDelta = evt.localMousePosition - _startMousePosition;
                Vector2 mouseDelta = evt.mousePosition - _startMousePosition;
                CanvasPosition = _startNodePosition + mouseDelta;
                
                // 通知端口更新位置
                _inputPort?.UpdateWorldPosition();
                _outputPort?.UpdateWorldPosition();
                
                // 通知连线更新
                OnPositionChanged?.Invoke();
                
                evt.StopPropagation();
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (_isDragging && evt.button == 0)
            {
                _isDragging = false;
                this.ReleaseMouse();
                evt.StopPropagation();
            }
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
    }
}
