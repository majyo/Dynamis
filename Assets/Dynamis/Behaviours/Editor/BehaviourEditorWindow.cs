using UnityEditor;
using UnityEngine;
using Dynamis.Behaviours.Editor.Views;
using Dynamis.Behaviours.Runtimes;
using UnityEngine.Assertions;

namespace Dynamis.Behaviours.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {
        public static BehaviourEditorWindow Instance { get; private set; }
        
        private BehaviourEditorToolbar _toolbar;
        private TwoColumnLayout _twoColumnLayout;
        private NodeCanvasPanel _nodeCanvasPanel;
        
        // private Dictionary<string, NodeElement> _sampleNodes;
        
        // 当前编辑的行为树资产
        private BehaviourTreeAsset _currentAsset;
        private string _currentAssetPath;
        private bool _hasUnsavedChanges;
        
        [MenuItem("Dynamis/Behaviour Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<BehaviourEditorWindow>("Behaviour Editor");
            Instance.titleContent = new GUIContent("Behaviour Editor");
            
            if (Instance._currentAsset == null)
            {
                Instance.ClearAndCreateNewAsset();
            }
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1;
            
            _toolbar = new BehaviourEditorToolbar();
            
            // 连接工具栏事件
            _toolbar.OnNewClicked += OnNewAsset;
            _toolbar.OnOpenClicked += OnOpenAsset;
            _toolbar.OnSaveClicked += OnSaveAsset;
            
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
            
            // AddSampleNodes();
            // CreateSampleConnections();
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
        
        // private void AddSampleNodes()
        // {
        //     var rootNode = new NodeElement("Root", "Behaviour tree root node", false, true);
        //     _nodeCanvasPanel.AddNode(rootNode, new Vector2(200, 50));
        //     
        //     var selectorNode = new NodeElement("Selector", "Select first successful child");
        //     _nodeCanvasPanel.AddNode(selectorNode, new Vector2(150, 180));
        //
        //     var sequenceNode = new NodeElement("Sequence", "Execute children in order");
        //     _nodeCanvasPanel.AddNode(sequenceNode, new Vector2(250, 180));
        //
        //     var actionNode1 = new NodeElement("Move To Target", "Move character to target position", true, false);
        //     _nodeCanvasPanel.AddNode(actionNode1, new Vector2(100, 310));
        //
        //     var actionNode2 = new NodeElement("Attack Enemy", "Perform attack on enemy target", true, false);
        //     _nodeCanvasPanel.AddNode(actionNode2, new Vector2(200, 310));
        //
        //     var actionNode3 = new NodeElement("Wait", "Wait for specified duration", true, false);
        //     _nodeCanvasPanel.AddNode(actionNode3, new Vector2(300, 310));
        //
        //     _sampleNodes = new Dictionary<string, NodeElement>
        //     {
        //         ["root"] = rootNode,
        //         ["selector"] = selectorNode,
        //         ["sequence"] = sequenceNode,
        //         ["moveToTarget"] = actionNode1,
        //         ["attackEnemy"] = actionNode2,
        //         ["wait"] = actionNode3
        //     };
        // }
        //
        // private void CreateSampleConnections()
        // {
        //     rootVisualElement.schedule.Execute(() =>
        //     {
        //         var connection1 =
        //             new Connection(_sampleNodes["root"].OutputPort, _sampleNodes["selector"].InputPort)
        //             {
        //                 ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
        //                 LineWidth = 2f
        //             };
        //         _nodeCanvasPanel.AddConnection(connection1);
        //
        //         var connection2 =
        //             new Connection(_sampleNodes["root"].OutputPort, _sampleNodes["sequence"].InputPort)
        //             {
        //                 ConnectionColor = new Color(0.8f, 0.8f, 0.8f, 1f),
        //                 LineWidth = 2f
        //             };
        //         _nodeCanvasPanel.AddConnection(connection2);
        //
        //         var connection3 = new Connection(_sampleNodes["selector"].OutputPort,
        //             _sampleNodes["moveToTarget"].InputPort)
        //         {
        //             ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
        //             LineWidth = 2f
        //         };
        //         _nodeCanvasPanel.AddConnection(connection3);
        //
        //         var connection4 = new Connection(_sampleNodes["selector"].OutputPort,
        //             _sampleNodes["attackEnemy"].InputPort)
        //         {
        //             ConnectionColor = new Color(0.3f, 0.8f, 0.3f, 1f),
        //             LineWidth = 2f
        //         };
        //         _nodeCanvasPanel.AddConnection(connection4);
        //
        //         var connection5 =
        //             new Connection(_sampleNodes["sequence"].OutputPort, _sampleNodes["wait"].InputPort)
        //             {
        //                 ConnectionColor = new Color(0.8f, 0.3f, 0.3f, 1f),
        //                 LineWidth = 2f
        //             };
        //         _nodeCanvasPanel.AddConnection(connection5);
        //     }).ExecuteLater(100); // 延迟100ms执行
        // }
        
        #region Asset Management
        
        /// <summary>
        /// 创建新的行为树资产
        /// </summary>
        private void OnNewAsset()
        {
            if (_hasUnsavedChanges)
            {
                if (!EditorUtility.DisplayDialog("Unsaved Changes", 
                    "You have unsaved changes. Do you want to continue without saving?", 
                    "Continue", "Cancel"))
                {
                    return;
                }
            }
            
            ClearAndCreateNewAsset();
        }
        
        /// <summary>
        /// 打开现有的行为树资产
        /// </summary>
        private void OnOpenAsset()
        {
            if (_hasUnsavedChanges)
            {
                if (!EditorUtility.DisplayDialog("Unsaved Changes", 
                    "You have unsaved changes. Do you want to continue without saving?", 
                    "Continue", "Cancel"))
                {
                    return;
                }
            }
            
            var path = EditorUtility.OpenFilePanel("Open Behaviour Tree", "Assets", "asset");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
                
            // 转换为相对路径
            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            
            // 加载资产
            var asset = AssetDatabase.LoadAssetAtPath<BehaviourTreeAsset>(relativePath);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to load behaviour tree asset.", "OK");
                return;
            }
            
            LoadAsset(asset, relativePath);
        }
        
        /// <summary>
        /// 保存当前行为树资产
        /// </summary>
        private void OnSaveAsset()
        {
            Assert.IsNotNull(_currentAsset);
            
            if (string.IsNullOrEmpty(_currentAssetPath))
            {
                // 另存为
                SaveAssetAs();
            }
            else
            {
                // 保存到现有路径
                SaveAsset();
            }
        }
        
        private void ClearAndCreateNewAsset()
        {
            ClearCanvas();
            
            _currentAsset = CreateInstance<BehaviourTreeAsset>();
            _currentAssetPath = null;
            _hasUnsavedChanges = false;
            
            LoadBehaviourTreeFromAsset();
            UpdateWindowTitle();
        }
        
        /// <summary>
        /// 另存为新文件
        /// </summary>
        private void SaveAssetAs()
        {
            var path = EditorUtility.SaveFilePanel("Save Behaviour Tree", "Assets", "NewBehaviourTree", "asset");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
                
            // 转换为相对路径
            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            
            // 保存资产
            SaveBehaviourTreeToAsset();
            AssetDatabase.CreateAsset(_currentAsset, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            _currentAssetPath = relativePath;
            _hasUnsavedChanges = false;
            
            UpdateWindowTitle();
        }
        
        /// <summary>
        /// 保存到现有文件
        /// </summary>
        private void SaveAsset()
        {
            if (_currentAsset == null || string.IsNullOrEmpty(_currentAssetPath))
            {
                return;
            }
                
            SaveBehaviourTreeToAsset();
            EditorUtility.SetDirty(_currentAsset);
            AssetDatabase.SaveAssets();
            
            _hasUnsavedChanges = false;
            UpdateWindowTitle();
        }
        
        /// <summary>
        /// 加载指定的行为树资产
        /// </summary>
        /// <param name="asset">要加载的资产</param>
        /// <param name="assetPath">资产路径</param>
        public void LoadAsset(BehaviourTreeAsset asset, string assetPath)
        {
            _currentAsset = asset;
            _currentAssetPath = assetPath;
            _hasUnsavedChanges = false;
            
            ClearCanvas();
            LoadBehaviourTreeFromAsset();
            UpdateWindowTitle();
        }
        
        /// <summary>
        /// 清除画布内容
        /// </summary>
        private void ClearCanvas()
        {
            _nodeCanvasPanel?.ClearAll();
        }
        
        /// <summary>
        /// 从资产加载行为树内容（待实现）
        /// </summary>
        private void LoadBehaviourTreeFromAsset()
        {
            // TODO: 当BehaviourTreeAsset有具体内容时，在这里实现加载逻辑
        }
        
        /// <summary>
        /// 将当前行为树内容保存到资产（待实现）
        /// </summary>
        private void SaveBehaviourTreeToAsset()
        {
            if (_currentAsset == null)
            {
                return;
            }
                
            // TODO: 当BehaviourTreeAsset有具体内容时，在这里实现保存逻辑
            // 将_nodeCanvasPanel中的节点和连接保存到_currentAsset
        }
        
        /// <summary>
        /// 更新窗口标题
        /// </summary>
        private void UpdateWindowTitle()
        {
            var windowTitle = "Behaviour Editor";
            
            if (_currentAsset != null)
            {
                var assetName = string.IsNullOrEmpty(_currentAssetPath) ? "New Behaviour Tree" : 
                    System.IO.Path.GetFileNameWithoutExtension(_currentAssetPath);
                windowTitle += " - " + assetName;
                
                if (_hasUnsavedChanges)
                    windowTitle += "*";
            }
            
            titleContent = new GUIContent(windowTitle);
        }
        
        /// <summary>
        /// 标记为有未保存的更改
        /// </summary>
        public void MarkAsModified()
        {
            _hasUnsavedChanges = true;
            UpdateWindowTitle();
        }
        
        #endregion
    }
}