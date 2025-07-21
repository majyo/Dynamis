using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "Sequence Node", menuName = "Dynamis/Behaviour Nodes/Composite/Sequence")]
    public class SequenceNode : CompositeNode
    {
        private int _currentChildIndex;

        protected override void OnStart()
        {
            _currentChildIndex = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (children.Count == 0)
                return NodeState.Success;

            var child = children[_currentChildIndex];
            
            switch (child.Update())
            {
                case NodeState.Running:
                    return NodeState.Running;
                    
                case NodeState.Failure:
                    return NodeState.Failure;
                    
                case NodeState.Success:
                    _currentChildIndex++;
                    break;
            }

            return _currentChildIndex == children.Count ? NodeState.Success : NodeState.Running;
        }

        protected override void OnReset()
        {
            _currentChildIndex = 0;
        }
    }
}
