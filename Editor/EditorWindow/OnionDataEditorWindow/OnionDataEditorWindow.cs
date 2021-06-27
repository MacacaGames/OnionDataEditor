﻿
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
        Object Target => treeRoot?.Target;

        TreeNode _SelectedNode;
        TreeNode SelectedNode
        {
            get => _SelectedNode;
            set
            {
                OnSelectedNodeChange(value);

                _SelectedNode = value;
            }
        }


        internal DataObjTreeView treeView;
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

        internal static OnionDataEditorWindow ShowWindow(TreeNode node)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            Tab tab = window.CreateNewEmptyTab();
            window.ChangeTabTo(tab);
            window.ReplaceTarget(node);

            return window;
        }


        public void OnEnable()
        {
            titleContent = new GUIContent("Onion Data Editor", OnionDataEditor.GetIconTexture("Node_Element"));
            Init();

            OpenTarget(treeRoot ?? OnionDataEditor.Setting.OnboardingNode);
        }


        void Init()
        {
            //Create
            var root = this.rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{OnionDataEditor.Path}/Editor/EditorWindow/OnionDataEditorWindow/Onion.uxml");
            TemplateContainer cloneTree = visualTree.CloneTree();
            cloneTree
                .SetFlexGrow(1)
                .AddTo(root);

            //Tab

            SetIcon(root.Q("oniondataeditor-icon"), "OnionDataEditorIcon");


            root.Q<Button>("btn-add-tab").clicked += () =>
            {
                var tab = CreateNewEmptyTab();
                ChangeTabTo(tab);
            };
            SetIcon(root.Q("btn-add-tab-icon"), "Add");


            //Bind btn-bookmark
            root.Q<Button>("btn-bookmark").clicked += ChangeTabToBookmark;
            SetIcon(root.Q("btn-bookmark-icon"), "Bookmark_Fill");

            //Bind btn-setting
            root.Q<Button>("btn-setting").clicked += ChangeTabToSetting;
            SetIcon(root.Q("btn-setting-icon"), "Settings");


            //Bind btn-back-histroy
            root.Q<Button>("btn-back-histroy").clicked += () => viewHistroy.Back();
            SetIcon(root.Q("btn-back-histroy-icon"), "Arrow_Left");

            //Bind btn-refresh
            root.Q<Button>("btn-refresh").clicked += OnRebuildNode;
            SetIcon(root.Q("btn-refresh-icon"), "Refresh");

            //Bind btn-add-bookmark
            root.Q<Button>("btn-add-bookmark").clicked += OnToggleBookmark;

            //Bind btn-toggle-inspector
            root.Q<Button>("btn-toggle-inspector").clicked += ()=>
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

            BuildTab();


            SetInspectorActive(true);

            bool isFullWidth = OnionDataEditor.Setting.isFullWidth;
            SetInspectorWidthFull(isFullWidth);


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
                    SelectedNode?.OnInspectorGUI?.Invoke();
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
                        Rect rect = treeViewContainer.layout;
                        DropAreaGUI(rect);
                        treeView.OnGUI(rect);
                    }
                }

                void DropAreaGUI(Rect drop_area)
                {
                    Event evt = Event.current;

                    switch (evt.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:

                            if (!drop_area.Contains(evt.mousePosition))
                            {
                                return;
                            }

                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                Object target = DragAndDrop.objectReferences[0];
                                TreeNode targetNode = new TreeNode(target);

                                OpenTarget(targetNode);

                            }
                            break;
                    }
                }
            }

            void BuildTab()
            {
                var tab = CreateNewEmptyTab();

                ChangeTabTo(tab);
            }

            //

            void ChangeTabToSetting()
            {
                var targetTab = tabs.FirstOrDefault(n => OnionDataEditor.IsSetting(n.node));
                if (targetTab != null)
                {
                    ChangeTabTo(targetTab);
                }
                else
                {
                    targetTab = CreateNewTab(new Tab
                    {
                        viewHistroy = new ViewHistroy(this, new TreeNode(OnionDataEditor.Setting))
                        {
                            OnHistroyChange = OnHistroyChange,
                        },
                    });
                    ChangeTabTo(targetTab);
                }
            }

            void ChangeTabToBookmark()
            {
                var targetTab = tabs.FirstOrDefault(n => OnionDataEditor.IsBookmark(n.node));
                if (targetTab != null)
                {
                    ChangeTabTo(targetTab);
                }
                else
                {
                    targetTab = CreateNewTab(new Tab
                    {
                        viewHistroy = new ViewHistroy(this, OnionDataEditor.Bookmarks)
                        {
                            OnHistroyChange = OnHistroyChange,
                        },
                    });
                    ChangeTabTo(targetTab);
                }
            }

            //
            

            void OnToggleBookmark()
            {
                if (CanNodeAddToBookmarks(treeRoot))
                {
                    if (OnionDataEditor.Setting.IsAddedInBookmark(Target) == true)
                    {
                        OnionDataEditor.Setting.RemoveBookmark(Target);
                    }
                    else
                    {
                        OnionDataEditor.Setting.AddBookmark(Target);
                    }

                    FreshBookmarkView(treeRoot);
                }
                else
                {
                    Debug.Log("Can not add this object in bookmark.");
                }

            }

        }

        void OnRebuildNode()
        {
            if (treeRoot == null)
            {
                OpenTarget(OnionDataEditor.Setting.OnboardingNode);
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

            if (Target == null)
                return false;

            //---

            if (AssetDatabase.IsNativeAsset(Target) == true)                        //Asset(without prefab, sence...)
                return true;

            if (AssetDatabase.IsForeignAsset(Target) == true)                       //Asset
                return true;

            if (Target is GameObject && AssetDatabase.IsMainAsset(Target) == true)  //Prefab
                return true;

            return false;
        }

        internal void OnTargetChange(TreeNode newNode)
        {
            VisualElement containerRoot = rootVisualElement.Q("tree-view-container");
            containerRoot.visible = (newNode != null);

            if (newNode != null)
            {
                treeRoot = newNode;

                if (treeRoot != null)
                {
                    treeRoot.CreateTreeView(out treeView);
                }

                treeView.SetState(viewHistroy.Current.treeViewState);

                SelectedNode = treeView.GetSelectedNode();

                FreshBookmarkView(treeRoot);

                UpdateAllTabView();
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

            UpdateTitleInfo(newNode);

            UpdateActionButtonInfo(newNode);
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
        



        // Inspector

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

                containerA.SetFlexGrow(0);
                containerA.style.maxWidth = new StyleLength(500);
            }
            else
            {
                var winRect = window.position;
                originInspectorWidth = containerB.layout.width;

                winRect.width -= originInspectorWidth;
                window.position = winRect;

                containerA.SetFlexGrow(1F);
                containerA.style.maxWidth = new StyleLength(StyleKeyword.Auto);
            }

            containerB.ShowIf(active);
            spliter.ShowIf(active);
            btnToggleInspector.HideIf(active);

        }

        internal static void SetInspectorWidthFull(bool isFullWidth)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            var containerB = window.rootVisualElement.Q("ContainerB");
            containerB.style.alignItems = isFullWidth ? new StyleEnum<Align>(Align.Stretch) : new StyleEnum<Align>(Align.Center);

            window.rootVisualElement.Q("InspectorContainer").style.alignItems = new StyleEnum<Align>(Align.Center);
            window.rootVisualElement.Q("inspector-scroll").style.alignItems = new StyleEnum<Align>(Align.Center);


            var inspectoreHeader = window.rootVisualElement.Q("InspectorHeader");
            inspectoreHeader.style.width = new Length(100, LengthUnit.Percent);
            inspectoreHeader.style.maxWidth = isFullWidth ? new StyleLength(StyleKeyword.Auto) : new StyleLength(500F);

            var inspectoreViewport = window.rootVisualElement.Q("unity-content-viewport");
            inspectoreViewport.style.width = new Length(100, LengthUnit.Percent);
            inspectoreViewport.style.maxWidth = isFullWidth ? new StyleLength(StyleKeyword.Auto) : new StyleLength(500F);


        }

        void UpdateTitleInfo(TreeNode n)
        {
            var root = this.rootVisualElement;

            //text info
            string nodeTitle = n.displayName;
            string nodeDescription = n.description;

            root.Q<Label>("info-title").text = nodeTitle;

            if (n != null && string.IsNullOrEmpty(n.description) == true)
            {
                root.Q<Label>("info-description").style.display = DisplayStyle.None;
            }
            else
            {
                root.Q<Label>("info-description").text = nodeDescription;
                root.Q<Label>("info-description").style.display = DisplayStyle.Flex;
            }

            if (n.icon == null)
            {
                root.Q("info-icon").style.display = DisplayStyle.None;
            }
            else
            {
                root.Q("info-icon").style.backgroundImage = new StyleBackground((Texture2D)n.icon);
                root.Q("info-icon").style.display = DisplayStyle.Flex;
            }

        }

        List<Button> actionBtns = new List<Button>();
        void UpdateActionButtonInfo(TreeNode n)
        {
            var root = this.rootVisualElement;

            if (n == null || n.NodeActions == null || n.NodeActions.Any() == false)
            {
                root.Q("data-info").style.display = DisplayStyle.None;
                return;
            }

            root.Q("data-info").style.display = DisplayStyle.Flex;

            var container = root.Q("data-info-btn-list");

            foreach (var actionBtn in actionBtns)
            {
                container.Remove(actionBtn);
            }

            actionBtns = new List<Button>();

            foreach (var action in n.NodeActions)
            {
                Button actionBtn = new Button()
                    .AddClass("onion-btn")
                    .AddClass("pointer");

                actionBtn.clicked += () => action.action();

                if (action.actionIcon != null)
                {
                    actionBtn.Add(new Image() { image = action.actionIcon });
                }
                actionBtn.Add(new Label() { text = action.actionName });



                actionBtns.Add(actionBtn);

                container.Add(actionBtn);
            }
        }



        // UI

        public void OnTriggerItem(TreeNode node)
        {
            SelectedNode = node;
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





        //Tab

        internal class Tab
        {
            public int index;
            public ViewHistroy viewHistroy;
            public TreeNode node;
            
            public VisualElement ve;
            public Image iconVe;
        }

        readonly List<Tab> tabs = new List<Tab>();
        int currentTabIndex = 0;
        Tab CurrentTab => tabs[currentTabIndex];

        Tab CreateNewTab(Tab tab)
        {
            int index = tabs.Any() ? tabs.Max(n => n.index) + 1 : 0;
            tab.index = index;

            var container = rootVisualElement.Q("TabListContainer");
            Button ve = new Button()
                .AddTo(container)
                .AddClass("btn-tab")
                .AddClass("pointer");

            ve.RegisterCallback<MouseDownEvent>(e =>
            {
                //確保Tab分頁在1個以上時才能關閉
                if (e.button == 1 && tabs.Count > 1)
                {
                    var menu = new OnionMenu();
                    menu.AddItem("Close Tab", () => { CloseTab(tab); }, OnionDataEditor.GetIconTexture("Trash"));

                    menu.Show();
                }
            });

            ve.clicked += () =>
            {
                ChangeTabTo(tab);
                UpdateAllTabView();
            };

            Image icon = new Image()
                .AddTo(ve)
                .AddClass("btn-icon");


            tab.ve = ve;
            tab.iconVe = icon;


            tabs.Add(tab);

            return tab;
        }

        Tab CreateNewEmptyTab()
        {
            Tab tab = new Tab
            {
                ve = null,
                viewHistroy = new ViewHistroy(this, OnionDataEditor.Setting.OnboardingNode)
                {
                    OnHistroyChange = OnHistroyChange,
                },
            };

            return CreateNewTab(tab);
        }

        void CloseTab(Tab tab)
        {
            if(currentTabIndex == tab.index)
            {
                currentTabIndex = 0;
            }

            var container = rootVisualElement.Q("TabListContainer");
            container.Remove(tab.ve);
            tabs.Remove(tab);

            for(int i = 0; i < tabs.Count; i++)
            {
                tabs[i].index = i;
            }

            if (currentTabIndex >= tabs.Count)
            {
                currentTabIndex = tabs.Count - 1;
            }

            UpdateAllTabView();
        }

        void ChangeTabTo(Tab tab)
        {
            viewHistroy?.SaveCurrentState();

            currentTabIndex = tab.index;
            viewHistroy = tab.viewHistroy;
            viewHistroy.ReplaceSelf();

            UpdateAllTabView();
        }

        void UpdateAllTabView()
        {
            const string className = "active-tab";

            foreach(var t in tabs)
            {
                if (t.index == currentTabIndex)
                {
                    t.ve.AddToClassList(className);
                }
                else
                {
                    t.ve.RemoveFromClassList(className);
                }
            }
        }

        public void UpdateCurrentTab(TreeNode currentNode)
        {
            var tab = CurrentTab;
            tab.node = currentNode;

            tab.ve.tooltip = currentNode.displayName;
            tab.iconVe.image = currentNode.icon == null ? OnionDataEditor.GetIconTexture("Compass") : currentNode.icon;
        }





        //ViewHistroy

        ViewHistroy viewHistroy;

        void OnHistroyChange(TreeNode node)
        {
            if (viewHistroy.Count >= 2)
            {
                rootVisualElement.Q("btn-back-histroy").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

                rootVisualElement.Q("btn-back-histroy").tooltip = viewHistroy.Last.DisplayName;
            }
            else
            {
                rootVisualElement.Q("btn-back-histroy").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }


            UpdateCurrentTab(node);
        }

        // ## NOTE ##
        // Open、Push、Replace 和 Public Methods 的三個方法是對稱的，
        // 一個是 TreeNode 一個是 Object，都是操作 viewHistroy

        internal void OpenTarget(TreeNode newTarget)
        {
            viewHistroy.Clear();
            viewHistroy.PushState(newTarget);
        }

        void PushTarget(TreeNode newTarget)
        {
            viewHistroy.PushState(newTarget);
        }

        void ReplaceTarget(TreeNode newTarget)
        {
            viewHistroy.ReplaceState(newTarget);
        }



        // Internal Methods

        internal static bool IsTarget(Object target)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            TreeNode node = window.CurrentTab.node;
            
            if (node.IsPseudo || node.IsNull)
                return false;

            return node.Target == target;
        }


        // Public Methods

        public static void RebuildNode()
        {
            var window = GetWindow<OnionDataEditorWindow>();
            window.OnRebuildNode();
        }

        public static void UpdateTreeView()
        {
            var window = GetWindow<OnionDataEditorWindow>();

            if (window.treeRoot != null)
            {
                window.treeView = new DataObjTreeView(window.treeRoot, new TreeViewState());
            }
        }

        public static void SetSelectionAt(TreeNode node)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            window.SelectedNode = node;
            window.treeView.SelectAt(node);
        }

        public static void ChangeTargetTo(Object newTarget)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            var targetNode = new TreeNode(newTarget);
            window.OpenTarget(targetNode);
        }


        public static void Open(Object newTarget)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            if (newTarget == null)
                window.OpenTarget(OnionDataEditor.Setting.OnboardingNode);
            else
                window.OpenTarget(new TreeNode(newTarget));
        }

        public static void Push(Object newTarget)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            TreeNode targetNode = new TreeNode(newTarget);
            window.viewHistroy.PushState(targetNode);
        }

        public static void Replace(Object newTarget)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            TreeNode targetNode = new TreeNode(newTarget);
            window.viewHistroy.ReplaceState(targetNode);
        }



    }

}