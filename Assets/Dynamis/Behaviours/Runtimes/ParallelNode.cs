using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public enum ParallelPolicy
    {
        RequireOne,  // 需要一个子节点成功
        RequireAll   // 需要所有子节点成功
    }

    [CreateAssetMenu(fileName = "Parallel Node", menuName = "Dynamis/Behaviour Nodes/Composite/Parallel")]
    public class ParallelNode : CompositeNode
    {
        [SerializeField] private ParallelPolicy successPolicy = ParallelPolicy.RequireAll;
        [SerializeField] private ParallelPolicy failurePolicy = ParallelPolicy.RequireOne;

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Success;

            int successCount = 0;
            int failureCount = 0;

            foreach (var child in children)
            {
                if (child == null) continue;

                switch (child.Update())
                {
                    case NodeState.Success:
                        successCount++;
                        break;
                    case NodeState.Failure:
                        failureCount++;
                        break;
                }
            }

            // 检查失败条件
            if (failurePolicy == ParallelPolicy.RequireOne && failureCount > 0)
                return NodeState.Failure;
            if (failurePolicy == ParallelPolicy.RequireAll && failureCount == children.Count)
                return NodeState.Failure;

            // 检查成功条件
            if (successPolicy == ParallelPolicy.RequireOne && successCount > 0)
                return NodeState.Success;
            if (successPolicy == ParallelPolicy.RequireAll && successCount == children.Count)
                return NodeState.Success;

            return NodeState.Running;
        }
    }
}
