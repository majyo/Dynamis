using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Dynamis.Behaviours.Editor.Views;

namespace Dynamis.Behaviours.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {
        public static BehaviourEditorWindow Instance { get; private set; }
        
        private BehaviourEditorToolbar _toolbar;
        private TwoColumnLayout _twoColumnLayout;
        private NodeCanvasPanel _nodeCanvasPanel;
        
        private Dictionary<string, BehaviourNode> _sampleNodes;
        
        [MenuItem("Dynamis/Behaviour Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<BehaviourEditorWindow>("Behaviour Editor");
            Instance.titleContent = new GUIContent("Behaviour Editor");
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1;
            
            // 首先添加工具栏作为第一个元素
            _toolbar = new BehaviourEditorToolbar();
            root.Add(_toolbar);
            
            // 然后添加两列布局
            _twoColumnLayout = new TwoColumnLayout
            {
                style =
                {
                    flexGrow = 1
                }
            };
            root.Add(_twoColumnLayout);
            
            // 添加节点画布面板到右侧面板
            SetupNodeCanvas();
            
            // 添加示例
            AddSampleNodes();
            CreateSampleConnections();
        }
        
        private void SetupNodeCanvas()
        {
            var rightContent = _twoColumnLayout.RightContent;
            
            if (rightContent == null)
            {
                return;
            }
            
            _nodeCanvasPanel = new NodeCanvasPanel();
            rightContent.Add(_nodeCanvasPanel);
        }
        
        private void AddSampleNodes()
        {
            // 创建几个示例节点来展示效果

            // Root节点 - 只有输出端口
            var rootNode = new BehaviourNode("Root", "Behaviour tree root node", false, true);
            _nodeCanvasPanel.AddNode(rootNode, new Vector2(200, 50));

            // Selector节点 - 有输入和输出端口
            var selectorNode = new BehaviourNode("Selector", "Select first successful child");
            _nodeCanvasPanel.AddNode(selectorNode, new Vector2(150, 180));

            // Sequence节点 - 有输入和输出端口
            var sequenceNode = new BehaviourNode("Sequence", "Execute children in order");
            _nodeCanvasPanel.AddNode(sequenceNode, new Vector2(250, 180));

            // Action节点 - 只有输入端口
            var actionNode1 = new BehaviourNode("Move To Target", "Move character to target position", true, false);
            _nodeCanvasPanel.AddNode(actionNode1, new Vector2(100, 310));

            var actionNode2 = new BehaviourNode("Attack Enemy", "Perform attack on enemy target", true, false);
            _nodeCanvasPanel.AddNode(actionNode2, new Vector2(200, 310));

            var actionNode3 = new BehaviourNode("Wait", "Wait for specified duration", true, false);
            _nodeCanvasPanel.AddNode(actionNode3, new Vector2(300, 310));

            // 存储节点引用以便创建连线
            _sampleNodes = new Dictionary<string, BehaviourNode>
            {
                ["root"] = rootNode,
                ["selector"] = selectorNode,
                ["sequence"] = sequenceNode,
                ["moveToTarget"] = actionNode1,
                ["attackEnemy"] = actionNode2,
                ["wait"] = actionNode3
            };
        }
        
        private void CreateSampleConnections()
        {
            // 等待一帧确保节点位置已确定
            rootVisualElement.schedule.Execute(() =>
            {
                // Root 连接到 Selector
                var connection1 =
                    new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["selector"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection1);

                // Root 连接到 Sequence
                var connection2 =
                    new NodeConnection(_sampleNodes["root"].OutputPort, _sampleNodes["sequence"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection2);

                // Selector 连接到 Move To Target
                var connection3 = new NodeConnection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["moveToTarget"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                _nodeCanvasPanel.AddConnection(connection3);

                // Selector 连接到 Attack Enemy
                var connection4 = new NodeConnection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["attackEnemy"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                _nodeCanvasPanel.AddConnection(connection4);

                // Sequence 连接到 Wait
                var connection5 =
                    new NodeConnection(_sampleNodes["sequence"].OutputPort, _sampleNodes["wait"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.3f, 0.3f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection5);
            }).ExecuteLater(100); // 延迟100ms执行
        }
    }
}