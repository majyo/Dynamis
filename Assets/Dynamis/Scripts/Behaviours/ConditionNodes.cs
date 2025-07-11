using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 黑板条件节点 - 检查黑板中的值
    /// </summary>
    public class BlackboardConditionNode : ConditionNode
    {
        private string key;
        private object expectedValue;
        private CompareType compareType = CompareType.Equals;

        public enum CompareType
        {
            Equals,
            NotEquals,
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual,
            Exists,
            NotExists
        }

        public BlackboardConditionNode(string key, object expectedValue, CompareType compareType = CompareType.Equals)
        {
            this.key = key;
            this.expectedValue = expectedValue;
            this.compareType = compareType;
        }

        protected override BehaviourNode CreateClone()
        {
            return new BlackboardConditionNode(key, expectedValue, compareType);
        }

        protected override NodeState OnUpdate()
        {
            if (blackboard == null || string.IsNullOrEmpty(key))
                return NodeState.Failure;

            switch (compareType)
            {
                case CompareType.Exists:
                    return blackboard.HasKey(key) ? NodeState.Success : NodeState.Failure;
                
                case CompareType.NotExists:
                    return !blackboard.HasKey(key) ? NodeState.Success : NodeState.Failure;
            }

            if (!blackboard.HasKey(key))
                return NodeState.Failure;

            var value = blackboard.GetValue<object>(key);
            
            if (expectedValue is float expectedFloat && value is float actualFloat)
            {
                return CompareFloat(actualFloat, expectedFloat) ? NodeState.Success : NodeState.Failure;
            }
            else if (expectedValue is int expectedInt && value is int actualInt)
            {
                return CompareInt(actualInt, expectedInt) ? NodeState.Success : NodeState.Failure;
            }
            else if (expectedValue is bool expectedBool && value is bool actualBool)
            {
                return CompareBool(actualBool, expectedBool) ? NodeState.Success : NodeState.Failure;
            }
            else
            {
                return CompareObject(value, expectedValue) ? NodeState.Success : NodeState.Failure;
            }
        }

        private bool CompareFloat(float actual, float expected)
        {
            switch (compareType)
            {
                case CompareType.Equals: return Mathf.Approximately(actual, expected);
                case CompareType.NotEquals: return !Mathf.Approximately(actual, expected);
                case CompareType.Greater: return actual > expected;
                case CompareType.GreaterOrEqual: return actual >= expected;
                case CompareType.Less: return actual < expected;
                case CompareType.LessOrEqual: return actual <= expected;
                default: return false;
            }
        }

        private bool CompareInt(int actual, int expected)
        {
            switch (compareType)
            {
                case CompareType.Equals: return actual == expected;
                case CompareType.NotEquals: return actual != expected;
                case CompareType.Greater: return actual > expected;
                case CompareType.GreaterOrEqual: return actual >= expected;
                case CompareType.Less: return actual < expected;
                case CompareType.LessOrEqual: return actual <= expected;
                default: return false;
            }
        }

        private bool CompareBool(bool actual, bool expected)
        {
            switch (compareType)
            {
                case CompareType.Equals: return actual == expected;
                case CompareType.NotEquals: return actual != expected;
                default: return false;
            }
        }

        private bool CompareObject(object actual, object expected)
        {
            switch (compareType)
            {
                case CompareType.Equals: return Equals(actual, expected);
                case CompareType.NotEquals: return !Equals(actual, expected);
                default: return false;
            }
        }
    }

    /// <summary>
    /// 设置黑板值节点
    /// </summary>
    public class SetBlackboardValueNode : ActionNode
    {
        private string key;
        private object value;

        public SetBlackboardValueNode(string key, object value)
        {
            this.key = key;
            this.value = value;
        }

        protected override BehaviourNode CreateClone()
        {
            return new SetBlackboardValueNode(key, value);
        }

        protected override NodeState OnUpdate()
        {
            if (blackboard == null || string.IsNullOrEmpty(key))
                return NodeState.Failure;

            blackboard.SetValue(key, value);
            return NodeState.Success;
        }
    }

    /// <summary>
    /// 距离条件节点 - 检查与目标的距离
    /// </summary>
    public class DistanceConditionNode : ConditionNode
    {
        private Transform target;
        private string targetKey; // 从黑板获取目标
        private float distance;
        private CompareType compareType = CompareType.Less;

        public enum CompareType
        {
            Less,
            LessOrEqual,
            Greater,
            GreaterOrEqual,
            Equals
        }

        public DistanceConditionNode(Transform target, float distance, CompareType compareType = CompareType.Less)
        {
            this.target = target;
            this.distance = distance;
            this.compareType = compareType;
        }

        public DistanceConditionNode(string targetKey, float distance, CompareType compareType = CompareType.Less)
        {
            this.targetKey = targetKey;
            this.distance = distance;
            this.compareType = compareType;
        }

        protected override BehaviourNode CreateClone()
        {
            if (target != null)
                return new DistanceConditionNode(target, distance, compareType);
            else
                return new DistanceConditionNode(targetKey, distance, compareType);
        }

        protected override NodeState OnUpdate()
        {
            if (transform == null)
                return NodeState.Failure;

            Transform targetTransform = target;
            
            // 如果没有直接设置目标，尝试从黑板获取
            if (targetTransform == null && !string.IsNullOrEmpty(targetKey) && blackboard != null)
            {
                targetTransform = blackboard.GetValue<Transform>(targetKey);
                if (targetTransform == null)
                {
                    var targetGameObject = blackboard.GetValue<GameObject>(targetKey);
                    if (targetGameObject != null)
                        targetTransform = targetGameObject.transform;
                }
            }

            if (targetTransform == null)
                return NodeState.Failure;

            float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
            
            switch (compareType)
            {
                case CompareType.Less:
                    return currentDistance < distance ? NodeState.Success : NodeState.Failure;
                case CompareType.LessOrEqual:
                    return currentDistance <= distance ? NodeState.Success : NodeState.Failure;
                case CompareType.Greater:
                    return currentDistance > distance ? NodeState.Success : NodeState.Failure;
                case CompareType.GreaterOrEqual:
                    return currentDistance >= distance ? NodeState.Success : NodeState.Failure;
                case CompareType.Equals:
                    return Mathf.Approximately(currentDistance, distance) ? NodeState.Success : NodeState.Failure;
                default:
                    return NodeState.Failure;
            }
        }
    }

    /// <summary>
    /// 时间条件节点 - 检查经过的时间
    /// </summary>
    public class TimeConditionNode : ConditionNode
    {
        private float targetTime;
        private string timeKey; // 从黑板获取开始时间
        private float startTime;

        public TimeConditionNode(float targetTime)
        {
            this.targetTime = targetTime;
        }

        public TimeConditionNode(string timeKey, float targetTime)
        {
            this.timeKey = timeKey;
            this.targetTime = targetTime;
        }

        protected override BehaviourNode CreateClone()
        {
            if (!string.IsNullOrEmpty(timeKey))
                return new TimeConditionNode(timeKey, targetTime);
            else
                return new TimeConditionNode(targetTime);
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            if (!string.IsNullOrEmpty(timeKey) && blackboard != null)
            {
                startTime = blackboard.GetValue<float>(timeKey, Time.time);
            }
            else
            {
                startTime = Time.time;
            }
        }

        protected override NodeState OnUpdate()
        {
            float elapsedTime = Time.time - startTime;
            return elapsedTime >= targetTime ? NodeState.Success : NodeState.Failure;
        }
    }

    /// <summary>
    /// 随机条件节点 - 根据概率随机返回成功或失败
    /// </summary>
    public class RandomConditionNode : ConditionNode
    {
        private float successProbability;

        public RandomConditionNode(float successProbability = 0.5f)
        {
            this.successProbability = Mathf.Clamp01(successProbability);
        }

        protected override BehaviourNode CreateClone()
        {
            return new RandomConditionNode(successProbability);
        }

        protected override NodeState OnUpdate()
        {
            return Random.Range(0f, 1f) <= successProbability ? NodeState.Success : NodeState.Failure;
        }
    }
}
