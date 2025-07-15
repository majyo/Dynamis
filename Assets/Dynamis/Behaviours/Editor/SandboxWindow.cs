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
            
            var instructionLabel = new Label("• 右键显示菜单\n• 点击菜单外任意地方隐藏菜单\n• 选择菜单项执行操作");
            instructionLabel.style.fontSize = 12;
            instructionLabel.style.color = Color.gray;
            instructionLabel.style.marginTop = 10;
            instructionLabel.style.marginLeft = 20;
            instructionLabel.style.whiteSpace = WhiteSpace.Normal;
            mainContainer.Add(instructionLabel);
            
            // 创建弹出菜单
            CreatePopupMenu();
            
            // 注册右键事件
            RegisterRightClickHandler(mainContainer);
            
            // 注册点击外部隐藏菜单的事件
            RegisterClickOutsideHandler();
        }
        
        private void CreatePopupMenu()
        {
            _popupMenu = new CustomPopupMenu();
            _popupMenu.Title = "操作菜单";
            
            // 添加菜单项
            _popupMenu.AddMenuItem("创建新对象", () => {
                Debug.Log("创建新对象被点击");
                ShowNotification(new GUIContent("创建新对象"));
            });
            
            _popupMenu.AddMenuItem("删除选中对象", () => {
                Debug.Log("删除选中对象被点击");
                ShowNotification(new GUIContent("删除选中对象"));
            });
            
            _popupMenu.AddSeparator();
            
            _popupMenu.AddMenuItem("复制", () => {
                Debug.Log("复制被点击");
                ShowNotification(new GUIContent("复制"));
            });
            
            _popupMenu.AddMenuItem("粘贴", () => {
                Debug.Log("粘贴被点击");
                ShowNotification(new GUIContent("粘贴"));
            });
            
            _popupMenu.AddSeparator();
            
            _popupMenu.AddMenuItem("属性", () => {
                Debug.Log("属性被点击");
                ShowNotification(new GUIContent("打开属性"));
            });
            
            _popupMenu.AddMenuItem("禁用项", null, false); // 禁用的菜单项
            
            // 将菜单添加到根元素
            _rootElement.Add(_popupMenu);
        }
        
        private void RegisterRightClickHandler(VisualElement targetElement)
        {
            targetElement.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1) // 右键
                {
                    evt.StopPropagation();
                    
                    // 获取鼠标位置
                    var mousePosition = evt.mousePosition;
                    
                    // 显示弹出菜单
                    _popupMenu.Show(mousePosition);
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