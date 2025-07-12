using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Scripts.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private NodeGraphView graphView;
        private Texture2D indentationIcon;

        public void Configure(NodeGraphView nodeGraphView)
        {
            graphView = nodeGraphView;
            
            // 创建缩进图标
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                
                // 数学节点
                new SearchTreeGroupEntry(new GUIContent("Math"), 1),
                new SearchTreeEntry(new GUIContent("Math Node", indentationIcon))
                {
                    userData = typeof(MathNode),
                    level = 2
                },
                
                // 输入输出节点
                new SearchTreeGroupEntry(new GUIContent("Input/Output"), 1),
                new SearchTreeEntry(new GUIContent("Input Node", indentationIcon))
                {
                    userData = typeof(InputNode),
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Output Node", indentationIcon))
                {
                    userData = typeof(OutputNode),
                    level = 2
                },
                
                // 文本节点
                new SearchTreeGroupEntry(new GUIContent("Text"), 1),
                new SearchTreeEntry(new GUIContent("String Node", indentationIcon))
                {
                    userData = typeof(StringNode),
                    level = 2
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is System.Type nodeType)
            {
                var windowMousePosition = context.screenMousePosition - EditorWindow.focusedWindow.position.position;
                var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);
                
                graphView.CreateNode(nodeType, graphMousePosition);
                return true;
            }
            
            return false;
        }
    }
}
