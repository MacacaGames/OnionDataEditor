#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEngine.UIElements;

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

    public class TreeNode: IQueryableData
    {
        public Object dataObj { get; protected set; }

        public bool isPseudo => nodeFlag.HasFlag(NodeFlag.Pseudo);
        public bool isNull => !isPseudo && dataObj == null;
        public bool isHideElementNodes => nodeFlag.HasFlag(NodeFlag.HideElementNodes);

        public string displayName;
        public Texture icon = null;
        List<TreeNode> children = new List<TreeNode>();
        public TreeNode parent;
        public int childCount => children.Count;


        public OnionAction onSelectedAction;
        public OnionAction onDoubleClickAction;

        public List<OnionAction> nodeActions;

        public OnionAction _onInspectorAction;
        public OnionAction onInspectorAction
        {
            get => _onInspectorAction ?? (_onInspectorAction = GetInspectorDrawer());
            set => _onInspectorAction = value;
        }

        public VisualElement _onInspectorVisualElementRoot;
        public VisualElement onInspectorVisualElementRoot
        {
            get =>_onInspectorVisualElementRoot ?? (_onInspectorVisualElementRoot = editorCache?.CreateInspectorGUI());
            set => _onInspectorVisualElementRoot = value;
        }

        UnityEditor.Editor _editorCache;
        UnityEditor.Editor editorCache
        {
            get
            {
                if (dataObj != null)
                    return _editorCache ?? (_editorCache = UnityEditor.Editor.CreateEditor(dataObj));
                return null;
            }
        }

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
                nodeActions = new List<OnionAction>(dataObj.GetNodeActions());

                onSelectedAction = dataObj.GetNodeOnSelectedAction();
                onDoubleClickAction = dataObj.GetNodeOnDoubleClickAction();
            }

        }

        public void AddChildren(IEnumerable<TreeNode> children)
        {
            this.children.AddRange(children);
            foreach (var child in children)
                child.parent = this;
        }
        public void ClearChildren()
        {
            children.Clear();
        }
        public IEnumerable<TreeNode> GetChildren()
        {
            return children;
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

        public string GetID()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<IQueryableData> GetEnumerator()
        {
            foreach (var child in children)
                yield return child;

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var child in children)
                yield return child;
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