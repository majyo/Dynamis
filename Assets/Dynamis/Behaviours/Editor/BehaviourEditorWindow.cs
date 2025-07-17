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
            _toolbar = new Views.BehaviourEditorToolbar();
            root.Add(_toolbar);
            
            // 然后添加两列布局
            _twoColumnLayout = new Views.TwoColumnLayout
            {
                style =
                {
                    flexGrow = 1
                }
            };
            root.Add(_twoColumnLayout);
            
            // 添加节点画布面板到右侧面板
            SetupNodeCanvas();
        }
        
        private void SetupNodeCanvas()
        {
            var rightContent = _twoColumnLayout.RightContent;
            if (rightContent != null)
            {
                var nodeCanvas = new Views.NodeCanvasPanel();
                rightContent.Add(nodeCanvas);
            }
        }
    }
}