
#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace OnionCollections.DataEditor.Editor
{

    public class OnionAction
    {
        public Action action { get; private set; }
        public string actionName { get; private set; }
        public OnionAction(Action action)
        {
            this.action = action;
        }
        public OnionAction(Action action, string actionName)
        {
            this.action = action;
            this.actionName = actionName;
        }
        public OnionAction(MethodInfo method, ScriptableObject target, string actionName)
        {
            action = CreateOpenDelegate(method, target);
            this.actionName = actionName;
        }
        private static Action CreateOpenDelegate(MethodInfo method, ScriptableObject target)
        {
            return (Action)Delegate.CreateDelegate(type: typeof(Action), target, method, true);
        }
    }
}

#endif