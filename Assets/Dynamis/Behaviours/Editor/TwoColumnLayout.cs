using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Dynamis.Behaviours.Editor
{
    [UxmlElement]
    public partial class TwoColumnLayout : VisualElement
    {
        private const string UxmlPath = "Assets/Dynamis/Behaviours/Editor/TwoColumnLayout.uxml";
        private const float MinPanelWidth = 200f;
        private const float SplitterWidth = 4f;
        
        private VisualElement leftPanel;
        private VisualElement rightPanel;
        private VisualElement splitter;
        private bool isDragging = false;
        private float totalWidth;
        private VisualElement rootContainer; // 添加根容器引用
        
        public TwoColumnLayout()
        {
            InitializeFromUxml();
            SetupSplitter();
        }
        
        private void InitializeFromUxml()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            
            if (visualTree != null)
            {
                visualTree.CloneTree(this);
            }
            
            leftPanel = this.Q<VisualElement>("left-panel");
            rightPanel = this.Q<VisualElement>("right-panel");
        }
        
        private void SetupSplitter()
        {
            // 创建分割线
            splitter = new VisualElement
            {
                name = "splitter"
            };
            splitter.AddToClassList("splitter");
            
            // 将分割线插入到左右面板之间
            var container = this.Q<VisualElement>("two-column-container");
            rootContainer = container; // 保存容器引用
            container.Insert(1, splitter);
            
            // 注册鼠标事件 - 只在splitter上监听MouseDown
            splitter.RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // 注册几何变化事件以更新总宽度
            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            totalWidth = evt.newRect.width;
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0) // 左键
            {
                isDragging = true;
                
                // 在根容器上注册全局鼠标事件，确保即使鼠标离开splitter也能继续拖拽
                this.RegisterCallback<MouseMoveEvent>(OnGlobalMouseMove, TrickleDown.TrickleDown);
                this.RegisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
                
                evt.StopPropagation();
            }
        }
        
        private void OnGlobalMouseMove(MouseMoveEvent evt)
        {
            if (isDragging && totalWidth > 0)
            {
                var containerRect = rootContainer.worldBound;
                var relativeX = evt.mousePosition.x - containerRect.x;
                
                // 计算新的左面板宽度百分比
                var newLeftWidth = Mathf.Clamp(relativeX, MinPanelWidth, totalWidth - MinPanelWidth - SplitterWidth);
                var leftWidthPercent = (newLeftWidth / totalWidth) * 100f;
                var rightWidthPercent = ((totalWidth - newLeftWidth - SplitterWidth) / totalWidth) * 100f;
                
                // 更新面板宽度
                leftPanel.style.flexBasis = new StyleLength(new Length(leftWidthPercent, LengthUnit.Percent));
                rightPanel.style.flexBasis = new StyleLength(new Length(rightWidthPercent, LengthUnit.Percent));
                
                evt.StopPropagation();
            }
        }
        
        private void OnGlobalMouseUp(MouseUpEvent evt)
        {
            if (isDragging)
            {
                isDragging = false;
                splitter.ReleaseMouse();
                
                // 取消注册全局鼠标事件
                this.UnregisterCallback<MouseMoveEvent>(OnGlobalMouseMove, TrickleDown.TrickleDown);
                this.UnregisterCallback<MouseUpEvent>(OnGlobalMouseUp, TrickleDown.TrickleDown);
                
                evt.StopPropagation();
            }
        }
        
        /// <summary>
        /// 设置左右面板的比例
        /// </summary>
        /// <param name="leftPercent">左面板占比 (0-100)</param>
        public void SetPanelRatio(float leftPercent)
        {
            leftPercent = Mathf.Clamp(leftPercent, 10f, 90f);
            var rightPercent = 100f - leftPercent;
            
            leftPanel.style.flexBasis = new StyleLength(new Length(leftPercent, LengthUnit.Percent));
            rightPanel.style.flexBasis = new StyleLength(new Length(rightPercent, LengthUnit.Percent));
        }
        
        /// <summary>
        /// 获取左面板内容容器
        /// </summary>
        public VisualElement LeftContent => leftPanel?.Q<VisualElement>("left-content");
        
        /// <summary>
        /// 获取右面板内容容器
        /// </summary>
        public VisualElement RightContent => rightPanel?.Q<VisualElement>("right-content");
    }
}
