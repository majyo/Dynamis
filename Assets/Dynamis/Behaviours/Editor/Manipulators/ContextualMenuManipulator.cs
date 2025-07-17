using System;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Manipulators
{
    public class ContextualMenuManipulator : Manipulator
    {
        private readonly Action<ContextualMenuPopulateEvent> _menuBuilder;

        public ContextualMenuManipulator(Action<ContextualMenuPopulateEvent> menuBuilder)
        {
            _menuBuilder = menuBuilder;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuPopulate);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuPopulate);
        }

        private void OnContextualMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            _menuBuilder?.Invoke(evt);
        }
    }
}

