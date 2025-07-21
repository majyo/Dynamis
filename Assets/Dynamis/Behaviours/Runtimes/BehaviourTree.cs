using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Dynamis/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject
    {
        [SerializeField] private Node rootNode;
        [SerializeField] private List<Node> nodes = new List<Node>();
        [SerializeField] private NodeState treeState = NodeState.Running;
        
        public Node RootNode 
        { 
            get => rootNode; 
            set => rootNode = value; 
        }
        
        public List<Node> Nodes => nodes;
        public NodeState TreeState => treeState;

        public NodeState Update()
        {
            if (rootNode == null)
                return NodeState.Failure;
                
            treeState = rootNode.Update();
            return treeState;
        }

        public void AddNode(Node node)
        {
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                node.SetBehaviourTree(this);
            }
        }

        public void RemoveNode(Node node)
        {
            nodes.Remove(node);
        }

        public void Reset()
        {
            if (rootNode != null)
                rootNode.Reset();
        }

        public BehaviourTree Clone()
        {
            BehaviourTree tree = Instantiate(this);
            tree.rootNode = rootNode?.Clone();
            tree.nodes = new List<Node>();
            
            if (tree.rootNode != null)
                tree.CollectNodes(tree.rootNode);
                
            return tree;
        }

        private void CollectNodes(Node node)
        {
            if (node != null)
            {
                nodes.Add(node);
                node.SetBehaviourTree(this);
                
                if (node is CompositeNode composite)
                {
                    foreach (var child in composite.Children)
                        CollectNodes(child);
                }
                else if (node is DecoratorNode decorator && decorator.Child != null)
                {
                    CollectNodes(decorator.Child);
                }
            }
        }
    }
}
