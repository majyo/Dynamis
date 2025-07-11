using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 行为树节点状态
    /// </summary>
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class BehaviourNode
    {
        public string name;
        public BehaviourNode parent;
        public readonly List<BehaviourNode> children = new();
        
        protected NodeState state = NodeState.Failure;
        protected bool started = false;

        public NodeState State => state;

        /// <summary>
        /// 更新节点
        /// </summary>
        public NodeState Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state is NodeState.Failure or NodeState.Success)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public virtual BehaviourNode AddChild(BehaviourNode child)
        {
            child.parent = this;
            children.Add(child);
            return child;
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public virtual void RemoveChild(BehaviourNode child)
        {
            children.Remove(child);
            child.parent = null;
        }

        /// <summary>
        /// 克隆节点 - 创建节点的深拷贝
        /// </summary>
        public BehaviourNode Clone()
        {
            // 创建节点实例
            BehaviourNode clone = CreateClone();
            
            // 复制基本属性
            clone.name = name;
            
            // 克隆所有子节点
            foreach (var child in children)
            {
                clone.AddChild(child.Clone());
            }
            
            return clone;
        }

        /// <summary>
        /// 创建节点的克隆实例 - 子类需要重写此方法来复制特定状态
        /// </summary>
        protected virtual BehaviourNode CreateClone()
        {
            return System.Activator.CreateInstance(GetType()) as BehaviourNode;
        }

        /// <summary>
        /// 中止执行
        /// </summary>
        public void Abort()
        {
            BehaviourTree.Traverse(this, (node) =>
            {
                node.started = false;
                node.state = NodeState.Failure;
                node.OnStop();
            });
        }

        protected virtual void OnStart() { }
        protected abstract NodeState OnUpdate();
        protected virtual void OnStop() { }
    }

    /// <summary>
    /// 行为树运行器
    /// </summary>
    public class BehaviourTree : MonoBehaviour
    {
        [SerializeField] private bool runOnUpdate = true;
        [SerializeField] private float updateInterval = 0.1f;

        private float _lastUpdateTime;
        public Blackboard Blackboard { get; private set; }

        public BehaviourNode RootNode { get; set; }

        private void Awake()
        {
            Blackboard = GetComponent<Blackboard>();
            if (Blackboard == null)
            {
                Blackboard = gameObject.AddComponent<Blackboard>();
            }
        }

        private void Start()
        {
            if (RootNode != null)
            {
                RootNode = RootNode.Clone();
            }
        }

        private void Update()
        {
            if (runOnUpdate && RootNode != null && Time.time - _lastUpdateTime >= updateInterval)
            {
                RootNode.Update();
                _lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// 手动执行一次行为树
        /// </summary>
        public NodeState Tick()
        {
            return RootNode?.Update() ?? NodeState.Failure;
        }

        /// <summary>
        /// 遍历节点
        /// </summary>
        public static void Traverse(BehaviourNode node, System.Action<BehaviourNode> visitor)
        {
            if (node != null)
            {
                visitor.Invoke(node);
                foreach (var child in node.children)
                {
                    Traverse(child, visitor);
                }
            }
        }

        /// <summary>
        /// 创建行为树
        /// </summary>
        public static BehaviourTree CreateTree(GameObject gameObject, BehaviourNode rootNode)
        {
            var tree = gameObject.GetComponent<BehaviourTree>();
            if (tree == null)
            {
                tree = gameObject.AddComponent<BehaviourTree>();
            }
            
            tree.RootNode = rootNode;
            return tree;
        }
    }
}
