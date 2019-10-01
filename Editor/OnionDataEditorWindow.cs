using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using OnionCollections;

public class OnionDataEditorWindow : EditorWindow
{
    const string path = "Assets/OnionDataEditor";

    ScriptableObject _target;
    public ScriptableObject target
    {
        get => _target;
        set
        {
            _target = value;
            rootVisualElement.Q<ObjectField>("target-field").SetValueWithoutNotify(value);

            VisualElement containerRoot = rootVisualElement.Q("tree-view-container");
            containerRoot.visible = (value != null);
            if (value != null)
            {
                tree = new TreeRoot(_target);
                tree.SetTreeRoot();

                rootVisualElement.Q("btn-add-bookmark").style.display = (value != bookmarkGroup) ? DisplayStyle.Flex : DisplayStyle.None;
                rootVisualElement.Q("btn-bookmark").style.display = (value != bookmarkGroup) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    TreeViewState treeViewState;
    TreeRoot tree;

    IMGUIContainer treeViewContainer;


    [MenuItem("Window/Onion Data Editor")]
    public static OnionDataEditorWindow ShowWindow()
    {
        var window = GetWindow<OnionDataEditorWindow>();
        window.target = null;
        window.minSize = new Vector2(250, 300);

        return window;
    }

    public void OnEnable()
    {
        Init();
    }

    void Init()
    {
        //建構
        var root = this.rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "/Editor/Onion.uxml");
        var cloneTree = visualTree.CloneTree();
        cloneTree.style.flexGrow = 1;
        root.Add(cloneTree);

        CreateNeededAsset();

        //綁定btn-refresh
        root.Q<Button>("btn-refresh").clickable.clicked += () => { if (tree != null) tree.SetTreeRoot(); };
        root.Q("btn-refresh-icon").style.backgroundImage = EditorGUIUtility.FindTexture("d_Refresh");

        //綁定btn-bookmark
        root.Q<Button>("btn-bookmark").clickable.clicked += () => { target = bookmarkGroup; };
        root.Q("btn-bookmark-icon").style.backgroundImage = EditorGUIUtility.FindTexture("FolderFavorite Icon");

        //綁定btn-add-bookmark
        root.Q<Button>("btn-add-bookmark").clickable.clicked += () =>
        {
            if (bookmarkGroup != null && target != bookmarkGroup)
            {
                if (bookmarkGroup.GetData<OnionBookmark>().Select(_ => _.target).Contains(target) == false)
                {
                    var bookmark = CreateInstance<OnionBookmark>();
                    bookmark.target = target;

                    AssetDatabase.CreateAsset(bookmark, $"{path}/Bookmark/B_{target.name}.asset");
                    bookmarkGroup.AddData(bookmark);
                }
                else
                {
                    Debug.LogError("This bookmark already exists.");
                }
            }
        };
        root.Q("btn-add-bookmark-icon").style.backgroundImage = EditorGUIUtility.FindTexture("Favorite Icon");

        //建構treeview
        VisualElement containerRoot = root.Q("tree-view-container");
        if (treeViewContainer == null)
        {
            treeViewContainer = new IMGUIContainer(DrawTreeView) { name = "tree-view" };
            treeViewContainer.style.flexGrow = 1;
            containerRoot.Add(treeViewContainer);
        }

        //target-field設定與綁定
        root.Q<ObjectField>("target-field").objectType = typeof(ScriptableObject);
        root.Q<ObjectField>("target-field").RegisterValueChangedCallback(_ => target = _.newValue as ScriptableObject);
        

        var inspectorContainer = new IMGUIContainer(DrawInspector);
        inspectorContainer.AddToClassList("inspect-container");
        root.Q("inspector-scroll").Q("unity-content-container").Add(inspectorContainer);
    }

    DataGroup bookmarkGroup = null;
    void CreateNeededAsset()
    {
        //bookmark group
        bookmarkGroup = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path + "/OnionBookmarkGroup.asset") as DataGroup;
        if (bookmarkGroup == null)
        {
            bookmarkGroup = CreateInstance<DataGroup>();
            AssetDatabase.CreateAsset(bookmarkGroup, $"{path}/OnionBookmarkGroup.asset");
            bookmarkGroup = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path + "/OnionBookmarkGroup.asset") as DataGroup;
        }

        //bookmark folder
        if (AssetDatabase.IsValidFolder($"{path}/Bookmark") == false)
            AssetDatabase.CreateFolder(path, "Bookmark");

    }


    //IMGUIContainer Handler
    void DrawTreeView()
    {
        if (tree != null && target != null)
        {
            tree.treeView.OnGUI(treeViewContainer.layout);
        }
    }

    //Inspector
    Editor inspectorEditor;
    void DrawInspector()
    {
        if (selectObject != null)
        {
            if (inspectorEditor == null || inspectorEditor.target != selectObject)
                inspectorEditor = Editor.CreateEditor(selectObject);

            inspectorEditor.OnInspectorGUI();
        }
    }

    List<Button> actionBtns = new List<Button>();
    void DisplayInfo(ScriptableObject dataObj)
    {
        DisplayTextInfo(dataObj);
        DisplayActionButtonInfo(dataObj);
    }
    void DisplayTextInfo(ScriptableObject dataObj)
    {
        var root = this.rootVisualElement;

        //text info
        string nodeTitle = GetNodeTitle(dataObj) ?? "";
        string nodeDescription = GetNodeDescription(dataObj) ?? "";
        if (dataObj == null)
        {
            root.Q<Label>("info-title").text = "";
            root.Q<Label>("info-description").text = "";
        }
        else
        {
            root.Q<Label>("info-title").text = nodeTitle;
            root.Q<Label>("info-description").text = nodeDescription;
        }
    }
    void DisplayActionButtonInfo(ScriptableObject dataObj)
    {
        var root = this.rootVisualElement;

        var container = root.Q("data-info-btn-list");
        foreach (var actionBtn in actionBtns)
            container.Remove(actionBtn);
        actionBtns = new List<Button>();

        var actions = GetNodeActions(dataObj);
        foreach (var action in actions)
        {
            Button actionBtn = new Button()
            {
                text = action.actionName,
            };
            actionBtn.clickable.clicked += () => action.action();
            actionBtn.AddToClassList("onion-btn");
            actionBtns.Add(actionBtn);

            container.Add(actionBtn);
        }
    }



    //GetNode
    const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    static T TryGetNodeAttrValue<T>(ScriptableObject dataObj, Type attrType) where T : class
    {
        ReflectionUtility.GetValueResult result = new ReflectionUtility.GetValueResult
        {
            hasValue = false,
            value = null
        };

        if (dataObj != null)
        {
            var type = dataObj.GetType();
            var fields = type.GetMembers(defaultBindingFlags).FilterWithAttribute(attrType).ToList();

            if (fields.Count > 0)                                //若有掛Attribute
            {
                result = fields[0].TryGetValue<T>(dataObj);      //嘗試取得值
                if (result.hasValue)
                    return result.value as T;
            }
        }
        return null;
    }

    static string GetNodeTitle(ScriptableObject dataObj)
    {
        var result = TryGetNodeAttrValue<string>(dataObj, typeof(Onion.NodeTitleAttribute));
        return result as string;
    }
    static string GetNodeDescription(ScriptableObject dataObj)
    {
        var result = TryGetNodeAttrValue<string>(dataObj, typeof(Onion.NodeDescriptionAttribute));
        return result as string;
    }
    static Texture GetNodeIcon(ScriptableObject dataObj)
    {
        var result = TryGetNodeAttrValue<Texture>(dataObj, typeof(Onion.NodeIconAttribute));
        if (result == null)
        {
            var s = TryGetNodeAttrValue<Sprite>(dataObj, typeof(Onion.NodeIconAttribute));
            if (s) result = s.texture;
        }
        return result;
    }    
    class OnionAction
    {
        public EventDelegate action { get; private set; }
        public string actionName { get; private set; }
        public OnionAction(EventDelegate action, string actionName)
        {
            this.action = action;
            this.actionName = actionName;
        }
        public OnionAction(MethodInfo method, ScriptableObject target, string actionName)
        {
            action = CreateOpenDelegate(method, target);
            this.actionName = actionName;
        }
        public delegate void EventDelegate();
        private static EventDelegate CreateOpenDelegate(MethodInfo method, ScriptableObject target)
        {
            return (EventDelegate)Delegate.CreateDelegate(type: typeof(EventDelegate), target, method, true);
        }
    }
    static IEnumerable<OnionAction> GetNodeActions(ScriptableObject dataObj)
    {
        List<OnionAction> result = new List<OnionAction>();
        if (dataObj != null)
        {
            var type = dataObj.GetType();
            var methods = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(Onion.NodeActionAttribute)).ToList();
            foreach (var method in methods)
            {
                if (method.GetGenericArguments().Length == 0)    //只接受沒有參數的Method
                {
                    var attr = method.GetCustomAttribute<Onion.NodeActionAttribute>();
                    result.Add(new OnionAction(method, dataObj, attr.actionName ?? method.Name));
                }
            }
        }
        return result;
    }
    static OnionAction GetNodeOnSelectedAction(ScriptableObject dataObj)
    {
        if (dataObj != null)
        {
            var type = dataObj.GetType();
            var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(Onion.NodeOnSelectedAttribute)).SingleOrDefault(_ => _.GetGenericArguments().Length == 0);
            if(method != null)
                return new OnionAction(method, dataObj, method.Name);
        }
        return null;
    }

    //UI事件
    ScriptableObject selectObject;
    void OnTriggerItem(ScriptableObject dataObj)
    {
        selectObject = dataObj;

        DisplayInfo(dataObj);

        int selectionId = tree.treeView.GetSelection()[0];
        OnionAction onSelectedAction = tree.treeView.treeQuery[selectionId].onSelectedAction;

        if (onSelectedAction != null)
        {
            onSelectedAction.action.Invoke();
        }

    }
    void OnDoubleClick(ScriptableObject dataObj)
    {
        int selectionId = tree.treeView.GetSelection()[0];
        var selectionObject = tree.treeView.treeQuery[selectionId].dataObj;

        EditorGUIUtility.PingObject(selectionObject);   //Ping
    }


    //Tree
    class TreeRoot : TreeNode
    {
        public DataObjTreeView treeView;

        public TreeRoot(ScriptableObject _) : base(_)
        {
            dataObj = _;
            displayName = GetDisplayName(_);
        }

        public void SetTreeRoot()
        {
            depth = 0;
            CreatTree(this, -1);

            var window = GetWindow<OnionDataEditorWindow>();

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

            node.depth = depth;

            if (ReferenceCheck(node) == false)
            {
                GetWindow<OnionDataEditorWindow>().target = null;
                throw new System.StackOverflowException($"{node.displayName} is a parent of itself.");
            }


            //嘗試抓下一層
            var els = GetElements(node);
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

        IEnumerable<TreeNode> GetElements(TreeNode node)
        {
            if (node.dataObj != null)
            {
                var type = node.dataObj.GetType();
                var fields = type.GetFields(defaultBindingFlags).FilterWithAttribute(typeof(Onion.NodeElementAttribute)).ToList();

                if (fields.Count > 0)
                {
                    List<TreeNode> result = new List<TreeNode>(
                        fields[0]                                                   //只會抓第一個Attr就返回，之後再視需求擴充
                        .GetValue<IEnumerable<ScriptableObject>>(node.dataObj)
                        .Select(_ => new TreeNode(_))
                        );

                    return result;
                }
            }

            return null;
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
    class TreeNode
    {
        public ScriptableObject dataObj { get; protected set; }

        protected const int maxDepth = 16;
        public int depth;

        public string displayName;
        public Texture icon = null;
        public TreeNode parent = null;
        public List<TreeNode> nodes = new List<TreeNode>();
        public OnionAction onSelectedAction = null;

        public TreeNode(ScriptableObject _)
        {
            dataObj = _;
            displayName = GetDisplayName(_);
            icon = GetIcon(_);

            onSelectedAction = GetNodeOnSelectedAction(_);
        }

        protected string GetDisplayName(ScriptableObject _)
        {
            if (_ != null)
            {
                string nodeTitle = GetNodeTitle(_);
                return nodeTitle ?? dataObj.name;
            }
            else
            {
                return "NULL";
            }
        }
        protected Texture GetIcon(ScriptableObject _)
        {
            if (_ != null)
                return GetNodeIcon(_);
            else
                return EditorGUIUtility.FindTexture("console.erroricon.sml");
        }
    }



    //TreeView
    private class DataObjTreeView : TreeView
    {
        public Dictionary<int, TreeNode> treeQuery;
        TreeNode tree;
        List<TreeViewItem> result;

        public DataObjTreeView(TreeNode tree, TreeViewState state) : base(state)
        {
            rowHeight = 20F;
            isSelectChange = false;

            SetData(tree);
        }
        public void SetData(TreeNode tree)
        {
            treeQuery = new Dictionary<int, TreeNode>();

            this.tree = tree;
            id = 0;

            BuildRoot();
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = TreeNodeToTreeViewItem(id, -1, tree);
            id++;

            result = new List<TreeViewItem>();

            Build(0, tree, result);

            SetupParentsAndChildrenFromDepths(root, result);

            return root;
        }

        enum RowState { Normal, Error };
        GUIStyle GetStateGUIStyle(RowState state, TreeViewItem item)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.padding = new RectOffset(0, 0, 0, 3);
            switch (state)
            {
                //一般
                case RowState.Normal:
                    break;
                //警告
                case RowState.Error:
                    style.normal.textColor = new Color(0.85F, 0.18F, 0.18F);
                    break;
            }
            return style;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            TreeNode node = treeQuery[args.item.id];
            RowState state = (node.dataObj == null) ? RowState.Error : RowState.Normal;
            GUIStyle style = GetStateGUIStyle(state, args.item);

            bool hasIcon = node.icon != null;
            //icon
            if (hasIcon)
            {
                int padding = 1;
                float iconHeight = args.rowRect.height - padding * 2;

                Rect iconRect = args.rowRect;
                iconRect.x += GetContentIndent(args.item);
                iconRect.y += padding;
                iconRect.width = iconHeight + 10;
                iconRect.height = iconHeight;

                GUI.Label(iconRect, node.icon);
            }

            //text
            Rect labelRect = args.rowRect;
            labelRect.x += GetContentIndent(args.item) + (hasIcon ? (args.rowRect.height + 3F) : 0F);
            labelRect.width = args.rowRect.width;
            GUI.Label(labelRect, node.displayName, style);

            //child count
            if (treeQuery[args.item.id].nodes.Count > 0)
            {
                const float tagWidth = 50F;
                Rect rightRect = new Rect(args.rowRect.width - tagWidth, args.rowRect.y, tagWidth, args.rowRect.height);
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = new Color(0.5F, 0.5F, 0.5F);
                labelStyle.alignment = TextAnchor.MiddleRight;
                labelStyle.padding = new RectOffset(0, 8, 0, 3);
                GUI.Label(rightRect, $"[{treeQuery[args.item.id].nodes.Count}]", labelStyle);
            }

            //Line
            Rect lineRect = args.rowRect;
            lineRect.x += GetContentIndent(args.item);
            lineRect.y += lineRect.height - 1;
            lineRect.height = 1;
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.1F));      //線
        }
        public override IList<TreeViewItem> GetRows()
        {
            return result;
        }

        int id = 0;
        TreeViewItem Build(int _depth, TreeNode node, List<TreeViewItem> list)
        {
            TreeViewItem viewItem = TreeNodeToTreeViewItem(id, _depth, node);
            list.Add(viewItem);
            treeQuery.Add(id, node);

            id++;

            foreach (var item in node.nodes)
                Build(_depth + 1, item, list);

            return viewItem;
        }

        TreeViewItem TreeNodeToTreeViewItem(int _id, int _depth, TreeNode node)
        {
            return new TreeViewItem
            {
                id = _id,
                depth = _depth,
                displayName = node.displayName
            };
        }

        //Events
        bool isSelectChange = false;
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            if (isSelectChange)
            {
                isSelectChange = false;
                return;
            }

            GetWindow<OnionDataEditorWindow>().OnTriggerItem(treeQuery[id].dataObj);
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            GetWindow<OnionDataEditorWindow>().OnDoubleClick(treeQuery[id].dataObj);
        }
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
            {
                isSelectChange = true;
                GetWindow<OnionDataEditorWindow>().OnTriggerItem(treeQuery[selectedIds[0]].dataObj);
            }
        }
    }

}