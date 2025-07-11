using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 条件装饰器基类 - 根据条件决定是否执行子节点
    /// </summary>
    public abstract class ConditionNode : DecoratorNode
    {
        /// <summary>
        /// 评估条件
        /// </summary>
        /// <returns>条件是否满足</returns>
        protected abstract bool EvaluateCondition();

        protected override NodeState OnUpdate()
        {
            if (child == null)
                return NodeState.Failure;

            // 如果条件不满足，直接返回失败
            if (!EvaluateCondition())
                return NodeState.Failure;

            // 条件满足，执行子节点
            return child.Update();
        }
    }

    /// <summary>
    /// 黑板条件装饰器 - 检查黑板中的值
    /// </summary>
    public class BlackboardConditionNode : ConditionNode
    {
        private string key;
        private object expectedValue;
        private CompareType compareType;

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

        public BlackboardConditionNode(string key, object expectedValue = null, CompareType compareType = CompareType.Exists)
        {
            this.key = key;
            this.expectedValue = expectedValue;
            this.compareType = compareType;
        }

        protected override BehaviourNode CreateClone()
        {
            return new BlackboardConditionNode(key, expectedValue, compareType);
        }

        protected override bool EvaluateCondition()
        {
            if (blackboard == null || string.IsNullOrEmpty(key))
                return false;

            switch (compareType)
            {
                case CompareType.Exists:
                    return blackboard.HasKey(key);
                
                case CompareType.NotExists:
                    return !blackboard.HasKey(key);
                
                default:
                    if (!blackboard.HasKey(key))
                        return false;
                    
                    var value = blackboard.GetValue<object>(key);
                    return CompareValues(value, expectedValue);
            }
        }

        private bool CompareValues(object actual, object expected)
        {
            if (expected is float expectedFloat && actual is float actualFloat)
            {
                return CompareFloat(actualFloat, expectedFloat);
            }
            else if (expected is int expectedInt && actual is int actualInt)
            {
                return CompareInt(actualInt, expectedInt);
            }
            else if (expected is bool expectedBool && actual is bool actualBool)
            {
                return CompareBool(actualBool, expectedBool);
            }
            else
            {
                return CompareObject(actual, expected);
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
    /// 距离条件装饰器 - 检查与目标的距离
    /// </summary>
    public class DistanceConditionNode : ConditionNode
    {
        private Transform target;
        private string targetKey;
        private float distance;
        private CompareType compareType;

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

        protected override bool EvaluateCondition()
        {
            if (transform == null)
                return false;

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
                return false;

            float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
            
            switch (compareType)
            {
                case CompareType.Less:
                    return currentDistance < distance;
                case CompareType.LessOrEqual:
                    return currentDistance <= distance;
                case CompareType.Greater:
                    return currentDistance > distance;
                case CompareType.GreaterOrEqual:
                    return currentDistance >= distance;
                case CompareType.Equals:
                    return Mathf.Approximately(currentDistance, distance);
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// 时间条件装饰器 - 检查经过的时间
    /// </summary>
    public class TimeConditionNode : ConditionNode
    {
        private float targetTime;
        private string timeKey;
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
                startTime = blackboard.GetValue("startTime", Time.time);
            }
            else
            {
                startTime = Time.time;
            }
        }

        protected override bool EvaluateCondition()
        {
            float elapsedTime = Time.time - startTime;
            return elapsedTime >= targetTime;
        }
    }

    /// <summary>
    /// 随机条件装饰器 - 根据概率随机判断条件
    /// </summary>
    public class RandomConditionNode : ConditionNode
    {
        private float successProbability;
        private bool hasEvaluated = false;
        private bool lastResult = false;

        public RandomConditionNode(float successProbability = 0.5f)
        {
            this.successProbability = Mathf.Clamp01(successProbability);
        }

        protected override BehaviourNode CreateClone()
        {
            return new RandomConditionNode(successProbability);
        }

        protected override void OnStart()
        {
            base.OnStart();
            hasEvaluated = false;
        }

        protected override bool EvaluateCondition()
        {
            // 每次开始时只评估一次，避免随机结果变化
            if (!hasEvaluated)
            {
                lastResult = Random.Range(0f, 1f) <= successProbability;
                hasEvaluated = true;
            }
            return lastResult;
        }
    }

    /// <summary>
    /// 冷却条件装饰器 - 在冷却时间内阻止执行
    /// </summary>
    public class CooldownConditionNode : ConditionNode
    {
        private float cooldownTime;
        private float lastExecutionTime = -1;

        public CooldownConditionNode(float cooldownTime = 1.0f)
        {
            this.cooldownTime = cooldownTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CooldownConditionNode(cooldownTime);
        }

        protected override bool EvaluateCondition()
        {
            if (lastExecutionTime > 0 && Time.time - lastExecutionTime < cooldownTime)
            {
                return false;
            }
            return true;
        }

        protected override NodeState OnUpdate()
        {
            if (!EvaluateCondition())
                return NodeState.Failure;

            var result = child.Update();
            
            // 记录执行时间
            if (result == NodeState.Success || result == NodeState.Failure)
            {
                lastExecutionTime = Time.time;
            }
            
            return result;
        }
    }

    /// <summary>
    /// 自定义条件装饰器 - 使用自定义函数评估条件
    /// </summary>
    public class CustomConditionNode : ConditionNode
    {
        private System.Func<bool> conditionFunc;

        public CustomConditionNode(System.Func<bool> conditionFunc)
        {
            this.conditionFunc = conditionFunc;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CustomConditionNode(conditionFunc);
        }

        protected override bool EvaluateCondition()
        {
            return conditionFunc?.Invoke() ?? false;
        }
    }
}
