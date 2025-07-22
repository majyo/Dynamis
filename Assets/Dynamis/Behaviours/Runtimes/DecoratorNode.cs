using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public abstract class DecoratorNode : Node
    {
        [SerializeField] protected Node child;

        public Node Child 
        { 
            get => child; 
            set 
            { 
                child = value;
                if (child != null)
                    child.SetBehaviourTree(tree);
            } 
        }

        public override void ResetNode()
        {
            base.ResetNode();
            child?.ResetNode();
        }

        public override Node Clone()
        {
            DecoratorNode clone = (DecoratorNode)base.Clone();
            if (child != null)
                clone.child = child.Clone();
            return clone;
        }
    }
}
