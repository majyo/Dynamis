using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    /// <summary>
    /// 现代行为树示例 - 使用条件装饰器
    /// </summary>
    public class ModernBehaviourTreeExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float patrolDistance = 10f;
        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private float attackDistance = 2f;
        
        private BehaviourTree _behaviourTree;

        private void Start()
        {
            CreateModernBehaviourTree();
        }

        /// <summary>
        /// 创建使用条件装饰器的现代行为树
        /// </summary>
        private void CreateModernBehaviourTree()
        {
            var tree = BehaviourTreeBuilder.Create()
                .Root<SelectorNode>() // 主选择器
                    
                    // 攻击行为 - 条件装饰器包装动作
                    .BlackboardCondition("Attack Condition", "target", null, BlackboardConditionNode.CompareType.NotEquals)
                        .DistanceCondition("Attack Range", "target", attackDistance, DistanceConditionNode.CompareType.Less)
                            .Sequence("Attack Sequence")
                                .Log("Attack Log", "开始攻击目标！")
                                .Node(new AttackAction(), true)
                                .CooldownCondition("Attack Cooldown", 2f) // 攻击冷却
                                    .Node(new SpecialAttackAction(), true)
                                    .Back()
                                .Back()
                            .Back()
                        .Back()
                    
                    // 追击行为 - 多层条件装饰器
                    .BlackboardCondition("Chase Condition", "target", null, BlackboardConditionNode.CompareType.NotEquals)
                        .DistanceCondition("Chase Range", "target", chaseDistance, DistanceConditionNode.CompareType.Less)
                            .DistanceCondition("Not In Attack Range", "target", attackDistance, DistanceConditionNode.CompareType.Greater)
                                .Sequence("Chase Sequence")
                                    .Log("Chase Log", "开始追击目标！")
                                    .Node(new ChaseAction(), true)
                                    .Back()
                                .Back()
                            .Back()
                        .Back()
                    
                    // 巡逻行为 - 使用随机条件增加变化
                    .RandomCondition("Patrol Random", 0.8f) // 80%概率执行巡逻
                        .Sequence("Patrol Sequence")
                            .Log("Patrol Log", "开始巡逻")
                            .Node(new PatrolAction(), true)
                            .Wait("Patrol Wait", 2f)
                            .Back()
                        .Back()
                    
                    // 空闲行为 - 默认行为
                    .Sequence("Idle Sequence")
                        .Log("Idle Log", "进入空闲状态")
                        .Wait("Idle Wait", 1f)
                        .Back()
                
                .Build();

            _behaviourTree = BehaviourTree.CreateTree(gameObject, tree);
            
            // 设置黑板数据
            if (target != null)
            {
                _behaviourTree.Blackboard.SetValue("target", target);
            }
            _behaviourTree.Blackboard.SetValue("patrolDistance", patrolDistance);
            _behaviourTree.Blackboard.SetValue("health", 100f);
        }

        /// <summary>
        /// 创建更复杂的现代行为树示例
        /// </summary>
        private void CreateAdvancedModernBehaviourTree()
        {
            var tree = BehaviourTreeBuilder.Create()
                .Root<SelectorNode>()
                    
                    // 逃跑行为 - 多重条件检查
                    .BlackboardCondition("Low Health", "health", 30f, BlackboardConditionNode.CompareType.Less)
                        .BlackboardCondition("Has Enemy", "enemy", null, BlackboardConditionNode.CompareType.NotEquals)
                            .DistanceCondition("Enemy Close", "enemy", 10f, DistanceConditionNode.CompareType.Less)
                                .Sequence("Flee Sequence")
                                    .Log("Flee Log", "血量过低，开始逃跑！", LogType.Warning)
                                    .Node(new FleeAction())
                                    .Back()
                                .Back()
                            .Back()
                        .Back()
                    
                    // 治疗行为 - 带冷却的条件检查
                    .BlackboardCondition("Need Heal", "health", 50f, BlackboardConditionNode.CompareType.Less)
                        .CooldownCondition("Heal Cooldown", 10f) // 10秒冷却
                            .BlackboardCondition("Has Potion", "potions", 0, BlackboardConditionNode.CompareType.Greater)
                                .Sequence("Heal Sequence")
                                    .Log("Heal Log", "使用治疗药水")
                                    .Node(new UseHealPotionAction())
                                    .Back()
                                .Back()
                            .Back()
                        .Back()
                    
                    // 战斗行为 - 复杂的战斗逻辑
                    .BlackboardCondition("Combat Ready", "health", 20f, BlackboardConditionNode.CompareType.Greater)
                        .BlackboardCondition("Has Target", "target", null, BlackboardConditionNode.CompareType.NotEquals)
                            .DistanceCondition("In Combat Range", "target", 8f, DistanceConditionNode.CompareType.Less)
                                .Selector("Combat Selector")
                                    
                                    // 特殊攻击 - 有概率使用
                                    .RandomCondition("Special Attack Chance", 0.3f)
                                        .CooldownCondition("Special Cooldown", 5f)
                                            .Node(new SpecialAttackAction())
                                            .Back()
                                        .Back()
                                    
                                    // 普通攻击
                                    .Node(new AttackAction())
                                    
                                    .Back()
                                .Back()
                            .Back()
                        .Back()
                    
                    // 探索行为 - 带时间条件
                    .TimeCondition("Explore Time", 30f) // 30秒后才开始探索
                        .Sequence("Explore Sequence")
                            .Log("Explore Log", "开始探索新区域")
                            .Node(new ExploreAction())
                            .Back()
                        .Back()
                    
                    // 默认空闲
                    .Node(new IdleAction())
                    
                .Build();

            _behaviourTree = BehaviourTree.CreateTree(gameObject, tree);
            
            // 初始化黑板数据
            _behaviourTree.Blackboard.SetValue("health", 100f);
            _behaviourTree.Blackboard.SetValue("maxHealth", 100f);
            _behaviourTree.Blackboard.SetValue("potions", 3);
            _behaviourTree.Blackboard.SetValue("startTime", Time.time);
        }

        private void Update()
        {
            // 动态更新黑板数据
            if (_behaviourTree?.Blackboard != null)
            {
                if (target != null)
                {
                    _behaviourTree.Blackboard.SetValue("target", target);
                }
                
                // 模拟血量变化
                float currentHealth = _behaviourTree.Blackboard.GetValue("health", 100f);
                if (Input.GetKeyDown(KeyCode.H))
                {
                    _behaviourTree.Blackboard.SetValue("health", Mathf.Max(0, currentHealth - 10));
                    Debug.Log($"Health: {_behaviourTree.Blackboard.GetValue<float>("health")}");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 绘制范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, patrolDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
        }
    }

    // 示例动作节点
    public class AttackAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            Debug.Log($"[{gameObject.name}] 执行攻击！");
            return NodeState.Success;
        }
    }

    public class SpecialAttackAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            Debug.Log($"[{gameObject.name}] 执行特殊攻击！");
            return NodeState.Success;
        }
    }

    public class ChaseAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            var target = blackboard?.GetValue<Transform>("target");
            if (target == null) return NodeState.Failure;

            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * (5f * Time.deltaTime);
            return NodeState.Running;
        }
    }

    public class PatrolAction : ActionNode
    {
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private bool _hasTarget;

        protected override void OnStart()
        {
            base.OnStart();
            _startPosition = transform.position;
            SetRandomPatrolTarget();
        }

        protected override NodeState OnUpdate()
        {
            if (!_hasTarget) SetRandomPatrolTarget();

            Vector3 direction = (_targetPosition - transform.position).normalized;
            transform.position += direction * (2f * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.5f)
            {
                _hasTarget = false;
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        private void SetRandomPatrolTarget()
        {
            float patrolDistance = blackboard?.GetValue("patrolDistance", 5f) ?? 5f;
            Vector2 randomCircle = Random.insideUnitCircle * patrolDistance;
            _targetPosition = _startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            _hasTarget = true;
        }
    }

    public class FleeAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            var enemy = blackboard?.GetValue<Transform>("enemy");
            if (enemy != null)
            {
                Vector3 fleeDirection = (transform.position - enemy.position).normalized;
                transform.position += fleeDirection * (8f * Time.deltaTime);
            }
            return NodeState.Running;
        }
    }

    public class UseHealPotionAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            int potions = blackboard?.GetValue("potions", 0) ?? 0;
            if (potions > 0)
            {
                blackboard?.SetValue("potions", potions - 1);
                float health = blackboard?.GetValue("health", 0f) ?? 0f;
                float maxHealth = blackboard?.GetValue("maxHealth", 100f) ?? 100f;
                blackboard?.SetValue("health", Mathf.Min(maxHealth, health + 30f));
                
                Debug.Log($"[{gameObject.name}] 使用治疗药水！剩余药水: {potions - 1}");
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }

    public class ExploreAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f), 
                0, 
                Random.Range(-1f, 1f)
            ).normalized;
            
            transform.position += randomDirection * (3f * Time.deltaTime);
            Debug.Log($"[{gameObject.name}] 探索中...");
            return NodeState.Running;
        }
    }

    public class IdleAction : ActionNode
    {
        protected override NodeState OnUpdate()
        {
            Debug.Log($"[{gameObject.name}] 空闲状态");
            return NodeState.Success;
        }
    }
}
