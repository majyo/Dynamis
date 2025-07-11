using UnityEngine;
using System.Linq;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 装饰节点基类 - 只能有一个子节点
    /// </summary>
    public abstract class DecoratorNode : BehaviourNode
    {
        [HideInInspector] public BehaviourNode child;
        
        // 添加上下文访问能力
        protected BehaviourTree tree;
        protected Blackboard blackboard;
        protected GameObject gameObject;
        protected Transform transform;

        public override BehaviourNode AddChild(BehaviourNode node)
        {
            if (child == null)
            {
                child = node;
                return base.AddChild(node);
            }
            
            Debug.LogWarning("Decorator node can only have one child");
            return null;
        }

        public override void RemoveChild(BehaviourNode node)
        {
            if (child == node)
            {
                child = null;
            }
            base.RemoveChild(node);
        }

        protected override void OnStart()
        {
            // 获取行为树上下文
            tree = GetTree();
            
            if (tree != null)
            {
                blackboard = tree.Blackboard;
                gameObject = tree.gameObject;
                transform = tree.transform;
            }
            
            // 确保child引用指向正确的子节点
            if (children.Count > 0)
            {
                child = children[0];
            }
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
    /// 反转节点 - 反转子节点的成功/失败状态
    /// </summary>
    public class InverterNode : DecoratorNode
    {
        protected override BehaviourNode CreateClone()
        {
            return new InverterNode();
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            return child.Update() switch
            {
                NodeState.Running => NodeState.Running,
                NodeState.Success => NodeState.Failure,
                NodeState.Failure => NodeState.Success,
                _ => NodeState.Failure
            };
        }
    }

    /// <summary>
    /// 重复节点 - 重复执行子节点指定次数
    /// </summary>
    public class RepeaterNode : DecoratorNode
    {
        private readonly int _repeatCount; // -1 表示无限重复
        private int _currentCount;

        public RepeaterNode(int repeatCount = -1)
        {
            this._repeatCount = repeatCount;
        }

        protected override BehaviourNode CreateClone()
        {
            return new RepeaterNode(_repeatCount);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _currentCount = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
            {
                return NodeState.Failure;
            }

            switch (child.Update())
            {
                case NodeState.Running:
                    break;
                case NodeState.Success:
                case NodeState.Failure:
                    _currentCount++;
                    if (_repeatCount > 0 && _currentCount >= _repeatCount)
                    {
                        return NodeState.Success;
                    }
                    break;
            }

            return NodeState.Running;
        }
    }

    /// <summary>
    /// 重复直到失败节点 - 重复执行子节点直到失败
    /// </summary>
    public class RepeatUntilFailNode : DecoratorNode
    {
        protected override BehaviourNode CreateClone()
        {
            return new RepeatUntilFailNode();
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            return child.Update() switch
            {
                NodeState.Running => NodeState.Running,
                NodeState.Success => NodeState.Running // 继续重复
                ,
                NodeState.Failure => NodeState.Success // 失败时成功
                ,
                _ => NodeState.Running
            };
        }
    }

    /// <summary>
    /// 重复直到成功节点 - 重复执行子节点直到成功
    /// </summary>
    public class RepeatUntilSuccessNode : DecoratorNode
    {
        protected override BehaviourNode CreateClone()
        {
            return new RepeatUntilSuccessNode();
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            return child.Update() switch
            {
                NodeState.Running => NodeState.Running,
                NodeState.Success => NodeState.Success // 成功时成功
                ,
                NodeState.Failure => NodeState.Running // 失败时继续重复
                ,
                _ => NodeState.Running
            };
        }
    }

    /// <summary>
    /// 超时节点 - 在指定时间内执行子节点，超时则失败
    /// </summary>
    public class TimeoutNode : DecoratorNode
    {
        private readonly float _timeoutDuration;
        private float _startTime;

        public TimeoutNode(float timeoutDuration = 5.0f)
        {
            this._timeoutDuration = timeoutDuration;
        }

        protected override BehaviourNode CreateClone()
        {
            return new TimeoutNode(_timeoutDuration);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            if (Time.time - _startTime > _timeoutDuration)
            {
                child.Abort();
                return NodeState.Failure;
            }

            return child.Update();
        }
    }

    /// <summary>
    /// 冷却节点 - 在冷却时间内返回失败，冷却结束后执行子节点
    /// </summary>
    public class CooldownNode : DecoratorNode
    {
        private readonly float _cooldownTime;
        private float _lastExecutionTime = -1;

        public CooldownNode(float cooldownTime = 1.0f)
        {
            this._cooldownTime = cooldownTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CooldownNode(_cooldownTime);
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            if (_lastExecutionTime > 0 && Time.time - _lastExecutionTime < _cooldownTime)
            {
                return NodeState.Failure;
            }

            var result = child.Update();
            if (result == NodeState.Success || result == NodeState.Failure)
            {
                _lastExecutionTime = Time.time;
            }

            return result;
        }
    }

    /// <summary>
    /// 条件节点 - 根据条件决定是否执行子节点
    /// </summary>
    public class ConditionalNode : DecoratorNode
    {
        private readonly System.Func<bool> _condition;

        public ConditionalNode(System.Func<bool> condition)
        {
            this._condition = condition;
        }

        protected override BehaviourNode CreateClone()
        {
            return new ConditionalNode(_condition);
        }

        protected override NodeState OnUpdate()
        {
            if (child == null || _condition == null)
                return NodeState.Failure;

            if (!_condition.Invoke())
                return NodeState.Failure;

            return child.Update();
        }
    }
}
