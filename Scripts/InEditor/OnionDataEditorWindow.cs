#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.IMGUI.Controls;

using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionDataEditorWindow : EditorWindow
    {
        static string path => OnionDataEditor.path;

        Object _target;
        Object target
        {
            get => _target;
            set
            {
                OnTargetChange(value);

                _target = value;
            }
        }

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
            Recent = 4,
        }
        Tab currentTab = Tab.None;
        Object openedTarget;

        public TreeViewState treeViewState;

        TreeRoot treeRoot;
        IMGUIContainer treeViewContainer;


        [MenuItem("Window/Onion Data Editor &#E")]
        public static OnionDataEditorWindow ShowWindow()
        {
            var window = GetWindow<OnionDataEditorWindow>();

            ShowWindow(window.openedTarget);    //預設開null，使其打開bookmarkGroup；有target的話就開target

            return window;
        }
        public static OnionDataEditorWindow ShowWindow(Object data)
        {
            var window = GetWindow<OnionDataEditorWindow>();

            //沒有東西的話就指定bookmarkGroup
            if (data == null)
                data = OnionDataEditor.bookmarkGroup;

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
            //建構
            var root = this.rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "/Editor/Onion.uxml");
            TemplateContainer cloneTree = visualTree.CloneTree();
            cloneTree.style.flexGrow = 1;
            root.Add(cloneTree);


            //Tab

            //綁定btn-opened
            root.Q<Button>("btn-opened").clickable.clicked += ChangeTabToOpened;
            SetIcon(root.Q("btn-opened-icon"), "Edit");

            //綁定btn-bookmark
            root.Q<Button>("btn-bookmark").clickable.clicked += ChangeTabToBookmark;
            SetIcon(root.Q("btn-bookmark-icon"), "Bookmark_Fill");


            //綁定btn-setting
            root.Q<Button>("btn-setting").clickable.clicked += ChangeTabToSetting;
            SetIcon(root.Q("btn-setting-icon"), "Settings");



            //綁定btn-refresh
            root.Q<Button>("btn-refresh").clickable.clicked += OnFresh;
            SetIcon(root.Q("btn-refresh-icon"), "Refresh");

            //綁定btn-add-bookmark
            root.Q<Button>("btn-add-bookmark").clickable.clicked += OnToggleBookmark;
            //SetIcon(root.Q("btn-add-bookmark-icon"), "Heart");

            //綁定btn-info-document
            root.Q<Button>("btn-info-document").clickable.clicked += OpenDocument;
            SetIcon(root.Q("btn-info-document-icon"), "Help");

            //綁定btn-search-target
            root.Q<Button>("btn-search-target").clickable.clicked += OnSearchTarget;
            root.Q("btn-search-target").Add(new IMGUIContainer(OnSearchTargetListener));
            SetIcon(root.Q("btn-search-target-icon"), "Search");


            //建構treeview
            BuildTreeView();

            //Inspector
            BindInspector();

            //Split
            BindSpliter();


            //

            void BindSpliter()
            {
                //左右分割
                root.Q("Spliter").AddManipulator(new VisualElementResizer(
                    root.Q("ContainerA"), root.Q("ContainerB"), root.Q("Spliter"),
                    VisualElementResizer.Direction.Horizontal));

                //右側上下分割
                //root.Q("ContainerB").Q("Spliter").AddManipulator(new VisualElementResizer(
                //    root.Q("ContainerB").Q("inspector-scroll"), root.Q("ContainerB").Q("data-info"), root.Q("ContainerB").Q("Spliter"),
                //    VisualElementResizer.Direction.Vertical));

            }

            void BindInspector()
            {
                var inspectorContainer = new IMGUIContainer(() =>
                {
                    selectedNode?.onInspectorAction?.action?.Invoke();
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
                    if (treeRoot != null && target != null)
                    {
                        treeRoot.treeView.OnGUI(treeViewContainer.layout);
                    }
                }
            }

            //

            void ChangeTabToBookmark()
            {
                SetTarget((Object)OnionDataEditor.bookmarkGroup);
                SwitchTab(Tab.Bookmark);
            }

            void ChangeTabToSetting()
            {
                SetTarget((Object)OnionDataEditor.setting);
                SwitchTab(Tab.Setting);
            }

            void ChangeTabToOpened()
            {
                if (openedTarget != null)
                {
                    SetTarget(openedTarget);
                    SwitchTab(Tab.Opened);
                }
                else
                {
                    Debug.Log("Opened target is null.");
                }
            }

            //
            
            void OnFresh()
            {
                OnTargetChange(target);
            }

            void OnToggleBookmark()
            {
                int index = -1;
                if (BookmarkObjectValid(target))
                {
                    if (OnionDataEditor.bookmarkGroup.IsAddedInBookmark(target) == true)
                        OnRemoveBookmark();
                    else
                        OnAddBookmark();

                    FreshBookmarkView(target);
                }
                else
                {
                    Debug.Log("Can not add this object in bookmark.");
                }

                void OnRemoveBookmark()
                {
                    OnionDataEditor.bookmarkGroup.RemoveBookmark(target);
                }

                void OnAddBookmark()
                {
                    if (BookmarkObjectValid(target) == false)
                    {
                        Debug.Log("Can not add this object in bookmark.");
                        return;
                    }

                    OnionDataEditor.bookmarkGroup.AddBookmark(target);
                }

            }

            void OpenDocument()
            {
                OnionDocumentWindow.ShowWindow(OnionDocument.GetDocument(selectedNode.dataObj));
            }

            void OnSearchTarget()
            {
                int controlID = rootVisualElement.Q<Button>("btn-search-target").GetHashCode();
                EditorGUIUtility.ShowObjectPicker<Object>(null, false, "", controlID);
            }

            void OnSearchTargetListener()
            {
                //作為監聽器
                if (Event.current.commandName == "ObjectSelectorClosed" &&
                    EditorGUIUtility.GetObjectPickerObject() != null &&
                    EditorGUIUtility.GetObjectPickerControlID() == rootVisualElement.Q<Button>("btn-search-target").GetHashCode())
                {
                    Object selectObj = EditorGUIUtility.GetObjectPickerObject();

                    if (selectObj != null)
                        SetTarget(selectObj);
                }
            }

        }

        void SwitchTab(Tab tab)
        {
            currentTab = tab;

            Dictionary<Tab, string> elQuery = new Dictionary<Tab, string>
            {
                [Tab.Bookmark] = "btn-bookmark",
                [Tab.Opened] = "btn-opened",
                [Tab.Setting] = "btn-setting",
                [Tab.Recent] = "btn-recent",
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
            rootVisualElement.Q(elQuery[Tab.Opened]).style.display = (openedTarget != null) ? DisplayStyle.Flex : DisplayStyle.None;

        }

        void FreshBookmarkView(Object newTarget)
        {
            SetIcon(rootVisualElement.Q("btn-add-bookmark-icon"), "Bookmark");

            VisualElement viewElement = rootVisualElement.Q("btn-add-bookmark");

            bool isShow = BookmarkObjectValid(newTarget);

            viewElement.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;

            if (isShow)
            {
                if (OnionDataEditor.bookmarkGroup.IsAddedInBookmark(newTarget) == true)
                {
                    SetIcon(rootVisualElement.Q("btn-add-bookmark-icon"), "Bookmark_Fill");
                }
            }
        }

        bool BookmarkObjectValid(Object target)
        {
            bool result = IsSystemTarget(target) == false &&    //非系統Object
                ((target is GameObject && AssetDatabase.IsMainAsset(target)) || AssetDatabase.IsNativeAsset(target) == true);   //Prefab 或 ScriptableObject

            return result;
        }

        public void FreshTreeView()
        {
            if (treeRoot != null)
            {
                treeRoot.CreateTreeView();
                if (treeViewState == null)
                    treeViewState = treeRoot.treeView.state;
            }
        }

        public void SetTarget(IQueryableData newTarget)
        {
            SetTarget(newTarget as Object);

        }
        public void SetTarget(Object newTarget)
        {
            target = newTarget;
        }
               
        void OnTargetChange(Object newTarget)
        {
            VisualElement containerRoot = rootVisualElement.Q("tree-view-container");
            containerRoot.visible = (newTarget != null);


            if(newTarget == OnionDataEditor.bookmarkGroup)
            {
                SwitchTab(Tab.Bookmark);
            }
            else if(newTarget == OnionDataEditor.setting)
            {
                SwitchTab(Tab.Setting);
            }
            else
            {
                openedTarget = newTarget;
                SwitchTab(Tab.Opened);
            }


            if (newTarget != null)
            {
                treeRoot = new TreeRoot(newTarget);
                treeRoot.CreateTreeView();

                if(treeViewState == null)
                    treeViewState = treeRoot.treeView.state;

                //選擇Root
                selectedNode = treeRoot;

                //選擇Root並展開
                int rootId = treeRoot.treeView.GetRows()[0].id;
                treeRoot.treeView.SetSelection(new List<int> { rootId });
                treeRoot.treeView.SetExpanded(rootId, true);

                FreshBookmarkView(newTarget);

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


                if (newNode.onInspectorVisualElementRoot != null)
                {
                    inspectContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    parentVisualElement.Add(newNode.onInspectorVisualElementRoot);
                }
                else
                {
                    inspectContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }

            }

            DisplayInfo(newNode);

            var root = rootVisualElement;
            root.Q("btn-info-document").style.display = new StyleEnum<DisplayStyle>(newNode.dataObj == null ? DisplayStyle.None : DisplayStyle.Flex);
        }
        
        bool IsSystemTarget(Object compareTarget)
        {
            return 
                compareTarget == OnionDataEditor.bookmarkGroup ||
                compareTarget == OnionDataEditor.setting;
        }

        static void SetIcon(VisualElement el, string iconName)
        {
            el.style.backgroundImage = new StyleBackground(GetIconTexture(iconName));
        }
        public static Texture2D GetIconTexture(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{path}/Editor/Icons/{iconName}.png");
        }

        List<Button> actionBtns = new List<Button>();
        void DisplayInfo(TreeNode node)
        {
            rootVisualElement.Q("btn-info-document").style.display = (node.dataObj != null) ? DisplayStyle.Flex : DisplayStyle.None;

            if (node != null && node.nodeActions != null && node.nodeActions.Any() == false)
            {
                rootVisualElement.Q("data-info").style.display = DisplayStyle.None;
            }
            else
            {
                rootVisualElement.Q("data-info").style.display = DisplayStyle.Flex;
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

                if (node != null && string.IsNullOrEmpty(node.description) == true)
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
                if (node != null && node.nodeActions != null && node.nodeActions.Any() == false)
                {
                    container.style.display = DisplayStyle.None;
                }
                else
                {
                    container.style.display = DisplayStyle.Flex;
                }

                foreach (var actionBtn in actionBtns)
                    container.Remove(actionBtn);
                actionBtns = new List<Button>();


                if (n.nodeActions != null)
                {
                    foreach (var action in n.nodeActions)
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
            }

        }


        //UI事件
        public void OnTriggerItem(TreeNode node)
        {
            selectedNode = node;

            int selectionId = treeRoot.treeView.GetSelection()[0];
            OnionAction onSelectedAction = treeRoot.treeView.treeQuery[selectionId].onSelectedAction;
            if (onSelectedAction != null)
                onSelectedAction.action.Invoke();

        }
        public void OnDoubleClickItem(TreeNode node)
        {
            int selectionId = treeRoot.treeView.GetSelection()[0];

            OnionAction onDoubleClickAction = treeRoot.treeView.treeQuery[selectionId].onDoubleClickAction;
            if (onDoubleClickAction != null)
            {
                onDoubleClickAction.action.Invoke();
            }
            else
            {
                var selectionObject = treeRoot.treeView.treeQuery[selectionId].dataObj;
                EditorGUIUtility.PingObject(selectionObject);   //Ping
            }
        }

    }

}
#endif