using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "Inverter Node", menuName = "Dynamis/Behaviour Nodes/Decorator/Inverter")]
    public class InverterNode : DecoratorNode
    {
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
                default:
                    return NodeState.Failure;
            }
        }
    }
}
