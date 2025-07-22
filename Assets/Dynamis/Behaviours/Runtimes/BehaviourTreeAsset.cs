using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Dynamis/Behaviour Tree", order = 1)]
    public class BehaviourTreeAsset : ScriptableObject
    {
        // TODO: 添加行为树数据结构
        // 例如：节点列表、连接信息、根节点引用等

        /// <summary>
        /// 行为树的名称
        /// </summary>
        public string treeName;

        /// <summary>
        /// 行为树的描述
        /// </summary>
        [TextArea(3, 5)]
        public string description;

        // 预留字段，供后续扩展使用
        // public List<NodeData> nodes;
        // public List<ConnectionData> connections;
        // public string rootNodeId;
    }
}