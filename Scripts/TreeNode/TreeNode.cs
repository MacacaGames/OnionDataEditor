

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;
using OnionCollections.DataEditor.Editor;

namespace OnionCollections.DataEditor
{
    public class TreeRoot : TreeNode
    {
        internal DataObjTreeView treeView;

        public TreeRoot(Object _) : base(_)
        {
            dataObj = _;
            displayName = GetTitle();
        }

        /// <summary>Use this root to create tree view.</summary>
        public void CreateTreeView()
        {
            CreatTree(this);

            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();

            if (window.treeViewState == null)
                treeView = new DataObjTreeView(this, new TreeViewState());
            else
                treeView = new DataObjTreeView(this, window.treeViewState);
            
        }

        void CreatTree(TreeNode node)
        {
            if (node == null) return;

            node.GetElementTree();
        }
    }

    public class TreeNode: IEnumerable<TreeNode>
    {
        /// <summary>Target object of this node.</summary>
        public Object dataObj { get; protected set; }

        /// <summary>Return true if this node is pseudo.</summary>
        public bool isPseudo => nodeFlag.HasFlag(NodeFlag.Pseudo);
        /// <summary>Return true if this node target is null, or node is pseudo.</summary>
        public bool isNull => !isPseudo && dataObj == null;
        /// <summary>Return true if this node hide children nodes.</summary>
        public bool isHideElementNodes => nodeFlag.HasFlag(NodeFlag.HideElementNodes);


        List<TreeNode> children = new List<TreeNode>();

        internal int childCount => children.Count;

        /// <summary>Return this node's parent node.</summary>
        public TreeNode parent { get; private set; }


        /// <summary>Display name of this node.</summary>
        public string displayName;
        /// <summary>Description of this node.</summary>
        public string description;
        /// <summary>Display icon of this node.</summary>
        public Texture icon = null;

        /// <summary>Action will be executed when node be selected.</summary>
        public OnionAction onSelectedAction;
        /// <summary>Action will be executed when node be double clicked.</summary>
        public OnionAction onDoubleClickAction;

        /// <summary>Actions of this node. Will display as button in data editor.</summary>
        public IEnumerable<OnionAction> nodeActions;

        OnionAction _onInspectorAction;
        /// <summary>Inspector action of this node.</summary>
        public OnionAction onInspectorAction
        {
            get
            {
                if (isPseudo == false && dataObj != null)
                {
                    if (_onInspectorAction == null)
                    {
                        _onInspectorAction = new OnionAction(editorCache.OnInspectorGUI);
                    }
                }

                return _onInspectorAction;                
            }
            set => _onInspectorAction = value;
        }

        VisualElement _onInspectorVisualElementRoot;
        /// <summary>Inspector visual element of this node.</summary>
        public VisualElement onInspectorVisualElementRoot
        {
            get
            {
                if (isPseudo == false && dataObj != null)
                {
                    if (_onInspectorVisualElementRoot == null)
                    {
                        _onInspectorVisualElementRoot = editorCache.CreateInspectorGUI();
                    }
                }

                return _onInspectorVisualElementRoot;
            }
            set => _onInspectorVisualElementRoot = value;
        }

        UnityEditor.Editor _editorCache;
        UnityEditor.Editor editorCache
        {
            get
            {
                if (dataObj != null)
                {
                    if (_editorCache == null)
                    {
                        _editorCache = UnityEditor.Editor.CreateEditor(dataObj);
                    }
                    return _editorCache;
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
            Pseudo = 1 << 0,
            HideElementNodes = 1 << 1,
        }
        internal NodeFlag nodeFlag = NodeFlag.None;

        public TreeNode(Object dataObj)
        {
            this.dataObj = dataObj;

            InitSetting();
        }

        public TreeNode(NodeFlag nodeFlag, Object dataObj = null)
        {
            this.dataObj = dataObj;
            this.nodeFlag = nodeFlag;
            
            if (isPseudo == false)
                InitSetting();
        }

        void InitSetting()
        {
            displayName = GetTitle();
            description = GetDescription();
            icon = GetIcon();

            if (dataObj != null)
            {
                nodeActions = dataObj.GetNodeActions();
                onSelectedAction = dataObj.GetNodeOnSelectedAction();
                onDoubleClickAction = dataObj.GetNodeOnDoubleClickAction();
            }

        }

        /// <summary>Add children in this node.</summary>
        public void AddChildren(IEnumerable<TreeNode> children)
        {
            this.children.AddRange(children);
            foreach (var child in children)
                child.parent = this;
        }
        /// <summary>Clear all children.</summary>
        public void ClearChildren()
        {
            children.Clear();
        }
        /// <summary>Get all children.</summary>
        public IEnumerable<TreeNode> GetChildren()
        {
            return children;
        }


        protected string GetTitle()
        {
            if (isPseudo)
            {
                return displayName;
            }
            else if (isNull)
            {
                return "NULL";
            }
            else
            {
                string nodeTitle = dataObj.GetNodeTitle();
                if (string.IsNullOrEmpty(nodeTitle) == false)
                {
                    return nodeTitle;
                }
                else if (string.IsNullOrEmpty(displayName) == false)
                {
                    return displayName;
                }
                else
                {
                    return dataObj.name;
                }
            }
        }
        string GetDescription()
        {
            if (isPseudo)
                return "";
            else if (isNull)
                return "";
            else
            {
                string des = dataObj.GetNodeDescription();
                if (string.IsNullOrEmpty(des) == true)
                {
                    return "";
                }
                else
                {
                    return des;
                }
            }
        }
        Texture GetIcon()
        {
            if (isPseudo)
                return icon;
            else if (isNull)
                return EditorGUIUtility.FindTexture("console.erroricon.sml");
            else
                return dataObj.GetNodeIcon();
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

#else

public class TreeNode
{
    [System.Flags]
    public enum NodeFlag
    {
        None = 0,
        Pseudo = 1 << 0,
        HideElementNodes = 1 << 1,
    }

    public TreeNode(ScriptableObject dataObj)
    {
    }

    public TreeNode(NodeFlag nodeFlag, ScriptableObject dataObj = null)
    {
    }
}

#endif