using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public interface IGraphNode
    {
        Vector2 Position { get; set; }
        
        void AddToAsset(ScriptableObject asset);
        void RemoveFromAsset(ScriptableObject asset);
    }
}