using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public class DebugLogNode : LeafNode
    {
        [SerializeField] private string _message = "Debug Log Node Executed";
        [SerializeField] private LogType _logType = LogType.Log;

        protected override NodeState OnUpdate()
        {
            switch (_logType)
            {
                case LogType.Log:
                    Debug.Log(_message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(_message);
                    break;
                case LogType.Error:
                    Debug.LogError(_message);
                    break;
            }
            
            return NodeState.Success;
        }
    }
}
