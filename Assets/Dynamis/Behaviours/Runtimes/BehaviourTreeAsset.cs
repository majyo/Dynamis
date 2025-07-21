using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public class BehaviourTreeAsset : ScriptableObject
    {
        private List<ScriptableObject> _treeAssets = new();
    }
}