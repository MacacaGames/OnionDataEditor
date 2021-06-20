using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
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


        public class ViewHistroyState
        {
            public Object target = null;
            public TreeNode node = null;
            public TreeViewState treeViewState = null;

            public string DisplayName { get; private set; }

            public ViewHistroyState(Object target)
            {
                this.target = target;
                DisplayName = target.name;
            }

            public ViewHistroyState(TreeNode node)
            {
                if (node.IsPseudo == false)
                {
                    target = node.Target;
                    DisplayName = target.name;
                }
                else
                {
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

        public ViewHistroyState Current => (Count >= 1) ? histroy.Peek() : null;
        public ViewHistroyState Last => (Count >= 2) ? histroy.Skip(1).First() : null;

        void SaveCurrentState()
        {
            if (histroy.Count > 0)
            {
                Current.treeViewState = new TreeViewState
                {
                    expandedIDs = window.treeView.state.expandedIDs,
                    selectedIDs = window.treeView.state.selectedIDs,
                };
            }
        }

        TreeViewState GetDefaultState()
        {
            //選擇Root並展開
            return new TreeViewState
            {
                selectedIDs = new List<int> { 1 },
                expandedIDs = new List<int> { 1 }
            };
        }

        public void PushState(TreeNode node)
        {
            SaveCurrentState();
            histroy.Push(new ViewHistroyState(node) { treeViewState = GetDefaultState() });
            window.OnTargetChange(node);

            HistroyChange();
        }

        public void ReplaceState(TreeNode node)
        {
            SaveCurrentState();
            if (histroy.Count > 0)
            {
                histroy.Pop();

                var state = new ViewHistroyState(node) { treeViewState = GetDefaultState() };

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
            if (histroy.Count > 0)
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

            OnHistroyChange?.Invoke();
        }

        internal void LogStates()
        {
            string log = string.Join(" > ", histroy.Reverse().Select(n => n.DisplayName));
            Debug.Log(log);
        }

    }
}