using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Dynamis.Behaviours.Editor.Views;

namespace Dynamis.Behaviours.Editor
{
    public class SandboxWindow : EditorWindow
    {
        public static SandboxWindow Instance { get; private set; }
        
        private CustomPopupMenu _popupMenu;
        private VisualElement _rootElement;
        private SelectionBox _selectionBox;
        private bool _isDragging;
        
        [MenuItem("Dynamis/Sandbox")]
        public static void ShowWindow()
        {
            Instance = GetWindow<SandboxWindow>("Sandbox Window");
            Instance.titleContent = new GUIContent("Sandbox Window");
        }

        private void CreateGUI()
        {
            _rootElement = rootVisualElement;
            
            // 创建主要内容区域
            var mainContainer = new VisualElement();
            mainContainer.style.flexGrow = 1;
            mainContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            _rootElement.Add(mainContainer);
            
            // 添加一些示例内容
            var label = new Label("右键任意位置打开弹出菜单");
            label.style.fontSize = 16;
            label.style.color = Color.white;
            label.style.marginTop = 20;
            label.style.marginLeft = 20;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            mainContainer.Add(label);
            
            var instructionLabel = new Label("• 右键显示菜单\n• 点击菜单外任意地方隐藏菜单\n• 选择菜单项执行操作\n• 左键拖拽绘制框选框");
            instructionLabel.style.fontSize = 12;
            instructionLabel.style.color = Color.gray;
            instructionLabel.style.marginTop = 10;
            instructionLabel.style.marginLeft = 20;
            instructionLabel.style.whiteSpace = WhiteSpace.Normal;
            mainContainer.Add(instructionLabel);
            
            // 创建弹出菜单
            CreatePopupMenu();
            
            // 创建框选框
            CreateSelectionBox();
            
            // 注册右键事件
            RegisterRightClickHandler(mainContainer);
            
            // 注册左键拖拽事件
            RegisterLeftClickDragHandler(mainContainer);
            
            // 注册点击外部隐藏菜单的事件
            RegisterClickOutsideHandler();
        }
        
        private void CreatePopupMenu()
        {
            _popupMenu = new CustomPopupMenu
            {
                Title = "操作菜单"
            };

            // 添加基本菜单项
            _popupMenu.AddMenuItem("创建新对象", () => {
                Debug.Log("创建新对象被点击");
                ShowNotification(new GUIContent("创建新对象"));
            });
            
            _popupMenu.AddMenuItem("删除选中对象", () => {
                Debug.Log("删除选中对象被点击");
                ShowNotification(new GUIContent("删除选中对象"));
            });
            
            _popupMenu.AddSeparator();
            
            // 添加编辑分组
            var editGroup = _popupMenu.AddGroup("编辑操作");
            editGroup.AddChild("复制", () => {
                Debug.Log("复制被点击");
                ShowNotification(new GUIContent("复制"));
            });
            editGroup.AddChild("粘贴", () => {
                Debug.Log("粘贴被点击");
                ShowNotification(new GUIContent("粘贴"));
            });
            editGroup.AddChild("剪切", () => {
                Debug.Log("剪切被点击");
                ShowNotification(new GUIContent("剪切"));
            });
            
            editGroup.AddChildSeparator();
            
            editGroup.AddChild("撤销", () => {
                Debug.Log("撤销被点击");
                ShowNotification(new GUIContent("撤销"));
            });
            editGroup.AddChild("重做", () => {
                Debug.Log("重做被点击");
                ShowNotification(new GUIContent("重做"));
            });
            
            // 添加变换分组
            var transformGroup = _popupMenu.AddGroup("变换");
            transformGroup.AddChild("移动", () => {
                Debug.Log("移动被点击");
                ShowNotification(new GUIContent("移动"));
            });
            transformGroup.AddChild("旋转", () => {
                Debug.Log("旋转被点击");
                ShowNotification(new GUIContent("旋转"));
            });
            transformGroup.AddChild("缩放", () => {
                Debug.Log("缩放被点击");
                ShowNotification(new GUIContent("缩放"));
            });
            
            // 在变换分组中添加子分组
            var advancedTransform = transformGroup.AddChildGroup("高级变换");
            advancedTransform.AddChild("重置位置", () => {
                Debug.Log("重置位置被点击");
                ShowNotification(new GUIContent("重置位置"));
            });
            advancedTransform.AddChild("重置旋转", () => {
                Debug.Log("重置旋转被点击");
                ShowNotification(new GUIContent("重置旋转"));
            });
            advancedTransform.AddChild("重置缩放", () => {
                Debug.Log("重置缩放被点击");
                ShowNotification(new GUIContent("重置缩放"));
            });
            
            _popupMenu.AddSeparator();
            
            // 添加查看分组
            var viewGroup = _popupMenu.AddGroup("查看");
            viewGroup.AddChild("聚焦到对象", () => {
                Debug.Log("聚焦到对象被点击");
                ShowNotification(new GUIContent("聚焦到对象"));
            });
            viewGroup.AddChild("框选显示", () => {
                Debug.Log("框选显示被点击");
                ShowNotification(new GUIContent("框选显示"));
            });
            
            // 添加属性菜单项
            _popupMenu.AddMenuItem("属性", () => {
                Debug.Log("属性被点击");
                ShowNotification(new GUIContent("打开属性"));
            });
            
            _popupMenu.AddMenuItem("禁用项", null, false); // 禁用的菜单项
            
            // 将菜单添加到根元素
            _rootElement.Add(_popupMenu);
        }
        
        private void CreateSelectionBox()
        {
            _selectionBox = new SelectionBox();
            _rootElement.Add(_selectionBox);
        }
        
        private void RegisterRightClickHandler(VisualElement targetElement)
        {
            targetElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1) // 右键
                {
                    evt.StopPropagation();
                    
                    _popupMenu.Show(evt.localMousePosition);
                }
            });
        }
        
        private void RegisterLeftClickDragHandler(VisualElement targetElement)
        {
            targetElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0) // 左键
                {
                    // 如果弹出菜单可见，先隐藏它
                    if (_popupMenu.IsVisible)
                    {
                        _popupMenu.Hide();
                        return;
                    }
                    
                    _isDragging = true;
                    // 将鼠标位置转换为相对于根容器的坐标
                    // var localPosition = _rootElement.WorldToLocal(evt.mousePosition + (Vector2)targetElement.worldBound.position);
                    _selectionBox.StartSelection(evt.localMousePosition);
                    targetElement.CaptureMouse();
                    evt.StopPropagation();
                }
            });
            
            targetElement.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (_isDragging)
                {
                    // 将鼠标位置转换为相对于根容器的坐标
                    // var localPosition = _rootElement.WorldToLocal(evt.mousePosition + (Vector2)targetElement.worldBound.position);
                    // _selectionBox.UpdateSelection(localPosition);
                    _selectionBox.UpdateSelection(evt.localMousePosition);
                    evt.StopPropagation();
                }
            });
            
            targetElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 0 && _isDragging) // 左键释放
                {
                    _isDragging = false;
                    targetElement.ReleaseMouse();
                    
                    var selectionRect = _selectionBox.SelectionRect;
                    _selectionBox.EndSelection();
                    
                    // 这里可以添加框选逻辑
                    Debug.Log($"框选区域: {selectionRect}");
                    ShowNotification(new GUIContent($"框选区域: {selectionRect.width:F1} x {selectionRect.height:F1}"));
                    
                    evt.StopPropagation();
                }
            });
        }
        
        private void RegisterClickOutsideHandler()
        {
            _rootElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (_popupMenu.IsVisible)
                {
                    // 检查点击是否在菜单外部
                    var menuBounds = _popupMenu.worldBound;
                    var clickPosition = evt.mousePosition;
                    
                    if (!menuBounds.Contains(clickPosition))
                    {
                        _popupMenu.Hide();
                        evt.StopPropagation();
                    }
                }
            }, TrickleDown.TrickleDown);
        }
    }
}