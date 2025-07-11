# 行为树系统 (Behaviour Tree System)

这是一个基于代码配置的Unity行为树系统，提供了灵活且强大的AI行为控制功能。

## 核心组件

### 1. 基础节点类型

- **BehaviourNode**: 所有节点的基类
- **CompositeNode**: 复合节点基类（可以有多个子节点）
- **DecoratorNode**: 装饰节点基类（只能有一个子节点）
- **LeafNode**: 叶子节点基类（不能有子节点）

### 2. 复合节点 (CompositeNodes.cs)

- **SequenceNode**: 顺序执行子节点，全部成功才成功
- **SelectorNode**: 选择执行子节点，任一成功即成功
- **ParallelNode**: 并行执行所有子节点
- **RandomSelectorNode**: 随机选择一个子节点执行
- **WeightedRandomSelectorNode**: 根据权重随机选择子节点

### 3. 装饰节点 (DecoratorNodes.cs)

- **InverterNode**: 反转子节点的成功/失败状态
- **RepeaterNode**: 重复执行子节点指定次数
- **RepeatUntilFailNode**: 重复执行直到失败
- **RepeatUntilSuccessNode**: 重复执行直到成功
- **TimeoutNode**: 超时控制
- **CooldownNode**: 冷却时间控制
- **ConditionalNode**: 条件执行

### 4. 叶子节点 (LeafNodes.cs)

**基础动作节点:**
- **WaitNode**: 等待指定时间
- **RandomWaitNode**: 等待随机时间
- **LogNode**: 输出日志信息
- **SuccessNode**: 总是返回成功
- **FailureNode**: 总是返回失败
- **RandomResultNode**: 随机返回成功或失败

**条件节点 (ConditionNodes.cs):**
- **BlackboardConditionNode**: 检查黑板中的值
- **DistanceConditionNode**: 检查与目标的距离
- **TimeConditionNode**: 检查经过的时间
- **RandomConditionNode**: 随机条件

### 5. 支持系统

- **BehaviourTree**: 行为树运行器组件
- **Blackboard**: 黑板系统，用于数据共享
- **BehaviourTreeBuilder**: 流畅API构建器

## 使用方法

### 1. 基本使用

```csharp
using Dynamis.Behaviours;

public class AIController : MonoBehaviour
{
    private void Start()
    {
        // 使用Builder创建行为树
        var tree = BehaviourTreeBuilder.Create()
            .Root<SelectorNode>()
                .Sequence("Combat")
                    .BlackboardCondition("HasTarget", "target", null, BlackboardConditionNode.CompareType.NotEquals)
                    .Node(new AttackAction())
                    .Back()
                .Sequence("Patrol")
                    .Node(new PatrolAction())
                    .Wait("PatrolWait", 2f)
                    .Back()
            .BuildAndAttach(gameObject);
            
        // 设置黑板数据
        tree.blackboard.SetValue("target", someTarget);
    }
}
```

### 2. 自定义动作节点

```csharp
public class CustomAction : ActionNode
{
    protected override NodeState OnUpdate()
    {
        // 访问黑板数据
        var target = blackboard.GetValue<Transform>("target");
        
        // 访问GameObject组件
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        
        // 执行自定义逻辑
        // ...
        
        return NodeState.Success; // 或 Failure, Running
    }
    
    protected override void OnStart()
    {
        // 节点开始时的初始化
    }
    
    protected override void OnStop()
    {
        // 节点结束时的清理
    }
}
```

### 3. 自定义条件节点

```csharp
public class CustomCondition : ConditionNode
{
    protected override NodeState OnUpdate()
    {
        // 检查条件
        bool conditionMet = SomeConditionCheck();
        
        return conditionMet ? NodeState.Success : NodeState.Failure;
    }
}
```

### 4. 黑板使用

```csharp
// 设置值
blackboard.SetValue("health", 100f);
blackboard.SetValue("target", targetTransform);
blackboard.SetValue("isAlerted", true);

// 获取值
float health = blackboard.GetValue<float>("health");
Transform target = blackboard.GetValue<Transform>("target");
bool isAlerted = blackboard.GetValue<bool>("isAlerted", false); // 带默认值

// 检查键是否存在
if (blackboard.HasKey("target"))
{
    // 处理逻辑
}
```

## 示例

查看 `BehaviourTreeExample.cs` 文件获取完整的使用示例，包括：

1. 简单的巡逻和追击AI
2. 复杂的战斗AI系统
3. 自定义动作节点的实现

## 高级功能

### 1. 运行时控制

```csharp
// 手动执行一次
NodeState result = behaviourTree.Tick();

// 中止执行
behaviourTree.RootNode.Abort();
```

### 2. 节点克隆

行为树会自动克隆节点以避免多个实例之间的数据冲突。

### 3. 调试支持

- 使用LogNode输出调试信息
- 节点名称帮助识别执行路径
- 通过Gizmos可视化相关数据

## 扩展指南

1. **创建新的复合节点**: 继承`CompositeNode`
2. **创建新的装饰节点**: 继承`DecoratorNode`
3. **创建新的动作节点**: 继承`ActionNode`
4. **创建新的条件节点**: 继承`ConditionNode`

## 性能考虑

1. 行为树每帧更新，设置合适的`updateInterval`
2. 避免在节点中进行昂贵的计算
3. 使用黑板共享数据而不是重复计算
4. 考虑使用冷却节点避免频繁执行

## 注意事项

1. 确保自定义节点正确实现`OnUpdate()`方法
2. 使用Builder时注意调用`Back()`返回上级节点
3. 装饰节点只能有一个子节点
4. 叶子节点不能有子节点
