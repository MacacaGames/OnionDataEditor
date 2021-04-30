using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    public class TreeNode: IEnumerable<TreeNode>
    {
        /// <summary>Target object of this node.</summary>
        public Object Target { get; protected set; }



        /// <summary>Return true if this node is pseudo.</summary>
        public bool IsPseudo => Flag.HasFlag(NodeFlag.Pseudo);

        /// <summary>Return true if this node target is null and node is not pseudo.</summary>
        public bool IsNull => !IsPseudo && Target == null;

        /// <summary>Return true if this node hide children nodes.</summary>
        public bool IsHideElementNodes => Flag.HasFlag(NodeFlag.HideElementNodes);



        readonly protected List<TreeNode> children = new List<TreeNode>();
        
        /// <summary>Length of children.</summary>
        internal int ChildCount => children.Count;

        /// <summary>Return this node's parent node.</summary>
        public TreeNode Parent { get; private set; }

        /// <summary>Display name of this node.</summary>
        public string displayName;

        /// <summary>Description of this node.</summary>
        public string description;

        /// <summary>Display icon of this node.</summary>
        public Texture icon;

        /// <summary>Display color tag of this node. Color will not display if value is null.</summary>
        public Color tagColor = new Color(0, 0, 0, 0);

        /// <summary>Tags use for any custom utilties if you want.</summary>
        internal string[] tags = new string[0];



        /// <summary>Action will be executed when node be selected.</summary>
        public OnionAction OnSelectedAction { get; set; }

        /// <summary>Action will be executed when node be double clicked.</summary>
        public OnionAction OnDoubleClickAction { get; set; }

        /// <summary>Action when node rebuild. It will not execute when node first build.</summary>
        internal OnionAction OnRebuildNode { get; set; }

        /// <summary>Actions of this node. Will display as button in data editor.</summary>
        public IEnumerable<OnionAction> NodeActions { get; set; }

        OnionAction _OnInspectorAction;
        /// <summary>Inspector action of this node.</summary>
        public OnionAction OnInspectorAction
        {
            get
            {
                if (IsPseudo == false && Target != null)
                {
                    if (_OnInspectorAction == null)
                    {
                        _OnInspectorAction = new OnionAction(EditorCache.OnInspectorGUI);
                    }
                }

                return _OnInspectorAction;                
            }
            set => _OnInspectorAction = value;
        }

        VisualElement _OnInspectorVisualElementRoot;
        /// <summary>Inspector visual element of this node.</summary>
        public VisualElement OnInspectorVisualElementRoot
        {
            get
            {
                if (IsPseudo == false && Target != null)
                {
                    if (_OnInspectorVisualElementRoot == null)
                    {
                        _OnInspectorVisualElementRoot = EditorCache.CreateInspectorGUI();
                    }
                }

                return _OnInspectorVisualElementRoot;
            }
            set => _OnInspectorVisualElementRoot = value;
        }

        UnityEditor.Editor _EditorCache;
        UnityEditor.Editor EditorCache
        {
            get
            {
                if (Target != null)
                {
                    if (_EditorCache == null)
                    {
                        _EditorCache = UnityEditor.Editor.CreateEditor(Target);
                    }
                    return _EditorCache;
                }
                else
                {
                    return null;
                }
            }
        }





        [System.Flags]
        public enum NodeFlag
        {
            None = 0,

            /// <summary>If node is pseudo, means it has no target object.</summary>
            Pseudo = 1 << 0,

            /// <summary>If node set this flag, all of children nodes don't show in data editor.</summary>
            HideElementNodes = 1 << 1,
        }

        internal NodeFlag Flag { get; set; } = NodeFlag.Pseudo;

        public TreeNode(Object target, NodeFlag flag = NodeFlag.None)
        {
            Target = target;
            Flag = flag;

            if (IsPseudo == false)
                InitSetting();
        }

        public TreeNode(NodeFlag flag, Object target = null)
        {
            Target = target;
            Flag = flag;
            
            if (IsPseudo == false)
                InitSetting();
        }

        public TreeNode()
        {
            Target = null;
            Flag = NodeFlag.Pseudo;
        }

        internal void InitSetting()
        {
            displayName = GetTitle();
            description = GetDescription();
            icon = GetIcon();
            tagColor = GetColorTag();

            if (IsNull == false)
            {
                NodeActions = Target.GetTargetActions();
                OnSelectedAction = Target.GetTargetOnSelectedAction();
                OnDoubleClickAction = Target.GetTargetOnDoubleClickAction();
            }

        }




        /// <summary>Add children in this node.</summary>
        public void AddChildren(IEnumerable<TreeNode> children)
        {
            this.children.AddRange(children);
            foreach (var child in children)
                child.Parent = this;
        }

        /// <summary>Add single child in this node.</summary>
        public void AddSingleChild(TreeNode child)
        {
            this.children.Add(child);
            child.Parent = this;
        }

        /// <summary>Clear all children.</summary>
        public void ClearChildren()
        {
            foreach(var ch in children)
            {
                ch.Parent = null;
            }

            children.Clear();
        }

        /// <summary>Get all children.</summary>
        public IEnumerable<TreeNode> GetChildren()
        {
            return children;
        }





        string GetTitle()
        {
            if (IsPseudo)
                return displayName;

            if (IsNull)
                return "NULL";

            string nodeTitle = Target.GetTargetTitle();

            if (string.IsNullOrEmpty(nodeTitle) == false)            
                return nodeTitle;
            
            if (string.IsNullOrEmpty(displayName) == false)            
                return displayName;
            
            return Target.name;

        }

        string GetDescription()
        {
            if (IsPseudo)
                return "";

            if (IsNull)
                return "";

            string des = Target.GetTargetDescription();

            if (string.IsNullOrEmpty(des) == true)
                return "";

            return des;
        }

        Texture GetIcon()
        {
            if (IsPseudo)
                return icon;

            if (IsNull)
                return OnionDataEditor.SmallErrorIcon;

            Texture nodeIcon = Target.GetTargetIcon();

            if (nodeIcon != null)
                return nodeIcon;

            if (Target is GameObject || Target is Component)
                return EditorGUIUtility.ObjectContent(null, Target.GetType()).image;
            
            return null;

        }

        Color GetColorTag()
        {
            if (IsPseudo)
                return tagColor;

            if (IsNull)
                return tagColor;

            Color c = Target.GetTargetTagColor();
            return c;
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var child in children)
                yield return child;
        }

        IEnumerator<TreeNode> IEnumerable<TreeNode>.GetEnumerator()
        {
            foreach (var child in children)
                yield return child;
        }

    }

}