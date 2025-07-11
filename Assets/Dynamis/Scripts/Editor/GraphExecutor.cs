using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dynamis.Editor
{
    public class GraphExecutor
    {
        private NodeGraphView graphView;
        private Dictionary<string, object> nodeValues = new Dictionary<string, object>();

        public GraphExecutor(NodeGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void ExecuteGraph()
        {
            nodeValues.Clear();
            
            // 获取所有节点
            var nodes = graphView.nodes.Cast<BaseNode>().ToList();
            var processedNodes = new HashSet<string>();
            
            // 执行输入节点
            ExecuteInputNodes(nodes, processedNodes);
            
            // 执行其他节点（按依赖顺序）
            ExecuteRemainingNodes(nodes, processedNodes);
            
            // 更新输出节点显示
            UpdateOutputNodes(nodes);
        }

        private void ExecuteInputNodes(List<BaseNode> nodes, HashSet<string> processedNodes)
        {
            foreach (var node in nodes.OfType<InputNode>())
            {
                var value = node.GetValue();
                nodeValues[node.NodeId] = value;
                processedNodes.Add(node.NodeId);
                Debug.Log($"Input Node {node.NodeId}: {value}");
            }
        }

        private void ExecuteRemainingNodes(List<BaseNode> nodes, HashSet<string> processedNodes)
        {
            bool madeProgress = true;
            
            while (madeProgress && processedNodes.Count < nodes.Count)
            {
                madeProgress = false;
                
                foreach (var node in nodes)
                {
                    if (processedNodes.Contains(node.NodeId))
                        continue;
                    
                    if (CanExecuteNode(node, processedNodes))
                    {
                        ExecuteNode(node);
                        processedNodes.Add(node.NodeId);
                        madeProgress = true;
                    }
                }
            }
        }

        private bool CanExecuteNode(BaseNode node, HashSet<string> processedNodes)
        {
            // 检查所有输入端口是否都有连接且已处理
            foreach (var inputPort in node.inputPorts)
            {
                if (inputPort.connected)
                {
                    var connectedOutputPort = inputPort.connections.FirstOrDefault()?.output;
                    if (connectedOutputPort != null)
                    {
                        var connectedNode = (BaseNode)connectedOutputPort.node;
                        if (!processedNodes.Contains(connectedNode.NodeId))
                        {
                            return false;
                        }
                    }
                }
                else if (!(node is OutputNode)) // 输出节点可以没有连接
                {
                    // 对于没有连接的输入端口，我们可以使用默认值
                    // 这里简化处理，实际项目中可能需要更复杂的逻辑
                }
            }
            
            return true;
        }

        private void ExecuteNode(BaseNode node)
        {
            switch (node)
            {
                case MathNode mathNode:
                    ExecuteMathNode(mathNode);
                    break;
                case StringNode stringNode:
                    ExecuteStringNode(stringNode);
                    break;
                case OutputNode:
                    // 输出节点在UpdateOutputNodes中处理
                    break;
            }
        }

        private void ExecuteMathNode(MathNode mathNode)
        {
            float inputA = GetInputValue(mathNode, "Input A", 0f);
            float inputB = GetInputValue(mathNode, "Input B", 0f);
            
            float result = 0f;
            var operation = mathNode.operation;
            
            switch (operation)
            {
                case MathNode.MathOperation.Add:
                    result = inputA + inputB;
                    break;
                case MathNode.MathOperation.Subtract:
                    result = inputA - inputB;
                    break;
                case MathNode.MathOperation.Multiply:
                    result = inputA * inputB;
                    break;
                case MathNode.MathOperation.Divide:
                    result = inputB != 0 ? inputA / inputB : 0f;
                    break;
            }
            
            nodeValues[mathNode.NodeId] = result;
            Debug.Log($"Math Node {mathNode.NodeId}: {inputA} {operation} {inputB} = {result}");
        }

        private void ExecuteStringNode(StringNode stringNode)
        {
            string inputValue = GetInputValue(stringNode, "Input", "");
            string nodeValue = stringNode.GetValue();
            
            // 简单的字符串连接操作
            string result = string.IsNullOrEmpty(inputValue) ? nodeValue : inputValue + nodeValue;
            
            nodeValues[stringNode.NodeId] = result;
            Debug.Log($"String Node {stringNode.NodeId}: {result}");
        }

        private T GetInputValue<T>(BaseNode node, string portName, T defaultValue)
        {
            var inputPort = node.inputPorts.FirstOrDefault(p => p.portName == portName);
            if (inputPort?.connected == true)
            {
                var connectedOutputPort = inputPort.connections.FirstOrDefault()?.output;
                if (connectedOutputPort != null)
                {
                    var connectedNode = (BaseNode)connectedOutputPort.node;
                    if (nodeValues.TryGetValue(connectedNode.NodeId, out var value) && value is T)
                    {
                        return (T)value;
                    }
                }
            }
            
            return defaultValue;
        }

        private void UpdateOutputNodes(List<BaseNode> nodes)
        {
            foreach (var outputNode in nodes.OfType<OutputNode>())
            {
                float inputValue = GetInputValue(outputNode, "Input", 0f);
                outputNode.UpdateResult(inputValue);
                Debug.Log($"Output Node {outputNode.NodeId}: {inputValue}");
            }
        }
    }
}
