using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "Wait Node", menuName = "Dynamis/Behaviour Nodes/Leaf/Wait")]
    public class WaitNode : LeafNode
    {
        [SerializeField] private float duration = 1.0f;
        private float _startTime;

        protected override void OnStart()
        {
            _startTime = Time.time;
        }

        protected override NodeState OnUpdate()
        {
            if (Time.time - _startTime > duration)
                return NodeState.Success;
            
            return NodeState.Running;
        }
    }
}
