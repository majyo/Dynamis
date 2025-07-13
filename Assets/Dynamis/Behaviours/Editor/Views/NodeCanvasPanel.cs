using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class NodeCanvasPanel : VisualElement
    {
        public NodeCanvasPanel()
        {
            SetupPanel();
            AddSampleNodes();
        }

        private void SetupPanel()
        {
            // 设置面板名称
            name = "node-canvas-panel";
            
            // 设置面板样式 - 深灰色背景
            style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 深灰色
            style.flexGrow = 1; // 占满父容器
            style.minWidth = 200; // 最小宽度
            style.minHeight = 200; // 最小高度
        }
        
        private void AddSampleNodes()
        {
            // 创建几个示例节点来展示效果
            
            // Root节点
            var rootNode = new BehaviourNode("Root", "Behaviour tree root node");
            rootNode.SetPosition(new Vector2(200, 50));
            Add(rootNode);
            
            // Selector节点
            var selectorNode = new BehaviourNode("Selector", "Select first successful child");
            selectorNode.SetPosition(new Vector2(150, 180));
            Add(selectorNode);
            
            // Sequence节点
            var sequenceNode = new BehaviourNode("Sequence", "Execute children in order");
            sequenceNode.SetPosition(new Vector2(250, 180));
            Add(sequenceNode);
            
            // Action节点
            var actionNode1 = new BehaviourNode("Move To Target", "Move character to target position");
            actionNode1.SetPosition(new Vector2(100, 310));
            Add(actionNode1);
            
            var actionNode2 = new BehaviourNode("Attack Enemy", "Perform attack on enemy target");
            actionNode2.SetPosition(new Vector2(200, 310));
            Add(actionNode2);
            
            var actionNode3 = new BehaviourNode("Wait", "Wait for specified duration");
            actionNode3.SetPosition(new Vector2(300, 310));
            Add(actionNode3);
        }
    }
}
