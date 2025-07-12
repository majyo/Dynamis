using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Scripts.Editor
{
    public class MathNode : BaseNode
    {
        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public MathOperation operation = MathOperation.Add;
        private DropdownField operationDropdown;

        public MathNode() : base()
        {
            NodeTitle = "Math Node";
            AddToClassList("math-node");
        }

        public MathNode(Vector2 position) : base(position, "Math Node")
        {
            AddToClassList("math-node");
        }

        protected override void CreatePorts()
        {
            // 创建输入端口
            CreateInputPort("Input A", typeof(float));
            CreateInputPort("Input B", typeof(float));
            
            // 创建输出端口
            CreateOutputPort("Result", typeof(float));
            
            // 创建操作选择下拉菜单
            operationDropdown = new DropdownField("Operation", 
                new System.Collections.Generic.List<string> { "Add", "Subtract", "Multiply", "Divide" }, 
                0);
            operationDropdown.RegisterValueChangedCallback(evt => 
            {
                operation = (MathOperation)operationDropdown.index;
            });
            
            mainContainer.Add(operationDropdown);
        }

        public override NodeData GetNodeData()
        {
            var data = base.GetNodeData();
            // 可以在这里添加特定于MathNode的数据
            return data;
        }
    }

    public class InputNode : BaseNode
    {
        private FloatField valueField;
        private float inputValue = 0f;

        public InputNode() : base()
        {
            NodeTitle = "Input Node";
            AddToClassList("input-node");
        }

        public InputNode(Vector2 position) : base(position, "Input Node")
        {
            AddToClassList("input-node");
        }

        protected override void CreatePorts()
        {
            // 只有输出端口
            CreateOutputPort("Value", typeof(float));
            
            // 创建数值输入字段
            valueField = new FloatField("Value:");
            valueField.value = inputValue;
            valueField.RegisterValueChangedCallback(evt => 
            {
                inputValue = evt.newValue;
            });
            
            mainContainer.Add(valueField);
        }

        public float GetValue() => inputValue;
    }

    public class OutputNode : BaseNode
    {
        private Label resultLabel;

        public OutputNode() : base()
        {
            NodeTitle = "Output Node";
            AddToClassList("output-node");
        }

        public OutputNode(Vector2 position) : base(position, "Output Node")
        {
            AddToClassList("output-node");
        }

        protected override void CreatePorts()
        {
            // 只有输入端口
            CreateInputPort("Input", typeof(float));
            
            // 创建结果显示标签
            resultLabel = new Label("Result: 0");
            resultLabel.AddToClassList("result-label");
            
            mainContainer.Add(resultLabel);
        }

        public void UpdateResult(float value)
        {
            resultLabel.text = $"Result: {value:F2}";
        }
    }

    public class StringNode : BaseNode
    {
        private TextField textField;
        private string textValue = "";

        public StringNode() : base()
        {
            NodeTitle = "String Node";
            AddToClassList("string-node");
        }

        public StringNode(Vector2 position) : base(position, "String Node")
        {
            AddToClassList("string-node");
        }

        protected override void CreatePorts()
        {
            CreateInputPort("Input", typeof(string));
            CreateOutputPort("Output", typeof(string));
            
            textField = new TextField("Text:");
            textField.value = textValue;
            textField.RegisterValueChangedCallback(evt => 
            {
                textValue = evt.newValue;
            });
            
            mainContainer.Add(textField);
        }

        public string GetValue() => textValue;
    }
}
