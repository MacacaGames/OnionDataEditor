
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;


namespace OnionCollections.DataEditor.Editor
{
    public class OnionAction
    {
        public Action action { get; private set; }
        public string actionName { get; private set; }
        public Texture actionIcon { get; private set; }


        public OnionAction(Action action, string actionName = null, Texture actionIcon = null)
        {
            this.action = action;
            this.actionName = actionName;
            this.actionIcon = actionIcon;
        }

        public OnionAction(MethodInfo method, Object target, string actionName = null, Texture actionIcon = null)
        {
            action = CreateOpenDelegate(method, target);
            this.actionName = actionName;
            this.actionIcon = actionIcon;
        }

        private static Action CreateOpenDelegate(MethodInfo method, Object target)
        {
            return (Action)Delegate.CreateDelegate(typeof(Action), target, method, true);
        }
    }
}