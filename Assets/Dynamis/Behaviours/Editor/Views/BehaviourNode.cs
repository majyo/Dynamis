using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class BehaviourNode : VisualElement
    {
        private Label _nameLabel;
        private Label _descriptionLabel;
        
        // Port related fields
        private Port _inputPort;
        private Port _outputPort;
        
        // State related fields
        private bool _isHovered;
        private bool _isSelected;
        
        // Connection update callback
        public System.Action onPositionChanged;
        
        public string NodeName { get; set; }
        public string Description { get; set; }
        public Port InputPort => _inputPort;
        public Port OutputPort => _outputPort;

        // Hover state property
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

        // Selected state property
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

        // Node position calculation based on transform, used to replace style-based calculation, not sure if it will cause layout issues
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
                
            // Notify ports to update position
            _inputPort?.UpdateWorldPosition();
            _outputPort?.UpdateWorldPosition();
                
            // Notify connections to update
            onPositionChanged?.Invoke();
        }
        
        // Check if a point is inside the node
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
            // Set basic node style
            name = "behaviour-node";
            
            style.position = Position.Absolute;
            style.width = 180;
            style.minHeight = 80;
            style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f); // Dark gray background
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Border color
            style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            style.borderTopLeftRadius = 8;
            style.borderTopRightRadius = 8;
            style.borderBottomLeftRadius = 8;
            style.borderBottomRightRadius = 8;
            
            // Create header area (similar to UE4's blue header)
            var header = new VisualElement
            {
                name = "node-header",
                style = {
                    backgroundColor = new Color(0.1f, 0.3f, 0.6f, 1f), // Blue header
                    height = 30,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center
                }
            };
            
            // Node name label
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
            
            // Create content area
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
            
            // Description label
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
            // Set different border styles according to the four state combinations, keep the background color unchanged to distinguish node types
            if (_isSelected && _isHovered)
            {
                // Selected and hovered: golden thick border + glow effect
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
                // Only selected: orange thick border
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
                // Only hovered: white medium border
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
                // Default state: original border style
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
