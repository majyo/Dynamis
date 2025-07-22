using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public enum ParallelPolicy
    {
        RequireOne,  // Succeed when one child succeeds
        RequireAll   // Succeed when all children succeed
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

            // Check failure policy
            if (failurePolicy == ParallelPolicy.RequireOne && failureCount > 0)
                return NodeState.Failure;
            if (failurePolicy == ParallelPolicy.RequireAll && failureCount == children.Count)
                return NodeState.Failure;

            // Check success policy
            if (successPolicy == ParallelPolicy.RequireOne && successCount > 0)
                return NodeState.Success;
            if (successPolicy == ParallelPolicy.RequireAll && successCount == children.Count)
                return NodeState.Success;

            return NodeState.Running;
        }
    }
}
