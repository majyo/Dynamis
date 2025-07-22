using System.Collections.Generic;
using Dynamis.Behaviours.Runtimes.Blackboards;
using UnityEditor;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Dynamis/Behaviour Tree", order = 1)]
    public class BehaviourTreeAsset : ScriptableObject
    {
        private List<ScriptableObject> _assets;
        private List<ScriptableObject> Assets => _assets ??= new List<ScriptableObject>();
        
        private BehaviourTree _tree;
        private Blackboard _blackboard; 
        
        public void AddSubAsset(ScriptableObject subAsset)
        {
            AssetDatabase.AddObjectToAsset(subAsset, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void RemoveSubAsset(ScriptableObject subAsset)
        {
            AssetDatabase.RemoveObjectFromAsset(subAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}