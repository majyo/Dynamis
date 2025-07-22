using System;
using System.Collections.Generic;
using Dynamis.Behaviours.Runtimes.Blackboards;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public enum SearchStrategy
    {
        Bfs,
        Dfs
    }

    public class BehaviourTree : ScriptableObject
    {
        [SerializeField] private Node rootNode;
        [SerializeField] private List<Node> nodes = new();
        [SerializeField] private NodeState treeState = NodeState.Running;
        [SerializeField] private Blackboard blackboard = new();

        public Node RootNode 
        { 
            get => rootNode; 
            set => rootNode = value; 
        }

        public List<Node> Nodes => nodes;
        public NodeState TreeState => treeState;
        public Blackboard Blackboard => blackboard;

        public void Reset()
        {
            if (rootNode != null)
            {
                rootNode.ResetNode();
            }
            // Reset blackboard if needed
            // blackboard.Clear(); // Uncomment if you want to clear blackboard on reset
        }

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

        public void Traverse(Action<Node> action, SearchStrategy order = SearchStrategy.Bfs)
        {
            if (rootNode == null || action == null)
                return;

            if (order == SearchStrategy.Bfs)
            {
                TraverseBfs(action);
            }
            else
            {
                TraverseDfs(action);
            }
        }

        private void TraverseBfs(Action<Node> action)
        {
            var queue = PoolUtils.GetQueue();
            var children = PoolUtils.GetList();
            try
            {
                queue.Enqueue(rootNode);

                while (queue.Count > 0)
                {
                    var currentNode = queue.Dequeue();
                    action(currentNode);

                    // Get child nodes and add them to the queue
                    GetChildren(currentNode, children);
                    
                    foreach (var child in children)
                    {
                        if (child != null)
                        {
                            queue.Enqueue(child);
                        }
                    }
                    
                    children.Clear();
                }
            }
            finally
            {
                PoolUtils.ReleaseQueue(queue);
            }
        }

        private void TraverseDfs(Action<Node> action)
        {
            var stack = PoolUtils.GetStack();
            var children = PoolUtils.GetList();
            try
            {
                stack.Push(rootNode);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();
                    action(currentNode);

                    // Get child nodes and add them to the stack in reverse order (so they are visited in the correct order during traversal)
                    GetChildren(currentNode, children);
                    
                    for (int i = children.Count - 1; i >= 0; i--)
                    {
                        if (children[i] != null)
                            stack.Push(children[i]);
                    }
                    
                    children.Clear();
                }
            }
            finally
            {
                PoolUtils.ReleaseStack(stack);
                PoolUtils.ReleaseList(children);
            }
        }

        private static void GetChildren(Node node, List<Node> children)
        {
            switch (node)
            {
                case CompositeNode composite:
                    children.AddRange(composite.Children);
                    break;
                case DecoratorNode decorator when decorator.Child != null:
                    children.Add(decorator.Child);
                    break;
            }
        }

        public BehaviourTree Clone()
        {
            var tree = Instantiate(this);
            tree.rootNode = rootNode?.Clone();
            tree.nodes = new List<Node>();
            tree.blackboard = blackboard.Clone();

            if (tree.rootNode != null)
            {
                tree.Traverse(node =>
                {
                    tree.nodes.Add(node);
                    node.SetBehaviourTree(tree);
                });
            }
                
            return tree;
        }
    }
}
