using UnityEngine;

namespace Dynamis.Blackboards
{
    /// <summary>
    /// Blackboard使用示例脚本
    /// 演示如何在代码中使用Blackboard进行数据存储和读取
    /// </summary>
    public class BlackboardExample : MonoBehaviour
    {
        [Header("Blackboard引用")]
        [SerializeField] private Blackboard blackboard;
        
        [Header("测试数据")]
        [SerializeField] private string playerName = "玩家1";
        [SerializeField] private int playerLevel = 5;
        [SerializeField] private float playerHealth = 100f;
        [SerializeField] private bool isPlayerAlive = true;
        [SerializeField] private Vector3 playerPosition = Vector3.zero;
        [SerializeField] private Color playerColor = Color.white;
        
        private void Start()
        {
            if (blackboard == null)
            {
                Debug.LogError("Blackboard引用为空！请在Inspector中分配一个Blackboard资产。");
                return;
            }
            
            // 演示如何设置值
            SetExampleValues();
            
            // 演示如何读取值
            ReadExampleValues();
        }
        
        private void SetExampleValues()
        {
            Debug.Log("=== 设置Blackboard值 ===");
            
            // 设置各种类型的值
            blackboard.SetValue("PlayerName", playerName);
            blackboard.SetValue("PlayerLevel", playerLevel);
            blackboard.SetValue("PlayerHealth", playerHealth);
            blackboard.SetValue("IsPlayerAlive", isPlayerAlive);
            blackboard.SetValue("PlayerPosition", playerPosition);
            blackboard.SetValue("PlayerColor", playerColor);
            
            Debug.Log($"已设置玩家数据到Blackboard");
        }
        
        private void ReadExampleValues()
        {
            Debug.Log("=== 从Blackboard读取值 ===");
            
            // 读取各种类型的值
            string name = blackboard.GetValue("PlayerName", "未知玩家");
            int level = blackboard.GetValue("PlayerLevel", 1);
            float health = blackboard.GetValue("PlayerHealth", 0f);
            bool alive = blackboard.GetValue("IsPlayerAlive", false);
            Vector3 position = blackboard.GetValue("PlayerPosition", Vector3.zero);
            Color color = blackboard.GetValue("PlayerColor", Color.gray);
            
            Debug.Log($"玩家姓名: {name}");
            Debug.Log($"玩家等级: {level}");
            Debug.Log($"玩家血量: {health}");
            Debug.Log($"玩家存活: {alive}");
            Debug.Log($"玩家位置: {position}");
            Debug.Log($"玩家颜色: {color}");
            
            // 检查键是否存在
            if (blackboard.HasKey("PlayerName"))
            {
                Debug.Log("PlayerName键存在于Blackboard中");
            }
            
            // 获取所有键
            string[] allKeys = blackboard.GetAllKeys();
            Debug.Log($"Blackboard中共有 {allKeys.Length} 个键: {string.Join(", ", allKeys)}");
        }
        
        [ContextMenu("更新Blackboard数据")]
        private void UpdateBlackboardData()
        {
            if (blackboard != null)
            {
                SetExampleValues();
                Debug.Log("Blackboard数据已更新");
            }
        }
        
        [ContextMenu("清空Blackboard")]
        private void ClearBlackboard()
        {
            if (blackboard != null)
            {
                blackboard.Clear();
                Debug.Log("Blackboard已清空");
            }
        }
        
        [ContextMenu("删除玩家数据")]
        private void RemovePlayerData()
        {
            if (blackboard != null)
            {
                blackboard.RemoveKey("PlayerName");
                blackboard.RemoveKey("PlayerLevel");
                blackboard.RemoveKey("PlayerHealth");
                Debug.Log("玩家数据已从Blackboard中删除");
            }
        }
    }
}
