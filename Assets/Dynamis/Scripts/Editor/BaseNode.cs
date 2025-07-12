using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Dynamis.Scripts.Editor
{
    [Serializable]
    public class BaseNode : Node
    {
        [SerializeField] private string nodeId;
        [SerializeField] private Vector2 nodePosition;
        [SerializeField] private string nodeTitle;

        public List<Port> inputPorts = new List<Port>();
        public List<Port> outputPorts = new List<Port>();

        public string NodeId
        {
            get => nodeId;
            set => nodeId = value;
        }

        public Vector2 NodePosition
        {
            get => nodePosition;
            set => nodePosition = value;
        }

        public string NodeTitle
        {
            get => nodeTitle;
            set => nodeTitle = value;
        }

        public BaseNode()
        {
            nodeId = Guid.NewGuid().ToString();
            nodeTitle = "Base Node";
            SetupNode();
        }

        public BaseNode(Vector2 position, string title = "Node")
        {
            nodeId = Guid.NewGuid().ToString();
            nodePosition = position;
            nodeTitle = title;
            SetupNode();
        }

        private void SetupNode()
        {
            title = nodeTitle ?? "Base Node";
            
            // 设置节点样式
            AddToClassList("node");
            
            // 设置节点位置
            SetPosition(new Rect(nodePosition, Vector2.zero));
            
            // 创建默认端口
            CreatePorts();
            
            // 刷新节点
            RefreshExpandedState();
            RefreshPorts();
        }

        protected virtual void CreatePorts()
        {
            // 子类可以重写此方法来创建特定的端口
        }

        protected Port CreateInputPort(string portName, Type portType, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, portType);
            port.portName = portName;
            inputContainer.Add(port);
            inputPorts.Add(port);
            return port;
        }

        protected Port CreateOutputPort(string portName, Type portType, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, portType);
            port.portName = portName;
            outputContainer.Add(port);
            outputPorts.Add(port);
            return port;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            nodePosition = newPos.position;
        }

        // 获取节点数据用于序列化
        public virtual NodeData GetNodeData()
        {
            return new NodeData
            {
                nodeId = this.nodeId,
                position = this.nodePosition,
                title = this.nodeTitle,
                nodeType = this.GetType().AssemblyQualifiedName
            };
        }

        // 从数据加载节点
        public virtual void LoadFromData(NodeData data)
        {
            nodeId = data.nodeId;
            nodePosition = data.position;
            nodeTitle = data.title;
            title = nodeTitle;
            SetPosition(new Rect(nodePosition, Vector2.zero));
        }
    }

    [Serializable]
    public class NodeData
    {
        public string nodeId;
        public Vector2 position;
        public string title;
        public string nodeType;
    }
}
