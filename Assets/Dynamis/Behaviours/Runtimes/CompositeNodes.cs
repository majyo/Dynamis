using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 复合节点基类
    /// </summary>
    public abstract class CompositeNode : BehaviourNode
    {
        protected int currentChild;

        protected override void OnStart()
        {
            currentChild = 0;
        }
    }

    /// <summary>
    /// 顺序节点 - 按顺序执行子节点，全部成功才成功
    /// </summary>
    public class SequenceNode : CompositeNode
    {
        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Success;

            for (int i = currentChild; i < children.Count; i++)
            {
                currentChild = i;
                var child = children[currentChild];

                switch (child.Update())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        return NodeState.Failure;
                    case NodeState.Success:
                        continue;
                }
            }

            return NodeState.Success;
        }
    }

    /// <summary>
    /// 选择节点 - 按顺序执行子节点，任一成功即成功
    /// </summary>
    public class SelectorNode : CompositeNode
    {
        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            for (int i = currentChild; i < children.Count; i++)
            {
                currentChild = i;
                var child = children[currentChild];

                switch (child.Update())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        return NodeState.Success;
                    case NodeState.Failure:
                        continue;
                }
            }

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 并行节点 - 同时执行所有子节点
    /// </summary>
    public class ParallelNode : CompositeNode
    {
        private readonly bool _failOnAny;
        private readonly bool _succeedOnAll;

        public ParallelNode(bool failOnAny = false, bool succeedOnAll = true)
        {
            this._failOnAny = failOnAny;
            this._succeedOnAll = succeedOnAll;
        }

        protected override BehaviourNode CreateClone()
        {
            return new ParallelNode(_failOnAny, _succeedOnAll);
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Success;

            bool stillRunning = false;
            int successCount = 0;
            int failureCount = 0;

            foreach (var child in children)
            {
                switch (child.Update())
                {
                    case NodeState.Success:
                        successCount++;
                        break;
                    case NodeState.Failure:
                        failureCount++;
                        if (_failOnAny)
                            return NodeState.Failure;
                        break;
                    case NodeState.Running:
                        stillRunning = true;
                        break;
                }
            }

            if (_succeedOnAll && successCount == children.Count)
                return NodeState.Success;

            if (!_succeedOnAll && successCount > 0)
                return NodeState.Success;

            if (stillRunning)
                return NodeState.Running;

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 随机选择节点 - 随机选择一个子节点执行
    /// </summary>
    public class RandomSelectorNode : CompositeNode
    {
        private int _selectedChild = -1;

        protected override void OnStart()
        {
            _selectedChild = Random.Range(0, children.Count);
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            if (_selectedChild >= 0 && _selectedChild < children.Count)
            {
                return children[_selectedChild].Update();
            }

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 权重随机选择节点 - 根据权重随机选择子节点
    /// </summary>
    public class WeightedRandomSelectorNode : CompositeNode
    {
        private readonly float[] _weights;
        private int _selectedChild = -1;

        public WeightedRandomSelectorNode(params float[] weights)
        {
            this._weights = weights?.Clone() as float[];
        }

        protected override BehaviourNode CreateClone()
        {
            return new WeightedRandomSelectorNode(_weights);
        }

        protected override void OnStart()
        {
            if (_weights == null || _weights.Length != children.Count)
            {
                _selectedChild = Random.Range(0, children.Count);
                return;
            }

            float totalWeight = 0;
            foreach (float weight in _weights)
            {
                totalWeight += weight;
            }

            float randomValue = Random.Range(0, totalWeight);
            float currentWeight = 0;

            for (int i = 0; i < _weights.Length; i++)
            {
                currentWeight += _weights[i];
                if (randomValue <= currentWeight)
                {
                    _selectedChild = i;
                    break;
                }
            }
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            if (_selectedChild >= 0 && _selectedChild < children.Count)
            {
                return children[_selectedChild].Update();
            }

            return NodeState.Failure;
        }
    }
}
