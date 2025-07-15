using UnityEditor;
using UnityEngine;

namespace Dynamis.Behaviours.Editor
{
    public class SandboxWindow : EditorWindow
    {
        public static SandboxWindow Instance { get; private set; }
        
        [MenuItem("Dynamis/Sandbox")]
        public static void ShowWindow()
        {
            Instance = GetWindow<SandboxWindow>("Sandbox Window");
            Instance.titleContent = new GUIContent("Sandbox Window");
        }

        private void CreateGUI()
        {
        }
    }
}