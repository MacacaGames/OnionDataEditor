#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    public class TreeRoot : TreeNode
    {
        public DataObjTreeView treeView;

        public TreeRoot(Object _) : base(_)
        {
            dataObj = _;
            displayName = GetTitle();
        }

        public void SetTreeRoot()
        {
            CreatTree(this);

            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();

            if (window.treeViewState == null)
                treeView = new DataObjTreeView(this, new TreeViewState());
            else
                treeView = new DataObjTreeView(this, window.treeViewState);

            if (window.treeViewState == null)
                window.treeViewState = treeView.state;
            
        }

        void CreatTree(TreeNode node)
        {
            if (node == null) return;

            node.GetElementTree();
        }
    }

    public class TreeNode
    {
        public Object dataObj { get; protected set; }

        public bool isPseudo => nodeFlag.HasFlag(NodeFlag.Pseudo);
        public bool isNull => !isPseudo && dataObj == null;
        public bool isHideElementNodes => nodeFlag.HasFlag(NodeFlag.HideElementNodes);

        public string displayName;
        public Texture icon = null;
        public TreeNode parent = null;
        public List<TreeNode> nodes = new List<TreeNode>();

        public List<OnionAction> nodeActions;
        public OnionAction onSelectedAction = null;
        public OnionAction onDoubleClickAction = null;
        public OnionAction onInspectorAction = null;

        UnityEditor.Editor editorCache = null;

        [System.Flags]
        public enum NodeFlag
        {
            None = 0,
            Pseudo = 1 << 0,
            HideElementNodes = 1 << 1,
        }
        public NodeFlag nodeFlag = NodeFlag.None;

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
            icon = GetIcon();

            if (dataObj != null)
            {
                editorCache = UnityEditor.Editor.CreateEditor(dataObj);
                onInspectorAction = GetInspectorDrawer();
            }

            nodeActions = new List<OnionAction>(dataObj.GetNodeActions());

            onSelectedAction = dataObj.GetNodeOnSelectedAction();
            onDoubleClickAction = dataObj.GetNodeOnDoubleClickAction();

        }

        public string GetTitle()
        {
            if (isPseudo)
                return displayName;
            else if (isNull)
                return "NULL";
            else
                return dataObj.GetNodeTitle() ?? dataObj.name;
        }
        public string GetDescription()
        {
            if (isPseudo)
                return "";
            else if (isNull)
                return "";
            else
                return dataObj.GetNodeDescription() ?? "";
        }
        public Texture GetIcon()
        {
            if (isPseudo)
                return icon;
            else if (isNull)
                return EditorGUIUtility.FindTexture("console.erroricon.sml");
            else
                return dataObj.GetNodeIcon();
        }
        public OnionAction GetInspectorDrawer()
        {
            if (isPseudo == false && dataObj != null)
            {
                return new OnionAction(editorCache.OnInspectorGUI);
            }

            return null;
        }
    }
        
}

#else

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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