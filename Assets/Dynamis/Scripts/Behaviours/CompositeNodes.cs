using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 复合节点基类
    /// </summary>
    public abstract class CompositeNode : BehaviourNode
    {
        protected int currentChild = 0;

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
        private bool failOnAny;
        private bool succeedOnAll;

        public ParallelNode(bool failOnAny = false, bool succeedOnAll = true)
        {
            this.failOnAny = failOnAny;
            this.succeedOnAll = succeedOnAll;
        }

        protected override BehaviourNode CreateClone()
        {
            return new ParallelNode(failOnAny, succeedOnAll);
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
                        if (failOnAny)
                            return NodeState.Failure;
                        break;
                    case NodeState.Running:
                        stillRunning = true;
                        break;
                }
            }

            if (succeedOnAll && successCount == children.Count)
                return NodeState.Success;

            if (!succeedOnAll && successCount > 0)
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
        private int selectedChild = -1;

        protected override void OnStart()
        {
            selectedChild = Random.Range(0, children.Count);
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            if (selectedChild >= 0 && selectedChild < children.Count)
            {
                return children[selectedChild].Update();
            }

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 权重随机选择节点 - 根据权重随机选择子节点
    /// </summary>
    public class WeightedRandomSelectorNode : CompositeNode
    {
        private float[] weights;
        private int selectedChild = -1;

        public WeightedRandomSelectorNode(params float[] weights)
        {
            this.weights = weights?.Clone() as float[];
        }

        protected override BehaviourNode CreateClone()
        {
            return new WeightedRandomSelectorNode(weights);
        }

        protected override void OnStart()
        {
            if (weights == null || weights.Length != children.Count)
            {
                selectedChild = Random.Range(0, children.Count);
                return;
            }

            float totalWeight = 0;
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            float randomValue = Random.Range(0, totalWeight);
            float currentWeight = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue <= currentWeight)
                {
                    selectedChild = i;
                    break;
                }
            }
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            if (selectedChild >= 0 && selectedChild < children.Count)
            {
                return children[selectedChild].Update();
            }

            return NodeState.Failure;
        }
    }
}
