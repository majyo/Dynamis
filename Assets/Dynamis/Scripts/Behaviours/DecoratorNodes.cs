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

            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                    return NodeState.Failure;
                case NodeState.Failure:
                    return NodeState.Success;
            }

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 重复节点 - 重复执行子节点指定次数
    /// </summary>
    public class RepeaterNode : DecoratorNode
    {
        private int repeatCount = -1; // -1 表示无限重复
        private int currentCount;

        public RepeaterNode(int repeatCount = -1)
        {
            this.repeatCount = repeatCount;
        }

        protected override BehaviourNode CreateClone()
        {
            return new RepeaterNode(repeatCount);
        }

        protected override void OnStart()
        {
            base.OnStart();
            currentCount = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                case NodeState.Failure:
                    currentCount++;
                    if (repeatCount > 0 && currentCount >= repeatCount)
                    {
                        return NodeState.Success;
                    }
                    return NodeState.Running;
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

            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                    return NodeState.Running; // 继续重复
                case NodeState.Failure:
                    return NodeState.Success; // 失败时成功
            }

            return NodeState.Running;
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

            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                    return NodeState.Success; // 成功时成功
                case NodeState.Failure:
                    return NodeState.Running; // 失败时继续重复
            }

            return NodeState.Running;
        }
    }

    /// <summary>
    /// 超时节点 - 在指定时间内执行子节点，超时则失败
    /// </summary>
    public class TimeoutNode : DecoratorNode
    {
        private float timeoutDuration = 5.0f;
        private float startTime;

        public TimeoutNode(float timeoutDuration = 5.0f)
        {
            this.timeoutDuration = timeoutDuration;
        }

        protected override BehaviourNode CreateClone()
        {
            return new TimeoutNode(timeoutDuration);
        }

        protected override void OnStart()
        {
            base.OnStart();
            startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            if (Time.time - startTime > timeoutDuration)
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
        private float cooldownTime = 1.0f;
        private float lastExecutionTime = -1;

        public CooldownNode(float cooldownTime = 1.0f)
        {
            this.cooldownTime = cooldownTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CooldownNode(cooldownTime);
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            if (lastExecutionTime > 0 && Time.time - lastExecutionTime < cooldownTime)
            {
                return NodeState.Failure;
            }

            var result = child.Update();
            if (result == NodeState.Success || result == NodeState.Failure)
            {
                lastExecutionTime = Time.time;
            }

            return result;
        }
    }

    /// <summary>
    /// 条件节点 - 根据条件决定是否执行子节点
    /// </summary>
    public class ConditionalNode : DecoratorNode
    {
        private System.Func<bool> condition;

        public ConditionalNode(System.Func<bool> condition)
        {
            this.condition = condition;
        }

        protected override BehaviourNode CreateClone()
        {
            return new ConditionalNode(condition);
        }

        protected override NodeState OnUpdate()
        {
            if (child == null || condition == null)
                return NodeState.Failure;

            if (!condition.Invoke())
                return NodeState.Failure;

            return child.Update();
        }
    }
}
