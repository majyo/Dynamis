using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public enum SearchStrategy
    {
        Bfs,
        Dfs
    }

    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Dynamis/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject
    {
        [SerializeField] private Node rootNode;
        [SerializeField] private List<Node> nodes = new();
        [SerializeField] private NodeState treeState = NodeState.Running;

        public Node RootNode 
        { 
            get => rootNode; 
            set => rootNode = value; 
        }

        public List<Node> Nodes => nodes;
        public NodeState TreeState => treeState;

        public void Reset()
        {
            if (rootNode != null)
                rootNode.Reset();
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

                    // 获取子节点并加入队列
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

                    // 获取子节点并以逆序加入栈（这样遍历时会按正序访问）
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
