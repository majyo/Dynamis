using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Dynamis.Behaviours.Editor
{
    public class TwoColumnLayoutDemo : EditorWindow
    {
        [MenuItem("Dynamis/Two Column Layout Demo")]
        public static void ShowWindow()
        {
            var window = GetWindow<TwoColumnLayoutDemo>();
            window.titleContent = new GUIContent("Two Column Layout Demo");
            window.minSize = new Vector2(600, 400);
        }
        
        public void CreateGUI()
        {
            // 创建主容器
            var root = rootVisualElement;
            root.style.flexGrow = 1;
            
            // 添加标题
            var title = new Label("可拖拽的双栏布局示例");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.paddingTop = 10;
            title.style.paddingBottom = 10;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(title);
            
            // 创建TwoColumnLayout实例
            var twoColumnLayout = new TwoColumnLayout();
            twoColumnLayout.style.flexGrow = 1;
            root.Add(twoColumnLayout);
            
            // 添加左侧内容
            var leftContent = twoColumnLayout.LeftContent;
            if (leftContent != null)
            {
                leftContent.Clear(); // 清除默认内容
                
                var leftTitle = new Label("左侧面板");
                leftTitle.style.fontSize = 14;
                leftTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                leftTitle.style.marginBottom = 10;
                leftContent.Add(leftTitle);
                
                var toggleDemo = new Toggle("启用功能");
                toggleDemo.value = true;
                leftContent.Add(toggleDemo);
                
                var buttonDemo = new Button(() => Debug.Log("左侧按钮被点击"))
                {
                    text = "测试按钮"
                };
                leftContent.Add(buttonDemo);
                
                var sliderDemo = new Slider("透明度", 0, 1)
                {
                    value = 0.5f
                };
                leftContent.Add(sliderDemo);
            }
            
            // 添加右侧内容
            var rightContent = twoColumnLayout.RightContent;
            if (rightContent != null)
            {
                rightContent.Clear(); // 清除默认内容
                
                var rightTitle = new Label("右侧面板（主要内容区域）");
                rightTitle.style.fontSize = 14;
                rightTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                rightTitle.style.marginBottom = 10;
                rightContent.Add(rightTitle);
                
                var textField = new TextField("输入框")
                {
                    value = "在这里输入文本..."
                };
                rightContent.Add(textField);
                
                var objectField = new ObjectField("游戏对象")
                {
                    objectType = typeof(GameObject)
                };
                rightContent.Add(objectField);
                
                var helpBox = new HelpBox("拖拽中间的分割线可以调整左右面板的比例。最小宽度为200像素。", HelpBoxMessageType.Info);
                rightContent.Add(helpBox);
                
                // 添加比例控制按钮
                var buttonContainer = new VisualElement();
                buttonContainer.style.flexDirection = FlexDirection.Row;
                buttonContainer.style.marginTop = 10;
                
                var ratio25Button = new Button(() => twoColumnLayout.SetPanelRatio(25f))
                {
                    text = "25:75"
                };
                ratio25Button.style.marginRight = 5;
                buttonContainer.Add(ratio25Button);
                
                var ratio33Button = new Button(() => twoColumnLayout.SetPanelRatio(33f))
                {
                    text = "33:67"
                };
                ratio33Button.style.marginRight = 5;
                buttonContainer.Add(ratio33Button);
                
                var ratio50Button = new Button(() => twoColumnLayout.SetPanelRatio(50f))
                {
                    text = "50:50"
                };
                buttonContainer.Add(ratio50Button);
                
                rightContent.Add(buttonContainer);
            }
        }
    }
}
