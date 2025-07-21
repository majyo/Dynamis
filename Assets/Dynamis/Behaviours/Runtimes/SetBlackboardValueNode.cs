using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 设置黑板值的节点
    /// </summary>
    [CreateAssetMenu(fileName = "Set Blackboard Value", menuName = "Dynamis/Behaviour Tree/Blackboard/Set Value")]
    public class SetBlackboardValueNode : LeafNode
    {
        [SerializeField] private string key;
        [SerializeField] private BlackboardValueType valueType = BlackboardValueType.String;
        
        [Header("Values")]
        [SerializeField] private string stringValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private Vector3 vector3Value;

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
                Debug.LogWarning($"SetBlackboardValueNode: Key is empty");
                return NodeState.Failure;
            }

            try
            {
                switch (valueType)
                {
                    case BlackboardValueType.String:
                        SetBlackboardValue(key, stringValue);
                        break;
                    case BlackboardValueType.Int:
                        SetBlackboardValue(key, intValue);
                        break;
                    case BlackboardValueType.Float:
                        SetBlackboardValue(key, floatValue);
                        break;
                    case BlackboardValueType.Bool:
                        SetBlackboardValue(key, boolValue);
                        break;
                    case BlackboardValueType.Vector3:
                        SetBlackboardValue(key, vector3Value);
                        break;
                    default:
                        Debug.LogWarning($"SetBlackboardValueNode: Unsupported value type {valueType}");
                        return NodeState.Failure;
                }

                return NodeState.Success;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SetBlackboardValueNode: Error setting value for key '{key}': {e.Message}");
                return NodeState.Failure;
            }
        }

        public void SetStringValue(string value)
        {
            stringValue = value;
            valueType = BlackboardValueType.String;
        }

        public void SetIntValue(int value)
        {
            intValue = value;
            valueType = BlackboardValueType.Int;
        }

        public void SetFloatValue(float value)
        {
            floatValue = value;
            valueType = BlackboardValueType.Float;
        }

        public void SetBoolValue(bool value)
        {
            boolValue = value;
            valueType = BlackboardValueType.Bool;
        }

        public void SetVector3Value(Vector3 value)
        {
            vector3Value = value;
            valueType = BlackboardValueType.Vector3;
        }
    }

    /// <summary>
    /// 黑板值类型枚举
    /// </summary>
    public enum BlackboardValueType
    {
        String,
        Int,
        Float,
        Bool,
        Vector3
    }
}
