using System;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 行为树构建器 - 提供流畅的API来构建行为树
    /// </summary>
    public class BehaviourTreeBuilder
    {
        private BehaviourNode _rootNode;
        private BehaviourNode _currentNode;

        /// <summary>
        /// 开始构建行为树
        /// </summary>
        public static BehaviourTreeBuilder Create()
        {
            return new BehaviourTreeBuilder();
        }

        /// <summary>
        /// 设置根节点
        /// </summary>
        public BehaviourTreeBuilder Root<T>() where T : BehaviourNode, new()
        {
            _rootNode = new T();
            _currentNode = _rootNode;
            return this;
        }

        /// <summary>
        /// 设置根节点
        /// </summary>
        public BehaviourTreeBuilder Root(BehaviourNode node)
        {
            _rootNode = node;
            _currentNode = _rootNode;
            return this;
        }

        public BehaviourTreeBuilder Node(BehaviourNode node, bool stay = false)
        {
            AddChild(node, stay);
            return this;
        }

        /// <summary>
        /// 添加顺序节点
        /// </summary>
        public BehaviourTreeBuilder Sequence(string name = "Sequence")
        {
            var node = new SequenceNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加选择节点
        /// </summary>
        public BehaviourTreeBuilder Selector(string name = "Selector")
        {
            var node = new SelectorNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加并行节点
        /// </summary>
        public BehaviourTreeBuilder Parallel(string name = "Parallel", bool failOnAny = false, bool succeedOnAll = true)
        {
            var node = new ParallelNode(failOnAny, succeedOnAll) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加随机选择节点
        /// </summary>
        public BehaviourTreeBuilder RandomSelector(string name = "RandomSelector")
        {
            var node = new RandomSelectorNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加权重随机选择节点
        /// </summary>
        public BehaviourTreeBuilder WeightedRandomSelector(string name = "WeightedRandomSelector", params float[] weights)
        {
            var node = new WeightedRandomSelectorNode(weights) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加反转装饰器
        /// </summary>
        public BehaviourTreeBuilder Inverter(string name = "Inverter")
        {
            var node = new InverterNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加重复装饰器
        /// </summary>
        public BehaviourTreeBuilder Repeater(string name = "Repeater", int repeatCount = -1)
        {
            var node = new RepeaterNode(repeatCount) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加重复直到失败装饰器
        /// </summary>
        public BehaviourTreeBuilder RepeatUntilFail(string name = "RepeatUntilFail")
        {
            var node = new RepeatUntilFailNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加重复直到成功装饰器
        /// </summary>
        public BehaviourTreeBuilder RepeatUntilSuccess(string name = "RepeatUntilSuccess")
        {
            var node = new RepeatUntilSuccessNode { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加超时装饰器
        /// </summary>
        public BehaviourTreeBuilder Timeout(string name = "Timeout", float timeoutDuration = 5.0f)
        {
            var node = new TimeoutNode(timeoutDuration) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加冷却装饰器
        /// </summary>
        public BehaviourTreeBuilder Cooldown(string name = "Cooldown", float cooldownTime = 1.0f)
        {
            var node = new CooldownNode(cooldownTime) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加条件装饰器
        /// </summary>
        public BehaviourTreeBuilder Conditional(string name = "Conditional", Func<bool> condition = null)
        {
            var node = new ConditionalNode(condition) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加黑板条件装饰器
        /// </summary>
        public BehaviourTreeBuilder BlackboardCondition(string name, string key, object expectedValue = null, 
            BlackboardConditionNode.CompareType compareType = BlackboardConditionNode.CompareType.Exists)
        {
            var node = new BlackboardConditionNode(key, expectedValue, compareType) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加距离条件装饰器
        /// </summary>
        public BehaviourTreeBuilder DistanceCondition(string name, Transform target, float distance, 
            DistanceConditionNode.CompareType compareType = DistanceConditionNode.CompareType.Less)
        {
            var node = new DistanceConditionNode(target, distance, compareType) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加距离条件装饰器（从黑板获取目标）
        /// </summary>
        public BehaviourTreeBuilder DistanceCondition(string name, string targetKey, float distance, 
            DistanceConditionNode.CompareType compareType)
        {
            var node = new DistanceConditionNode(targetKey, distance, compareType) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加时间条件装饰器
        /// </summary>
        public BehaviourTreeBuilder TimeCondition(string name = "TimeCondition", float targetTime = 1.0f)
        {
            var node = new TimeConditionNode(targetTime) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加随机条件装饰器
        /// </summary>
        public BehaviourTreeBuilder RandomCondition(string name = "RandomCondition", float successProbability = 0.5f)
        {
            var node = new RandomConditionNode(successProbability) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加冷却条件装饰器
        /// </summary>
        public BehaviourTreeBuilder CooldownCondition(string name = "CooldownCondition", float cooldownTime = 1.0f)
        {
            var node = new CooldownConditionNode(cooldownTime) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加自定义条件装饰器
        /// </summary>
        public BehaviourTreeBuilder CustomCondition(string name = "CustomCondition", Func<bool> conditionFunc = null)
        {
            var node = new CustomConditionNode(conditionFunc) { name = name };
            AddChild(node);
            return this;
        }

        /// <summary>
        /// 添加等待动作
        /// </summary>
        public BehaviourTreeBuilder Wait(string name = "Wait", float waitTime = 1.0f)
        {
            var node = new WaitNode(waitTime) { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 添加随机等待动作
        /// </summary>
        public BehaviourTreeBuilder RandomWait(string name = "RandomWait", float minWaitTime = 0.5f, float maxWaitTime = 2.0f)
        {
            var node = new RandomWaitNode(minWaitTime, maxWaitTime) { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 添加日志动作
        /// </summary>
        public BehaviourTreeBuilder Log(string name = "Log", string message = "Log message", LogType logType = LogType.Log)
        {
            var node = new LogNode(message, logType) { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 添加成功节点
        /// </summary>
        public BehaviourTreeBuilder Success(string name = "Success")
        {
            var node = new SuccessNode { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 添加失败节点
        /// </summary>
        public BehaviourTreeBuilder Failure(string name = "Failure")
        {
            var node = new FailureNode { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 添加随机结果节点
        /// </summary>
        public BehaviourTreeBuilder RandomResult(string name = "RandomResult", float successProbability = 0.5f)
        {
            var node = new RandomResultNode(successProbability) { name = name };
            AddChild(node, stay: true);
            return this;
        }

        /// <summary>
        /// 返回上一级节点
        /// </summary>
        public BehaviourTreeBuilder Back()
        {
            if (_currentNode is { parent: not null })
            {
                _currentNode = _currentNode.parent;
            }
            return this;
        }

        /// <summary>
        /// 结束当前分支，返回根节点
        /// </summary>
        public BehaviourTreeBuilder End()
        {
            _currentNode = _rootNode;
            return this;
        }

        /// <summary>
        /// 构建并返回行为树
        /// </summary>
        public BehaviourNode Build()
        {
            return _rootNode;
        }

        /// <summary>
        /// 构建并附加到GameObject
        /// </summary>
        public BehaviourTree BuildAndAttach(GameObject gameObject)
        {
            return BehaviourTree.CreateTree(gameObject, _rootNode);
        }

        private void AddChild(BehaviourNode child, bool stay = false)
        {
            if (_currentNode == null)
            {
                Debug.LogError("Cannot add child: current node is null. Use Root() first.");
                return;
            }
            
            _currentNode.AddChild(child);

            if (stay)
            {
                return;
            }
            
            _currentNode = child;
        }
    }
}
