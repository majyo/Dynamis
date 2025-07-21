using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 检查黑板键是否存在的节点
    /// </summary>
    [CreateAssetMenu(fileName = "Check Blackboard Key Exists", menuName = "Dynamis/Behaviour Tree/Blackboard/Check Key Exists")]
    public class CheckBlackboardKeyExistsNode : LeafNode
    {
        [SerializeField] private string key;
        [SerializeField] private bool invertResult = false; // 如果为true，则当键不存在时返回Success

        public string Key 
        { 
            get => key; 
            set => key = value; 
        }

        public bool InvertResult 
        { 
            get => invertResult; 
            set => invertResult = value; 
        }

        protected override NodeState OnUpdate()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"CheckBlackboardKeyExistsNode: Key is empty");
                return NodeState.Failure;
            }

            bool keyExists = HasBlackboardKey(key);
            bool result = invertResult ? !keyExists : keyExists;
            
            return result ? NodeState.Success : NodeState.Failure;
        }
    }

    /// <summary>
    /// 清除黑板键的节点
    /// </summary>
    [CreateAssetMenu(fileName = "Clear Blackboard Key", menuName = "Dynamis/Behaviour Tree/Blackboard/Clear Key")]
    public class ClearBlackboardKeyNode : LeafNode
    {
        [SerializeField] private string key;

        public string Key 
        { 
            get => key; 
            set => key = value; 
        }

        protected override NodeState OnUpdate()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"ClearBlackboardKeyNode: Key is empty");
                return NodeState.Failure;
            }

            if (Blackboard != null)
            {
                bool removed = Blackboard.RemoveKey(key);
                return removed ? NodeState.Success : NodeState.Failure;
            }

            return NodeState.Failure;
        }
    }

    /// <summary>
    /// 获取黑板值并显示调试信息的节点
    /// </summary>
    [CreateAssetMenu(fileName = "Debug Blackboard Value", menuName = "Dynamis/Behaviour Tree/Blackboard/Debug Value")]
    public class DebugBlackboardValueNode : LeafNode
    {
        [SerializeField] private string key;
        [SerializeField] private BlackboardValueType valueType = BlackboardValueType.String;
        [SerializeField] private bool logToConsole = true;

        public string Key 
        { 
            get => key; 
            set => key = value; 
        }

        public BlackboardValueType ValueType 
        { 
            get => valueType; 
            set => valueType = value; 
        }

        protected override NodeState OnUpdate()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"DebugBlackboardValueNode: Key is empty");
                return NodeState.Failure;
            }

            if (!HasBlackboardKey(key))
            {
                if (logToConsole)
                    Debug.Log($"Blackboard key '{key}' does not exist");
                return NodeState.Failure;
            }

            try
            {
                object value = null;
                switch (valueType)
                {
                    case BlackboardValueType.String:
                        value = GetBlackboardValue<string>(key);
                        break;
                    case BlackboardValueType.Int:
                        value = GetBlackboardValue<int>(key);
                        break;
                    case BlackboardValueType.Float:
                        value = GetBlackboardValue<float>(key);
                        break;
                    case BlackboardValueType.Bool:
                        value = GetBlackboardValue<bool>(key);
                        break;
                    case BlackboardValueType.Vector3:
                        value = GetBlackboardValue<Vector3>(key);
                        break;
                }

                if (logToConsole)
                    Debug.Log($"Blackboard['{key}'] = {value} (Type: {valueType})");

                return NodeState.Success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DebugBlackboardValueNode: Error getting value for key '{key}': {e.Message}");
                return NodeState.Failure;
            }
        }
    }
}
