using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public abstract class CompositeNode : Node
    {
        [SerializeField] protected List<Node> children = new List<Node>();

        public List<Node> Children => children;

        public void AddChild(Node child)
        {
            if (child != null && !children.Contains(child))
            {
                children.Add(child);
                child.SetBehaviourTree(tree);
            }
        }

        public void RemoveChild(Node child)
        {
            children.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
        }

        public override void Reset()
        {
            base.Reset();
            foreach (var child in children)
            {
                child?.Reset();
            }
        }

        public override Node Clone()
        {
            CompositeNode clone = (CompositeNode)base.Clone();
            clone.children = new List<Node>();
            
            foreach (var child in children)
            {
                if (child != null)
                    clone.children.Add(child.Clone());
            }
            
            return clone;
        }
    }
}
