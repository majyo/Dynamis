using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    public class BehaviourEditorToolbar : VisualElement
    {
        public BehaviourEditorToolbar()
        {
            SetupToolbar();
        }

        private void SetupToolbar()
        {
            // 设置工具栏基本样式
            name = "behaviour-editor-toolbar";
            
            style.height = 32;
            style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f); // Unity工具栏颜色
            style.borderBottomWidth = 1;
            style.borderBottomColor = new Color(0.13f, 0.13f, 0.13f, 1f);
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 2;
            style.paddingBottom = 2;
            
            // 创建工具栏内容
            CreateFileSection();
            CreateSeparator();
            CreateEditSection();
            CreateSeparator();
            CreateViewSection();
            CreateSeparator();
            CreateRunSection();
            
            // 添加弹性空间
            var spacer = new VisualElement { style = { flexGrow = 1 } };
            Add(spacer);
            
            CreateHelpSection();
        }
        
        private void CreateFileSection()
        {
            // 新建按钮
            var newButton = CreateToolbarButton("New", "Create a new behaviour tree");
            newButton.clicked += OnNewClicked;
            Add(newButton);
            
            // 打开按钮
            var openButton = CreateToolbarButton("Open", "Open an existing behaviour tree");
            openButton.clicked += OnOpenClicked;
            Add(openButton);
            
            // 保存按钮
            var saveButton = CreateToolbarButton("Save", "Save current behaviour tree");
            saveButton.clicked += OnSaveClicked;
            Add(saveButton);
        }
        
        private void CreateEditSection()
        {
            // 撤销按钮
            var undoButton = CreateToolbarButton("Undo", "Undo last action");
            undoButton.clicked += OnUndoClicked;
            Add(undoButton);
            
            // 重做按钮
            var redoButton = CreateToolbarButton("Redo", "Redo last undone action");
            redoButton.clicked += OnRedoClicked;
            Add(redoButton);
            
            // 删除按钮
            var deleteButton = CreateToolbarButton("Delete", "Delete selected nodes");
            deleteButton.clicked += OnDeleteClicked;
            Add(deleteButton);
        }
        
        private void CreateViewSection()
        {
            // 缩放适应按钮
            var fitButton = CreateToolbarButton("Fit All", "Fit all nodes in view");
            fitButton.clicked += OnFitAllClicked;
            Add(fitButton);
            
            // 网格切换按钮
            var gridButton = CreateToolbarButton("Grid", "Toggle grid visibility");
            gridButton.clicked += OnToggleGridClicked;
            Add(gridButton);
        }
        
        private void CreateRunSection()
        {
            // 播放按钮
            var playButton = CreateToolbarButton("▶ Play", "Start behaviour tree execution");
            playButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.3f);
            playButton.clicked += OnPlayClicked;
            Add(playButton);
            
            // 暂停按钮
            var pauseButton = CreateToolbarButton("⏸ Pause", "Pause behaviour tree execution");
            pauseButton.clicked += OnPauseClicked;
            Add(pauseButton);
            
            // 停止按钮
            var stopButton = CreateToolbarButton("⏹ Stop", "Stop behaviour tree execution");
            stopButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.3f);
            stopButton.clicked += OnStopClicked;
            Add(stopButton);
        }
        
        private void CreateHelpSection()
        {
            // 帮助按钮
            var helpButton = CreateToolbarButton("?", "Show help documentation");
            helpButton.clicked += OnHelpClicked;
            Add(helpButton);
        }
        
        private Button CreateToolbarButton(string text, string tooltip)
        {
            var button = new Button
            {
                text = text,
                tooltip = tooltip,
                style =
                {
                    height = 24,
                    paddingLeft = 8,
                    paddingRight = 8,
                    marginLeft = 2,
                    marginRight = 2,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f),
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                    borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                    borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                    borderRightColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    color = Color.white,
                    fontSize = 11
                }
            };
            
            // 添加悬停效果
            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            });
            
            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            });
            
            return button;
        }
        
        private void CreateSeparator()
        {
            var separator = new VisualElement
            {
                style =
                {
                    width = 1,
                    height = 20,
                    backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                    marginLeft = 4,
                    marginRight = 4
                }
            };
            Add(separator);
        }
        
        // 按钮事件处理方法
        private void OnNewClicked() => Debug.Log("New behaviour tree");
        private void OnOpenClicked() => Debug.Log("Open behaviour tree");
        private void OnSaveClicked() => Debug.Log("Save behaviour tree");
        private void OnUndoClicked() => Debug.Log("Undo action");
        private void OnRedoClicked() => Debug.Log("Redo action");
        private void OnDeleteClicked() => Debug.Log("Delete selected nodes");
        private void OnFitAllClicked() => Debug.Log("Fit all nodes in view");
        private void OnToggleGridClicked() => Debug.Log("Toggle grid visibility");
        private void OnPlayClicked() => Debug.Log("Start execution");
        private void OnPauseClicked() => Debug.Log("Pause execution");
        private void OnStopClicked() => Debug.Log("Stop execution");
        private void OnHelpClicked() => Debug.Log("Show help");
    }
}
