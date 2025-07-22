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
        
        private Dictionary<string, NodeElement> _sampleNodes;
        
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
            
            _toolbar = new BehaviourEditorToolbar();
            root.Add(_toolbar);
            
            _twoColumnLayout = new TwoColumnLayout
            {
                style =
                {
                    flexGrow = 1
                }
            };
            root.Add(_twoColumnLayout);
            
            SetupNodeCanvas();
            
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
            var rootNode = new NodeElement("Root", "Behaviour tree root node", false, true);
            _nodeCanvasPanel.AddNode(rootNode, new Vector2(200, 50));
            
            var selectorNode = new NodeElement("Selector", "Select first successful child");
            _nodeCanvasPanel.AddNode(selectorNode, new Vector2(150, 180));

            var sequenceNode = new NodeElement("Sequence", "Execute children in order");
            _nodeCanvasPanel.AddNode(sequenceNode, new Vector2(250, 180));

            var actionNode1 = new NodeElement("Move To Target", "Move character to target position", true, false);
            _nodeCanvasPanel.AddNode(actionNode1, new Vector2(100, 310));

            var actionNode2 = new NodeElement("Attack Enemy", "Perform attack on enemy target", true, false);
            _nodeCanvasPanel.AddNode(actionNode2, new Vector2(200, 310));

            var actionNode3 = new NodeElement("Wait", "Wait for specified duration", true, false);
            _nodeCanvasPanel.AddNode(actionNode3, new Vector2(300, 310));

            _sampleNodes = new Dictionary<string, NodeElement>
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
            rootVisualElement.schedule.Execute(() =>
            {
                var connection1 =
                    new Connection(_sampleNodes["root"].OutputPort, _sampleNodes["selector"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection1);

                var connection2 =
                    new Connection(_sampleNodes["root"].OutputPort, _sampleNodes["sequence"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection2);

                var connection3 = new Connection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["moveToTarget"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                _nodeCanvasPanel.AddConnection(connection3);

                var connection4 = new Connection(_sampleNodes["selector"].OutputPort,
                    _sampleNodes["attackEnemy"].InputPort)
                {
                    ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
                    LineWidth = 2f
                };
                _nodeCanvasPanel.AddConnection(connection4);

                var connection5 =
                    new Connection(_sampleNodes["sequence"].OutputPort, _sampleNodes["wait"].InputPort)
                    {
                        ConnectionColor = new Color(0.8f, 0.3f, 0.3f, 1f),
                        LineWidth = 2f
                    };
                _nodeCanvasPanel.AddConnection(connection5);
            }).ExecuteLater(100); // 延迟100ms执行
        }
    }
}