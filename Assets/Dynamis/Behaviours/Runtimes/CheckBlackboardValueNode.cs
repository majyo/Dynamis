using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 检查黑板值的节点 - 根据黑板中的值返回成功或失败
    /// </summary>
    [CreateAssetMenu(fileName = "Check Blackboard Value", menuName = "Dynamis/Behaviour Tree/Blackboard/Check Value")]
    public class CheckBlackboardValueNode : LeafNode
    {
        [SerializeField] private string key;
        [SerializeField] private BlackboardValueType valueType = BlackboardValueType.Bool;
        [SerializeField] private ComparisonOperator comparisonOperator = ComparisonOperator.Equals;
        
        [Header("Expected Values")]
        [SerializeField] private string expectedStringValue;
        [SerializeField] private int expectedIntValue;
        [SerializeField] private float expectedFloatValue;
        [SerializeField] private bool expectedBoolValue;
        [SerializeField] private Vector3 expectedVector3Value;
        [SerializeField] private float tolerance = 0.01f; // For float and Vector3 comparisons

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

        public ComparisonOperator Operator 
        { 
            get => comparisonOperator; 
            set => comparisonOperator = value; 
        }

        protected override NodeState OnUpdate()
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"CheckBlackboardValueNode: Key is empty");
                return NodeState.Failure;
            }

            if (!HasBlackboardKey(key))
            {
                return NodeState.Failure;
            }

            try
            {
                bool result = false;

                switch (valueType)
                {
                    case BlackboardValueType.String:
                        result = CompareString(GetBlackboardValue<string>(key));
                        break;
                    case BlackboardValueType.Int:
                        result = CompareInt(GetBlackboardValue<int>(key));
                        break;
                    case BlackboardValueType.Float:
                        result = CompareFloat(GetBlackboardValue<float>(key));
                        break;
                    case BlackboardValueType.Bool:
                        result = CompareBool(GetBlackboardValue<bool>(key));
                        break;
                    case BlackboardValueType.Vector3:
                        result = CompareVector3(GetBlackboardValue<Vector3>(key));
                        break;
                    default:
                        Debug.LogWarning($"CheckBlackboardValueNode: Unsupported value type {valueType}");
                        return NodeState.Failure;
                }

                return result ? NodeState.Success : NodeState.Failure;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CheckBlackboardValueNode: Error checking value for key '{key}': {e.Message}");
                return NodeState.Failure;
            }
        }

        private bool CompareString(string actualValue)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return actualValue == expectedStringValue;
                case ComparisonOperator.NotEquals:
                    return actualValue != expectedStringValue;
                default:
                    Debug.LogWarning($"CheckBlackboardValueNode: Unsupported operator {comparisonOperator} for string comparison");
                    return false;
            }
        }

        private bool CompareInt(int actualValue)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return actualValue == expectedIntValue;
                case ComparisonOperator.NotEquals:
                    return actualValue != expectedIntValue;
                case ComparisonOperator.GreaterThan:
                    return actualValue > expectedIntValue;
                case ComparisonOperator.GreaterThanOrEqual:
                    return actualValue >= expectedIntValue;
                case ComparisonOperator.LessThan:
                    return actualValue < expectedIntValue;
                case ComparisonOperator.LessThanOrEqual:
                    return actualValue <= expectedIntValue;
                default:
                    return false;
            }
        }

        private bool CompareFloat(float actualValue)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return Mathf.Abs(actualValue - expectedFloatValue) <= tolerance;
                case ComparisonOperator.NotEquals:
                    return Mathf.Abs(actualValue - expectedFloatValue) > tolerance;
                case ComparisonOperator.GreaterThan:
                    return actualValue > expectedFloatValue;
                case ComparisonOperator.GreaterThanOrEqual:
                    return actualValue >= expectedFloatValue;
                case ComparisonOperator.LessThan:
                    return actualValue < expectedFloatValue;
                case ComparisonOperator.LessThanOrEqual:
                    return actualValue <= expectedFloatValue;
                default:
                    return false;
            }
        }

        private bool CompareBool(bool actualValue)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return actualValue == expectedBoolValue;
                case ComparisonOperator.NotEquals:
                    return actualValue != expectedBoolValue;
                default:
                    Debug.LogWarning($"CheckBlackboardValueNode: Unsupported operator {comparisonOperator} for bool comparison");
                    return false;
            }
        }

        private bool CompareVector3(Vector3 actualValue)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.Equals:
                    return Vector3.Distance(actualValue, expectedVector3Value) <= tolerance;
                case ComparisonOperator.NotEquals:
                    return Vector3.Distance(actualValue, expectedVector3Value) > tolerance;
                default:
                    Debug.LogWarning($"CheckBlackboardValueNode: Unsupported operator {comparisonOperator} for Vector3 comparison");
                    return false;
            }
        }
    }

    /// <summary>
    /// 比较操作符枚举
    /// </summary>
    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
}
