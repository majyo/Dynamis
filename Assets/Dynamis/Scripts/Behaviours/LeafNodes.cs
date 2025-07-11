using System.Linq;
using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 叶子节点基类 - 不能有子节点
    /// </summary>
    public abstract class LeafNode : BehaviourNode
    {
        public override BehaviourNode AddChild(BehaviourNode child)
        {
            Debug.LogWarning("Leaf node cannot have children");
            return null;
        }
    }

    /// <summary>
    /// 上下文感知节点基类 - 提供对行为树上下文的访问
    /// </summary>
    public abstract class ContextAwareNode : LeafNode
    {
        protected BehaviourTree tree;
        protected Blackboard blackboard;
        protected GameObject gameObject;
        protected Transform transform;

        protected override void OnStart()
        {
            // 获取行为树和相关组件
            tree = GetTree();
            
            if (tree == null)
            {
                return;
            }
            
            blackboard = tree.Blackboard;
            gameObject = tree.gameObject;
            transform = tree.transform;
        }

        private BehaviourTree GetTree()
        {
            BehaviourNode current = this;
            while (current.parent != null)
            {
                current = current.parent;
            }

            // 查找根节点所属的BehaviourTree
            return Object.FindObjectsByType<BehaviourTree>(FindObjectsSortMode.None)
                .FirstOrDefault(bt => bt.RootNode == current);
        }
    }

    /// <summary>
    /// 动作节点基类 - 执行具体行为的节点
    /// </summary>
    public abstract class ActionNode : ContextAwareNode
    {
        // 动作节点特有的逻辑可以在这里添加
    }

    /// <summary>
    /// 等待节点 - 等待指定时间
    /// </summary>
    public class WaitNode : ActionNode
    {
        private readonly float _waitTime;
        private float _startTime;

        public WaitNode(float waitTime = 1.0f)
        {
            this._waitTime = waitTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new WaitNode(_waitTime);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (Time.time - _startTime >= _waitTime)
            {
                return NodeState.Success;
            }
            return NodeState.Running;
        }
    }

    /// <summary>
    /// 随机等待节点 - 等待随机时间
    /// </summary>
    public class RandomWaitNode : ActionNode
    {
        private readonly float _minWaitTime;
        private readonly float _maxWaitTime;
        private float _waitTime;
        private float _startTime;

        public RandomWaitNode(float minWaitTime = 0.5f, float maxWaitTime = 2.0f)
        {
            this._minWaitTime = minWaitTime;
            this._maxWaitTime = maxWaitTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new RandomWaitNode(_minWaitTime, _maxWaitTime);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _waitTime = Random.Range(_minWaitTime, _maxWaitTime);
            _startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (Time.time - _startTime >= _waitTime)
            {
                return NodeState.Success;
            }
            return NodeState.Running;
        }
    }

    /// <summary>
    /// 日志节点 - 输出日志信息
    /// </summary>
    public class LogNode : ActionNode
    {
        private readonly string _message;
        private readonly LogType _logType;

        public LogNode(string message, LogType logType = LogType.Log)
        {
            this._message = message;
            this._logType = logType;
        }

        protected override BehaviourNode CreateClone()
        {
            return new LogNode(_message, _logType);
        }

        protected override NodeState OnUpdate()
        {
            switch (_logType)
            {
                case LogType.Log:
                    Debug.Log($"[BehaviourTree] {_message}");
                    break;
                case LogType.Warning:
                    Debug.LogWarning($"[BehaviourTree] {_message}");
                    break;
                case LogType.Error:
                    Debug.LogError($"[BehaviourTree] {_message}");
                    break;
            }
            return NodeState.Success;
        }
    }

    /// <summary>
    /// 总是成功节点
    /// </summary>
    public class SuccessNode : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Success;
        }
    }

    /// <summary>
    /// 总是失败节点
    /// </summary>
    public class FailureNode : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 总是运行节点
    /// </summary>
    public class RunningNode : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Running;
        }
    }

    /// <summary>
    /// 随机结果节点 - 随机返回成功或失败
    /// </summary>
    public class RandomResultNode : ActionNode
    {
        private readonly float _successProbability;

        public RandomResultNode(float successProbability = 0.5f)
        {
            this._successProbability = Mathf.Clamp01(successProbability);
        }

        protected override BehaviourNode CreateClone()
        {
            return new RandomResultNode(_successProbability);
        }

        protected override NodeState OnUpdate()
        {
            return Random.Range(0f, 1f) <= _successProbability ? NodeState.Success : NodeState.Failure;
        }
    }
}
