using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "Repeater Node", menuName = "Dynamis/Behaviour Nodes/Decorator/Repeater")]
    public class RepeaterNode : DecoratorNode
    {
        [SerializeField] private int maxRepeats = -1; // -1 means infinite repeats
        [SerializeField] private bool restartOnSuccess = true;
        [SerializeField] private bool restartOnFailure;
        
        private int _currentRepeats;

        protected override void OnStart()
        {
            _currentRepeats = 0;
        }

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            var childState = child.Update();

            switch (childState)
            {
                case NodeState.Running:
                    return NodeState.Running;
                    
                case NodeState.Success:
                    if (restartOnSuccess)
                    {
                        _currentRepeats++;
                        if (maxRepeats > 0 && _currentRepeats >= maxRepeats)
                            return NodeState.Success;
                        
                        child.ResetNode();
                        return NodeState.Running;
                    }
                    return NodeState.Success;
                    
                case NodeState.Failure:
                    if (restartOnFailure)
                    {
                        _currentRepeats++;
                        if (maxRepeats > 0 && _currentRepeats >= maxRepeats)
                            return NodeState.Failure;
                        
                        child.ResetNode();
                        return NodeState.Running;
                    }
                    return NodeState.Failure;
            }

            return NodeState.Failure;
        }

        protected override void OnReset()
        {
            _currentRepeats = 0;
        }
    }
}
