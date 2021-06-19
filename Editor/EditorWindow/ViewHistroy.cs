using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    internal class ViewHistroy
    {
        OnionDataEditorWindow window;

        public Action OnHistroyChange = null;

        public ViewHistroy(OnionDataEditorWindow window)
        {
            this.window = window;
        }


        public struct ViewHistroyState
        {
            public Object target;
            public TreeNode node;
            public string DisplayName { get; private set; }

            public ViewHistroyState(Object target)
            {
                this.target = target;
                node = null;
                DisplayName = target.name;
            }

            public ViewHistroyState(TreeNode node)
            {
                if (node.IsPseudo == false)
                {
                    target = node.Target;
                    this.node = null;
                    DisplayName = target.name;
                }
                else
                {
                    target = null;
                    this.node = node;
                    DisplayName = node.displayName;
                }
            }

            public TreeNode GetNode()
            {
                if (node == null)
                    return new TreeNode(target);

                return node;
            }

            static public bool Equals(ViewHistroyState x, ViewHistroyState y)
            {
                return x.node == y.node && x.target == y.target;
            }

        }

        readonly Stack<ViewHistroyState> histroy = new Stack<ViewHistroyState>();

        public int Count => histroy.Count;

        public void PushState(TreeNode node)
        {
            histroy.Push(new ViewHistroyState(node));
            window.OnTargetChange(node);

            HistroyChange();
        }

        public void ReplaceState(TreeNode node)
        {
            if (histroy.Count > 0)
            {
                histroy.Pop();

                var state = new ViewHistroyState(node);

                if (histroy.Count > 0)
                {
                    var parentState = histroy.Peek();
                    if (ViewHistroyState.Equals(parentState, state))
                    {
                        histroy.Pop();
                    }
                }

                histroy.Push(state);
                window.OnTargetChange(node);
            }
            HistroyChange();
        }

        public void Back()
        {
            if(histroy.Count > 0)
            {
                histroy.Pop();
                var state = histroy.Peek();
                window.OnTargetChange(state.GetNode());
            }

            HistroyChange();
        }

        public void Clear()
        {
            histroy.Clear();

            HistroyChange();
        }

        void HistroyChange()
        {
            //string log = string.Join(" > ", histroy.Reverse().Select(n => n.DisplayName));
            //Debug.Log(log);

            OnHistroyChange?.Invoke();
        }

    }
}