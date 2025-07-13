using UnityEditor;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {
        public static BehaviourEditorWindow Instance { get; private set; }
        
        [MenuItem("Window/Dynamis/Behaviour Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<BehaviourEditorWindow>("Behaviour Editor");
        }

        private void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Dynamis/Behaviours/Editor/TwoColumnLayout.uxml");
            visualTree.CloneTree(rootVisualElement);
        }
    }
}