
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System;
using UnityEditor.IMGUI.Controls;

using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionDataEditorWindow : EditorWindow
    {
        static string path => OnionDataEditor.Path;

        Object target => treeRoot?.Target;


        TreeNode targetNode;

        TreeNode _selectedNode;
        TreeNode selectedNode
        {
            get => _selectedNode;
            set
            {
                OnSelectedNodeChange(value);

                _selectedNode = value;
            }
        }

        enum Tab
        {
            None = 0,
            Opened = 1,
            Bookmark = 2,
            Setting = 3,
        }
        Tab currentTab = Tab.None;

        /// <summary>與Bookmark、Setting同階層，在選擇Bookmark、Setting時，此Node不會被改變。</summary>
        TreeNode openedNode;

        public TreeViewState treeViewState;
        DataObjTreeView treeView;
        IMGUIContainer treeViewContainer;

        /// <summary>目前顯示的Root Node，不論Bookmark、Setting，都會變動此Node。</summary>
        TreeNode treeRoot;


        [MenuItem("Window/Onion Data Editor &#E")]
        public static OnionDataEditorWindow ShowWindow()
        {
            var window = GetWindow<OnionDataEditorWindow>();

            ShowWindow(window.treeRoot);    //預設開null，使其打開bookmarkGroup；有target的話就開target

            return window;
        }

        public static OnionDataEditorWindow ShowWindow(TreeNode node)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            if (node?.Target == null)
                window.SetTarget(OnionDataEditor.Bookmarks);    //沒有東西的話就指定bookmarks
            else
                window.SetTarget(node);

            return window;
        }

        public static OnionDataEditorWindow ShowWindow(Object data)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            
            if (data == null)
                window.SetTarget(OnionDataEditor.Bookmarks);    //沒有東西的話就指定bookmarks
            else
                window.SetTarget(data);

            return window;
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("Onion Data Editor");
            Init();
        }

        void Init()
        {
            //Create
            var root = this.rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{path}/Editor/EditorWindow/OnionDataEditorWindow/Onion.uxml");
            TemplateContainer cloneTree = visualTree.CloneTree();
            cloneTree.style.flexGrow = 1;
            root.Add(cloneTree);


            //Tab

            SetIcon(root.Q("oniondataeditor-icon"), "OnionDataEditorIcon");

            //綁定btn-opened
            root.Q<Button>("btn-opened").clickable.clicked += ChangeTabToOpened;
            SetIcon(root.Q("btn-opened-icon"), "Compass");

            //Bind btn-bookmark
            root.Q<Button>("btn-bookmark").clickable.clicked += ChangeTabToBookmark;
            SetIcon(root.Q("btn-bookmark-icon"), "Bookmark_Fill");


            //Bind btn-setting
            root.Q<Button>("btn-setting").clickable.clicked += ChangeTabToSetting;
            SetIcon(root.Q("btn-setting-icon"), "Settings");



            //Bind btn-refresh
            root.Q<Button>("btn-refresh").clickable.clicked += OnFresh;
            SetIcon(root.Q("btn-refresh-icon"), "Refresh");

            //Bind btn-add-bookmark
            root.Q<Button>("btn-add-bookmark").clickable.clicked += OnToggleBookmark;

            //Bind btn-toggle-inspector
            root.Q<Button>("btn-toggle-inspector").clickable.clicked += ()=>
            {
                SetInspectorActive(true);
            };
            SetIcon(root.Q("btn-toggle-inspector-icon"), "Arrow_Right");



            //建構treeview
            BuildTreeView();

            //Inspector
            BindInspector();

            //Split
            BindSpliter();


            SetInspectorActive(true);

            //

            void BindSpliter()
            {
                var manipulator = new VisualElementResizer(
                    root.Q("ContainerA"),
                    root.Q("ContainerB"),
                    root.Q("Spliter"),
                    VisualElementResizer.Direction.Horizontal)
                {
                    onMouseDoubleClick = () => { SetInspectorActive(false); }
                };

                //左右分割
                root.Q("Spliter").AddManipulator(manipulator);

                //右側上下分割
                //root.Q("ContainerB").Q("Spliter").AddManipulator(new VisualElementResizer(
                //    root.Q("ContainerB").Q("inspector-scroll"), root.Q("ContainerB").Q("data-info"), root.Q("ContainerB").Q("Spliter"),
                //    VisualElementResizer.Direction.Vertical));

            }

            void BindInspector()
            {
                var inspectorContainer = new IMGUIContainer(() =>
                {
                    selectedNode?.OnInspectorGUI?.Invoke();
                });
                inspectorContainer.AddToClassList("inspect-container");
                inspectorContainer.name = "inspect-container-imgui";

                var inspectorContainerVisualElement = new VisualElement();
                inspectorContainerVisualElement.AddToClassList("inspect-container");
                inspectorContainerVisualElement.name = "inspect-container-ve";

                var inspectorRoot = root.Q("inspector-scroll").Q("unity-content-container");
                inspectorRoot.Add(inspectorContainer);
                inspectorRoot.Add(inspectorContainerVisualElement);

            }

            void BuildTreeView()
            {
                VisualElement containerRoot = root.Q("tree-view-container");
                if (treeViewContainer == null)
                {
                    treeViewContainer = new IMGUIContainer(DrawTreeView)
                    {
                        name = "tree-view",
                        style = { flexGrow = 1 },
                    };
                    containerRoot.Add(treeViewContainer);
                }

                void DrawTreeView()
                {
                    if (treeRoot != null)
                    {
                        treeView.OnGUI(treeViewContainer.layout);
                    }
                }
            }

            //

            void ChangeTabToBookmark()
            {
                SetTarget(OnionDataEditor.Bookmarks);
                SwitchTab(Tab.Bookmark);
            }

            void ChangeTabToSetting()
            {
                SetTarget(OnionDataEditor.Setting);
                SwitchTab(Tab.Setting);
            }

            void ChangeTabToOpened()
            {
                if (openedNode != null)
                {
                    SetTarget(openedNode);
                    SwitchTab(Tab.Opened);
                }
                else
                {
                    ChangeTabToBookmark();
                }
            }

            //
            

            void OnToggleBookmark()
            {
                if (CanNodeAddToBookmarks(treeRoot))
                {
                    if (OnionDataEditor.Setting.IsAddedInBookmark(target) == true)
                    {
                        OnionDataEditor.Setting.RemoveBookmark(target);
                    }
                    else
                    {
                        OnionDataEditor.Setting.AddBookmark(target);
                    }

                    FreshBookmarkView(treeRoot);
                }
                else
                {
                    Debug.Log("Can not add this object in bookmark.");
                }

            }

            //void OnSearchTarget()
            //{
            //    int controlID = rootVisualElement.Q<Button>("btn-search-target").GetHashCode();
            //    EditorGUIUtility.ShowObjectPicker<Object>(null, false, "", controlID);
            //}

            //void OnSearchTargetListener()
            //{
            //    //作為監聽器
            //    if (Event.current.commandName == "ObjectSelectorClosed" &&
            //        EditorGUIUtility.GetObjectPickerObject() != null &&
            //        EditorGUIUtility.GetObjectPickerControlID() == rootVisualElement.Q<Button>("btn-search-target").GetHashCode())
            //    {
            //        Object selectObj = EditorGUIUtility.GetObjectPickerObject();

            //        if (selectObj != null)
            //            SetTarget(selectObj);
            //    }
            //}

        }

        void OnFresh()
        {
            if (treeRoot == null)
            {
                SetTarget(OnionDataEditor.Bookmarks);
                return;
            }

            if (treeRoot.OnRebuild == null)
            {
                treeRoot.InitSetting();
            }
            else
            {
                treeRoot.OnRebuild.Invoke();
            }

            OnTargetChange(treeRoot);
        }

        void SwitchTab(Tab tab)
        {
            currentTab = tab;

            Dictionary<Tab, string> elQuery = new Dictionary<Tab, string>
            {
                [Tab.Bookmark] = "btn-bookmark",
                [Tab.Opened] = "btn-opened",
                [Tab.Setting] = "btn-setting",
            };

            const string className = "active-tab";
            foreach (var p in elQuery)
            {
                if (p.Key != tab)
                    rootVisualElement.Q(p.Value)?.RemoveFromClassList(className);
                else
                    rootVisualElement.Q(p.Value)?.AddToClassList(className);
            }


            //Opened 依照openedTarget來顯示
            rootVisualElement.Q(elQuery[Tab.Opened]).style.display = (openedNode != null) ? DisplayStyle.Flex : DisplayStyle.None;

        }

        void FreshBookmarkView(TreeNode node)
        {
            bool isShow = CanNodeAddToBookmarks(node);

            VisualElement viewElement = rootVisualElement.Q("btn-add-bookmark");
            viewElement.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;

            if (isShow)
            {
                string iconName = (OnionDataEditor.Setting.IsAddedInBookmark(node.Target) == true) ? "Bookmark_Fill" : "Bookmark";
                SetIcon(rootVisualElement.Q("btn-add-bookmark-icon"), iconName);
            }
        }

        bool CanNodeAddToBookmarks(TreeNode node)
        {
            if (node.IsNull)
                return false;

            if (node.IsPseudo)
                return false;

            if (IsSystemTarget(node))
                return false;

            if (target == null)
                return false;

            //---

            if (AssetDatabase.IsNativeAsset(target) == true)                        //Asset(without prefab, sence...)
                return true;

            if (AssetDatabase.IsForeignAsset(target) == true)                       //Asset
                return true;

            if (target is GameObject && AssetDatabase.IsMainAsset(target) == true)  //Prefab
                return true;

            return false;
        }
                       
        void OnTargetChange(TreeNode newNode)
        {
            VisualElement containerRoot = rootVisualElement.Q("tree-view-container");
            containerRoot.visible = (newNode != null);

            if (newNode != null)
            {
                treeRoot = newNode;


                if (treeRoot != null)
                {
                    treeRoot.CreateTreeView(out treeView);


                    if (treeViewState == null)
                        treeViewState = treeView.state;
                }

                //選擇Root
                selectedNode = treeRoot;

                //選擇Root並展開
                int rootId = treeView.GetRows()[0].id;
                treeView.SetSelection(new List<int> { rootId });
                treeView.SetExpanded(rootId, true);

                FreshBookmarkView(treeRoot);
                
                if (OnionDataEditor.IsBookmark(newNode))
                {
                    SwitchTab(Tab.Bookmark);
                }
                else if (OnionDataEditor.IsSetting(newNode))
                {
                    SwitchTab(Tab.Setting);
                }
                else
                {
                    openedNode = treeRoot;
                    SwitchTab(Tab.Opened);
                }
            }
        }

        void OnSelectedNodeChange(TreeNode newNode)
        {
            if (newNode != null)
            {
                var parentVisualElement = rootVisualElement.Q("inspect-container-ve");
                var inspectContainer = rootVisualElement.Q("inspect-container-imgui");

                //清掉原本的visual element node
                foreach (var child in parentVisualElement.Children().ToArray())
                {
                    parentVisualElement.Remove(child);
                }


                if (newNode.OnInspectorVisualElementRoot != null)
                {
                    inspectContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    parentVisualElement.Add(newNode.OnInspectorVisualElementRoot);
                }
                else
                {
                    inspectContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }

            }

            DisplayInfo(newNode);
        }
        
        static bool IsSystemTarget(TreeNode node)
        {
            return
                OnionDataEditor.IsBookmark(node) ||
                OnionDataEditor.IsSetting(node);
        }

        static void SetIcon(VisualElement el, string iconName)
        {
            el.style.backgroundImage = new StyleBackground(OnionDataEditor.GetIconTexture(iconName));
        }
        

        List<Button> actionBtns = new List<Button>();
        void DisplayInfo(TreeNode node)
        {
            if (node != null && node.NodeActions != null && node.NodeActions.Any())
            {
                rootVisualElement.Q("data-info").style.display = DisplayStyle.Flex;
            }
            else
            {
                rootVisualElement.Q("data-info").style.display = DisplayStyle.None;
            }


            DisplayTextInfo(node);
            DisplayActionButtonInfo(node);

            void DisplayTextInfo(TreeNode n)
            {
                var root = this.rootVisualElement;

                //text info
                string nodeTitle = n.displayName;
                string nodeDescription = n.description;

                root.Q<Label>("info-title").text = nodeTitle;
                root.Q<Label>("info-description").text = nodeDescription;

                if (n != null && string.IsNullOrEmpty(n.description) == true)
                {
                    root.Q<Label>("info-description").style.display = DisplayStyle.None;
                }
                else
                {
                    root.Q<Label>("info-description").style.display = DisplayStyle.Flex;
                }

            }

            void DisplayActionButtonInfo(TreeNode n)
            {
                var root = this.rootVisualElement;

                var container = root.Q("data-info-btn-list");

                foreach (var actionBtn in actionBtns)
                    container.Remove(actionBtn);

                actionBtns = new List<Button>();

                if (n.NodeActions != null)
                {
                    foreach (var action in n.NodeActions)
                    {
                        Button actionBtn = new Button();

                        actionBtn.clickable.clicked += () => action.action();
                        actionBtn.AddToClassList("onion-btn");
                        actionBtn.AddToClassList("pointer");

                        if (action.actionIcon != null)
                        {
                            actionBtn.Add(new Image() { image = action.actionIcon });
                        }
                        actionBtn.Add(new Label() { text = action.actionName });



                        actionBtns.Add(actionBtn);

                        container.Add(actionBtn);
                    }
                }
            }

        }


        float originInspectorWidth = 0;
        bool IsInspectorActive = true;
        void SetInspectorActive(bool active)
        {
            if (IsInspectorActive == active)
                return;

            IsInspectorActive = active;

            var window = GetWindow<OnionDataEditorWindow>();
            var root = window.rootVisualElement;

            var containerA = root.Q("ContainerA");
            var containerB = root.Q("ContainerB");
            var spliter = root.Q("Spliter");
            var btnToggleInspector = root.Q("btn-toggle-inspector");

            if (active)
            {
                var winRect = window.position;
                winRect.width += originInspectorWidth;
                window.position = winRect;

                containerA.style.flexGrow = new StyleFloat(0F);
                containerA.style.maxWidth = new StyleLength(500);
                containerB.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                spliter.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                btnToggleInspector.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                var winRect = window.position;
                originInspectorWidth = containerB.layout.width;

                winRect.width -= originInspectorWidth;
                window.position = winRect;

                containerA.style.flexGrow = new StyleFloat(1F);
                containerA.style.maxWidth = new StyleLength(StyleKeyword.Auto);
                containerB.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                spliter.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                btnToggleInspector.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
        }

        // UI

        public void OnTriggerItem(TreeNode node)
        {
            selectedNode = node;
            node.OnSelected?.Invoke();
        }
        public void OnDoubleClickItem(TreeNode node)
        {
            Action onDoubleClickAction = node.OnDoubleClick;
            if (onDoubleClickAction != null)
            {
                onDoubleClickAction.Invoke();
            }
            else if(node.IsPseudo == false)
            {
                var selectionObject = node.Target;
                EditorGUIUtility.PingObject(selectionObject);   //Ping
            }
        }


        // Public Methods

        public void SetTarget(Object newTarget)
        {
            targetNode = new TreeNode(newTarget);
            OnTargetChange(targetNode);
        }
        public void SetTarget(TreeNode newTarget)
        {
            targetNode = newTarget;
            OnTargetChange(targetNode);
        }

        

        public static void RebuildNode()
        {
            var window = GetWindow<OnionDataEditorWindow>();
            window.OnFresh();
        }

        public static void UpdateTreeView()
        {
            var window = GetWindow<OnionDataEditorWindow>();

            if (window.treeRoot != null)
            {
                if (window.treeViewState == null)
                    window.treeView = new DataObjTreeView(window.treeRoot, new TreeViewState());
                else
                    window.treeView = new DataObjTreeView(window.treeRoot, window.treeView.state);
            }
        }

        public static void SetSelectionAt(TreeNode node)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            window.selectedNode = node;
            window.treeView.SelectAt(node);
        }

        public static void ChangeTargetTo(Object newTarget)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            window.SetTarget(newTarget);
        }


    }

}