using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
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
        private readonly string _key;
        private readonly object _expectedValue;
        private readonly CompareType _compareType;

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
            this._key = key;
            this._expectedValue = expectedValue;
            this._compareType = compareType;
        }

        protected override BehaviourNode CreateClone()
        {
            return new BlackboardConditionNode(_key, _expectedValue, _compareType);
        }

        protected override bool EvaluateCondition()
        {
            if (blackboard == null || string.IsNullOrEmpty(_key))
                return false;

            switch (_compareType)
            {
                case CompareType.Exists:
                    return blackboard.HasKey(_key);
                
                case CompareType.NotExists:
                    return !blackboard.HasKey(_key);
                
                default:
                    if (!blackboard.HasKey(_key))
                        return false;
                    
                    var value = blackboard.GetValue<object>(_key);
                    return CompareValues(value, _expectedValue);
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
            switch (_compareType)
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
            switch (_compareType)
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
            switch (_compareType)
            {
                case CompareType.Equals: return actual == expected;
                case CompareType.NotEquals: return actual != expected;
                default: return false;
            }
        }

        private bool CompareObject(object actual, object expected)
        {
            switch (_compareType)
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
        private readonly Transform _target;
        private readonly string _targetKey;
        private readonly float _distance;
        private readonly CompareType _compareType;

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
            this._target = target;
            this._distance = distance;
            this._compareType = compareType;
        }

        public DistanceConditionNode(string targetKey, float distance, CompareType compareType = CompareType.Less)
        {
            this._targetKey = targetKey;
            this._distance = distance;
            this._compareType = compareType;
        }

        protected override BehaviourNode CreateClone()
        {
            if (_target != null)
                return new DistanceConditionNode(_target, _distance, _compareType);
            else
                return new DistanceConditionNode(_targetKey, _distance, _compareType);
        }

        protected override bool EvaluateCondition()
        {
            if (transform == null)
                return false;

            Transform targetTransform = _target;
            
            // 如果没有直接设置目标，尝试从黑板获取
            if (targetTransform == null && !string.IsNullOrEmpty(_targetKey) && blackboard != null)
            {
                targetTransform = blackboard.GetValue<Transform>(_targetKey);
                if (targetTransform == null)
                {
                    var targetGameObject = blackboard.GetValue<GameObject>(_targetKey);
                    if (targetGameObject != null)
                        targetTransform = targetGameObject.transform;
                }
            }

            if (targetTransform == null)
                return false;

            float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
            
            switch (_compareType)
            {
                case CompareType.Less:
                    return currentDistance < _distance;
                case CompareType.LessOrEqual:
                    return currentDistance <= _distance;
                case CompareType.Greater:
                    return currentDistance > _distance;
                case CompareType.GreaterOrEqual:
                    return currentDistance >= _distance;
                case CompareType.Equals:
                    return Mathf.Approximately(currentDistance, _distance);
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
        private readonly float _targetTime;
        private readonly string _timeKey;
        private float _startTime;

        public TimeConditionNode(float targetTime)
        {
            this._targetTime = targetTime;
        }

        public TimeConditionNode(string timeKey, float targetTime)
        {
            this._timeKey = timeKey;
            this._targetTime = targetTime;
        }

        protected override BehaviourNode CreateClone()
        {
            if (!string.IsNullOrEmpty(_timeKey))
                return new TimeConditionNode(_timeKey, _targetTime);
            else
                return new TimeConditionNode(_targetTime);
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            if (!string.IsNullOrEmpty(_timeKey) && blackboard != null)
            {
                _startTime = blackboard.GetValue("startTime", Time.time);
            }
            else
            {
                _startTime = Time.time;
            }
        }

        protected override bool EvaluateCondition()
        {
            float elapsedTime = Time.time - _startTime;
            return elapsedTime >= _targetTime;
        }
    }

    /// <summary>
    /// 随机条件装饰器 - 根据概率随机判断条件
    /// </summary>
    public class RandomConditionNode : ConditionNode
    {
        private readonly float _successProbability;
        private bool _hasEvaluated;
        private bool _lastResult;

        public RandomConditionNode(float successProbability = 0.5f)
        {
            this._successProbability = Mathf.Clamp01(successProbability);
        }

        protected override BehaviourNode CreateClone()
        {
            return new RandomConditionNode(_successProbability);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _hasEvaluated = false;
        }

        protected override bool EvaluateCondition()
        {
            // 每次开始时只评估一次，避免随机结果变化
            if (!_hasEvaluated)
            {
                _lastResult = Random.Range(0f, 1f) <= _successProbability;
                _hasEvaluated = true;
            }
            return _lastResult;
        }
    }

    /// <summary>
    /// 冷却条件装饰器 - 在冷却时间内阻止执行
    /// </summary>
    public class CooldownConditionNode : ConditionNode
    {
        private readonly float _cooldownTime;
        private float _lastExecutionTime = -1;

        public CooldownConditionNode(float cooldownTime = 1.0f)
        {
            this._cooldownTime = cooldownTime;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CooldownConditionNode(_cooldownTime);
        }

        protected override bool EvaluateCondition()
        {
            if (_lastExecutionTime > 0 && Time.time - _lastExecutionTime < _cooldownTime)
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
                _lastExecutionTime = Time.time;
            }
            
            return result;
        }
    }

    /// <summary>
    /// 自定义条件装饰器 - 使用自定义函数评估条件
    /// </summary>
    public class CustomConditionNode : ConditionNode
    {
        private readonly System.Func<bool> _conditionFunc;

        public CustomConditionNode(System.Func<bool> conditionFunc)
        {
            this._conditionFunc = conditionFunc;
        }

        protected override BehaviourNode CreateClone()
        {
            return new CustomConditionNode(_conditionFunc);
        }

        protected override bool EvaluateCondition()
        {
            return _conditionFunc?.Invoke() ?? false;
        }
    }
}
