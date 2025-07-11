using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Editor
{
    public class NodeGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(200, 150);
        
        private NodeSearchWindow searchWindow;
        private new List<BaseNode> nodes = new List<BaseNode>();
        
        public NodeGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            // 添加网格背景
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            
            // 创建搜索窗口
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            
            AddToClassList("graph-view");
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node &&
                endPort.portType == startPort.portType).ToList();
        }

        public BaseNode CreateNode(Type nodeType, Vector2 position)
        {
            var node = (BaseNode)Activator.CreateInstance(nodeType, position);
            AddElement(node);
            nodes.Add(node);
            return node;
        }

        public void CreateNode<T>(Vector2 position) where T : BaseNode
        {
            CreateNode(typeof(T), position);
        }

        public void DeleteNode(BaseNode node)
        {
            if (nodes.Contains(node))
            {
                nodes.Remove(node);
                RemoveElement(node);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            var mousePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            
            evt.menu.AppendAction("Create/Math Node", _ => CreateNode<MathNode>(mousePosition));
            evt.menu.AppendAction("Create/Input Node", _ => CreateNode<InputNode>(mousePosition));
            evt.menu.AppendAction("Create/Output Node", _ => CreateNode<OutputNode>(mousePosition));
        }

        public GraphData GetGraphData()
        {
            var graphData = new GraphData();
            
            // 保存节点数据
            foreach (var node in nodes)
            {
                graphData.nodes.Add(node.GetNodeData());
            }
            
            // 保存连接数据
            foreach (var edge in edges.ToList())
            {
                var connectionData = new ConnectionData
                {
                    outputNodeId = ((BaseNode)edge.output.node).NodeId,
                    outputPortName = edge.output.portName,
                    inputNodeId = ((BaseNode)edge.input.node).NodeId,
                    inputPortName = edge.input.portName
                };
                graphData.connections.Add(connectionData);
            }
            
            return graphData;
        }

        public void LoadGraphData(GraphData graphData)
        {
            // 清空当前图形
            ClearGraph();
            
            // 创建节点
            var nodeDict = new Dictionary<string, BaseNode>();
            foreach (var nodeData in graphData.nodes)
            {
                var nodeType = Type.GetType(nodeData.nodeType);
                if (nodeType != null && nodeType.IsSubclassOf(typeof(BaseNode)))
                {
                    var node = (BaseNode)Activator.CreateInstance(nodeType);
                    node.LoadFromData(nodeData);
                    AddElement(node);
                    nodes.Add(node);
                    nodeDict[nodeData.nodeId] = node;
                }
            }
            
            // 创建连接
            foreach (var connectionData in graphData.connections)
            {
                if (nodeDict.TryGetValue(connectionData.outputNodeId, out var outputNode) &&
                    nodeDict.TryGetValue(connectionData.inputNodeId, out var inputNode))
                {
                    var outputPort = outputNode.outputPorts.FirstOrDefault(p => p.portName == connectionData.outputPortName);
                    var inputPort = inputNode.inputPorts.FirstOrDefault(p => p.portName == connectionData.inputPortName);
                    
                    if (outputPort != null && inputPort != null)
                    {
                        var edge = outputPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }
            }
        }

        public void ClearGraph()
        {
            foreach (var node in nodes.ToList())
            {
                RemoveElement(node);
            }
            nodes.Clear();
            
            foreach (var edge in edges.ToList())
            {
                RemoveElement(edge);
            }
        }
    }

    [Serializable]
    public class GraphData
    {
        public List<NodeData> nodes = new List<NodeData>();
        public List<ConnectionData> connections = new List<ConnectionData>();
    }

    [Serializable]
    public class ConnectionData
    {
        public string outputNodeId;
        public string outputPortName;
        public string inputNodeId;
        public string inputPortName;
    }
}
