using UnityEditor;
using UnityEngine;

namespace Dynamis.Behaviours.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {
        public static BehaviourEditorWindow Instance { get; private set; }
        
        [MenuItem("Window/Dynamis/Behaviour Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<BehaviourEditorWindow>("Behaviour Editor");
            Instance.titleContent = new GUIContent("Behaviour Editor");
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1;
            
            var twoColumnLayout = new TwoColumnLayout
            {
                style =
                {
                    flexGrow = 1
                }
            };
            root.Add(twoColumnLayout);
        }
    }
}