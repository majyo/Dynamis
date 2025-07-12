using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Scripts.Editor
{
    public class GraphWindow : EditorWindow
    {
        private NodeGraphView graphView;
        private GraphExecutor graphExecutor;
        private string currentFilePath;

        [MenuItem("Tools/Dynamis/Node Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<GraphWindow>();
            window.titleContent = new GUIContent("Node Editor");
            window.Show();
        }

        public void CreateGUI()
        {
            ConstructGraphView();
            GenerateToolbar();
            AddStyles();
            
            // 创建图形执行器
            graphExecutor = new GraphExecutor(graphView);
        }

        private void ConstructGraphView()
        {
            graphView = new NodeGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            // 新建按钮
            var newButton = new ToolbarButton(() => NewGraph())
            {
                text = "New"
            };
            toolbar.Add(newButton);

            // 保存按钮
            var saveButton = new ToolbarButton(() => SaveGraph())
            {
                text = "Save"
            };
            toolbar.Add(saveButton);

            // 另存为按钮
            var saveAsButton = new ToolbarButton(() => SaveAsGraph())
            {
                text = "Save As"
            };
            toolbar.Add(saveAsButton);

            // 加载按钮
            var loadButton = new ToolbarButton(() => LoadGraph())
            {
                text = "Load"
            };
            toolbar.Add(loadButton);

            // 分隔符
            toolbar.Add(new ToolbarSpacer());

            // 清空按钮
            var clearButton = new ToolbarButton(() => ClearGraph())
            {
                text = "Clear"
            };
            toolbar.Add(clearButton);

            // 执行按钮（示例功能）
            var executeButton = new ToolbarButton(() => ExecuteGraph())
            {
                text = "Execute"
            };
            toolbar.Add(executeButton);

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            var styleSheet = Resources.Load<StyleSheet>("NodeEditorStyles");
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }
        }

        private void NewGraph()
        {
            if (EditorUtility.DisplayDialog("New Graph", "Are you sure you want to create a new graph? Unsaved changes will be lost.", "Yes", "Cancel"))
            {
                graphView.ClearGraph();
                currentFilePath = null;
            }
        }

        private void SaveGraph()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveAsGraph();
            }
            else
            {
                SaveGraphToFile(currentFilePath);
            }
        }

        private void SaveAsGraph()
        {
            var path = EditorUtility.SaveFilePanel("Save Graph", "Assets", "NewGraph", "json");
            if (!string.IsNullOrEmpty(path))
            {
                currentFilePath = path;
                SaveGraphToFile(path);
            }
        }

        private void SaveGraphToFile(string path)
        {
            try
            {
                var graphData = graphView.GetGraphData();
                var json = JsonUtility.ToJson(graphData, true);
                File.WriteAllText(path, json);
                
                Debug.Log($"Graph saved to: {path}");
                EditorUtility.DisplayDialog("Save Complete", "Graph saved successfully!", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Save Error", $"Failed to save graph: {e.Message}", "OK");
            }
        }

        private void LoadGraph()
        {
            var path = EditorUtility.OpenFilePanel("Load Graph", "Assets", "json");
            if (!string.IsNullOrEmpty(path))
            {
                LoadGraphFromFile(path);
            }
        }

        private void LoadGraphFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var graphData = JsonUtility.FromJson<GraphData>(json);
                
                graphView.LoadGraphData(graphData);
                currentFilePath = path;
                
                Debug.Log($"Graph loaded from: {path}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Load Error", $"Failed to load graph: {e.Message}", "OK");
            }
        }

        private void ClearGraph()
        {
            if (EditorUtility.DisplayDialog("Clear Graph", "Are you sure you want to clear the graph?", "Yes", "Cancel"))
            {
                graphView.ClearGraph();
            }
        }

        private void ExecuteGraph()
        {
            if (graphExecutor != null)
            {
                graphExecutor.ExecuteGraph();
                Debug.Log("Graph execution completed. Check the console for results.");
            }
            else
            {
                Debug.LogWarning("Graph executor not initialized.");
            }
        }
    }
}