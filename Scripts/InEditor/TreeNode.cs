#if(UNITY_EDITOR)
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

        public TreeRoot(ScriptableObject _) : base(_)
        {
            dataObj = _;
            displayName = GetTitle();
        }

        public void SetTreeRoot()
        {
            //depth = 0;
            CreatTree(this, -1);

            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();

            if (window.treeViewState == null)
                treeView = new DataObjTreeView(this, new TreeViewState());
            else
                treeView = new DataObjTreeView(this, window.treeViewState);

            if (window.treeViewState == null)
                window.treeViewState = treeView.state;

        }

        void CreatTree(TreeNode node, int depth)
        {
            if (node == null) return;

            //深度過深
            if (depth >= maxDepth)
            {
                Debug.LogError("Too depth.");
                return;
            }

            //node.depth = depth;

            if (ReferenceCheck(node) == false)
            {
                EditorWindow.GetWindow<OnionDataEditorWindow>().target = null;
                throw new System.StackOverflowException($"{node.displayName} is a parent of itself.");
            }
            
            if (node.nodeFlag.HasFlag(NodeFlag.HideElementNodes) == false)
            {
                //嘗試抓下一層
                var els = node.dataObj.GetElements();
                if (els == null) return;


                //加入下一層
                node.nodes = new List<TreeNode>(els);


                //遍歷下一層
                foreach (var item in node.nodes)
                {
                    item.parent = node;
                    CreatTree(item, depth + 1);
                }
            }
        }
        
        bool ReferenceCheck(TreeNode node)
        {
            TreeNode checkNode = node;
            while (checkNode.parent != null)
            {
                checkNode = checkNode.parent;
                if (node.dataObj == checkNode.dataObj)
                    return false;
            }
            return true;
        }
    }

    public class TreeNode
    {
        public ScriptableObject dataObj { get; protected set; }

        protected const int maxDepth = 16;
        //protected int depth;

        public bool isPseudo => nodeFlag.HasFlag(NodeFlag.Pseudo);
        public bool isNull => !isPseudo && dataObj == null;

        public string displayName;
        public Texture icon = null;
        public TreeNode parent = null;
        public List<TreeNode> nodes = new List<TreeNode>();

        public List<OnionAction> nodeActions;
        public OnionAction onSelectedAction = null;
        public OnionAction onDoubleClickAction = null;
        public OnionAction onInspectorAction = null;

        [System.Flags]
        public enum NodeFlag
        {
            None = 0,
            Pseudo = 1 << 0,
            HideElementNodes = 1 << 1
        }
        public NodeFlag nodeFlag = NodeFlag.None;

        public TreeNode(ScriptableObject dataObj)
        {
            this.dataObj = dataObj;

            InitSetting();
        }

        public TreeNode(NodeFlag nodeFlag, ScriptableObject dataObj = null)
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

            nodeActions = new List<OnionAction>(dataObj.GetNodeActions());

            onSelectedAction = dataObj.GetNodeOnSelectedAction();
            onDoubleClickAction = dataObj.GetNodeOnDoubleClickAction();

        }

        public string GetTitle()
        {
            if (isPseudo)
                return displayName;
            else if (dataObj == null)
                return "NULL";
            else
                return dataObj.GetNodeTitle() ?? dataObj.name;
        }
        public string GetDescription()
        {
            if (isPseudo)
                return "";
            else if (dataObj == null)
                return "";
            else
                return dataObj.GetNodeDescription() ?? "";
        }
        public Texture GetIcon()
        {
            if (isPseudo)
                return null;
            else if (dataObj == null)
                return EditorGUIUtility.FindTexture("console.erroricon.sml");
            else
                return dataObj.GetNodeIcon();
        }
    }
        
}
#endif