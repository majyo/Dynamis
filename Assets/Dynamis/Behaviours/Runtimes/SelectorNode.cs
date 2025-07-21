using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "Selector Node", menuName = "Dynamis/Behaviour Nodes/Composite/Selector")]
    public class SelectorNode : CompositeNode
    {
        private int _currentChildIndex;

        protected override void OnStart()
        {
            _currentChildIndex = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Failure;

            var child = children[_currentChildIndex];
            
            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                    
                case NodeState.Success:
                    return NodeState.Success;
                    
                case NodeState.Failure:
                    _currentChildIndex++;
                    break;
            }

            return _currentChildIndex == children.Count ? NodeState.Failure : NodeState.Running;
        }

        protected override void OnReset()
        {
            _currentChildIndex = 0;
        }
    }
}
