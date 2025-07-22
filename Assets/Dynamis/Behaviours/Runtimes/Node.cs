// using Dynamis.Behaviours.Runtimes.Blackboards;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }

    [System.Serializable]
    public abstract class Node : ScriptableObject
    {
        [SerializeField] protected string guid;
        [SerializeField] protected Vector2 position;
        [SerializeField] protected NodeState state = NodeState.Running;
        [SerializeField] protected bool started;

        protected BehaviourTree tree;

        public string Guid 
        { 
            get 
            {
                if (string.IsNullOrEmpty(guid))
                    guid = System.Guid.NewGuid().ToString();
                return guid;
            }
        }

        public Vector2 Position 
        { 
            get => position; 
            set => position = value; 
        }

        public NodeState State => state;
        public BehaviourTree Tree => tree;
        // protected Blackboard Blackboard => tree?.Blackboard;
        
        public NodeState Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state == NodeState.Failure || state == NodeState.Success)
            {
                OnStop();
                started = false;
            }

            return state;
        }
        
        public virtual void ResetNode()
        {
            state = NodeState.Running;
            started = false;
            OnReset();
        }

        public void SetBehaviourTree(BehaviourTree behaviourTree)
        {
            tree = behaviourTree;
        }

        public virtual Node Clone()
        {
            Node clone = Instantiate(this);
            clone.guid = System.Guid.NewGuid().ToString();
            return clone;
        }
        
        public void AddToAsset(ScriptableObject asset)
        {
        }

        public void RemoveFromAsset(ScriptableObject asset)
        {
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnReset() { }
        protected abstract NodeState OnUpdate();

        public virtual void OnDrawGizmos() { }
    }
}
