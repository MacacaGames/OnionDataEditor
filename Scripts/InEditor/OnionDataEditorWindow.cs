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

        const string path = "Assets/OnionDataEditor";



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
            None,
            Opened,
            Bookmark,
            Recent,
        }
        Tab currentTab = Tab.None;
        Object openedTarget;

        public TreeViewState treeViewState;

        TreeRoot treeRoot;
        IMGUIContainer treeViewContainer;


        [MenuItem("Window/Onion Data Editor &#E")]
        public static OnionDataEditorWindow ShowWindow()
        {
            //預設開null，使其打開bookmarkGroup
            return ShowWindow(null);
        }
        public static OnionDataEditorWindow ShowWindow(IQueryableData data)
        {
            var window = GetWindow<OnionDataEditorWindow>();
            window.SetTarget(data ?? window.bookmarkGroup);     //沒有東西的話就指定bookmarkGroup

            return window;
        }

        public void OnEnable()
        {
            titleContent = new GUIContent("OnionDataEditor");
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

            CreateNeededAsset();

            //Tab
            //綁定btn-bookmark
            root.Q<Button>("btn-bookmark").clickable.clicked += ChangeTabToBookmark;
            SetIcon(root.Q("btn-bookmark-icon"), "Heart_Fill");

            //綁定btn-opened
            root.Q<Button>("btn-opened").clickable.clicked += ChangeTabToOpened;
            SetIcon(root.Q("btn-opened-icon"), "Edit");



            //綁定btn-refresh
            root.Q<Button>("btn-refresh").clickable.clicked += OnFresh;
            SetIcon(root.Q("btn-refresh-icon"), "Refresh");

            //綁定btn-add-bookmark
            root.Q<Button>("btn-add-bookmark").clickable.clicked += OnToggleBookmark;
            SetIcon(root.Q("btn-add-bookmark-icon"), "Heart");

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
        }

        void SwitchTab(Tab tab)
        {
            currentTab = tab;

            Dictionary<Tab, string> elQuery = new Dictionary<Tab, string>
            {
                [Tab.Bookmark] = "btn-bookmark",
                [Tab.Opened] = "btn-opened",
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

            switch (tab)
            {
                case Tab.Opened:
                    break;

                case Tab.Bookmark:
                    break;

                case Tab.Recent:
                    break;
            }
        }

        void ChangeTabToBookmark()
        {
            SetTarget((Object)bookmarkGroup);
            SwitchTab(Tab.Bookmark);
        }

        void ChangeTabToOpened()
        {
            if(openedTarget != null)
            {
                SetTarget(openedTarget);
                SwitchTab(Tab.Opened);
            }
            else
            {
                Debug.Log("Opened target is null.");
            }
        }

        void FreshBookmarkView(Object newTarget)
        {
            const string activeClassName = "active-bookmark";
            const string deactiveClassName = "btn-bookmark";
            SetIcon(rootVisualElement.Q("btn-add-bookmark-icon"), "Heart");

            VisualElement viewElement = rootVisualElement.Q("btn-add-bookmark");

            viewElement.style.display = (newTarget != bookmarkGroup) ? DisplayStyle.Flex : DisplayStyle.None;

            if (newTarget != bookmarkGroup)
            {
                viewElement.RemoveFromClassList(activeClassName);
                viewElement.AddToClassList(deactiveClassName);

                if (bookmarkGroup != null)
                {
                    int index = bookmarkGroup.OfType<OnionBookmark>().Select(_ => _.target).ToList().IndexOf(newTarget);
                    if (index >= 0)
                    {
                        viewElement.RemoveFromClassList(deactiveClassName);
                        viewElement.AddToClassList(activeClassName);
                        SetIcon(rootVisualElement.Q("btn-add-bookmark-icon"), "Heart_Fill");
                    }
                }
            }
        }

        //Bind & Build
        void BindSpliter()
        {
            var root = rootVisualElement;

            //左右分割
            root.Q("Spliter").AddManipulator(new VisualElementResizer(
                root.Q("ContainerA"), root.Q("ContainerB"), root.Q("Spliter"),
                VisualElementResizer.Direction.Horizontal));

            //右側上下分割
            root.Q("ContainerB").Q("Spliter").AddManipulator(new VisualElementResizer(
                root.Q("ContainerB").Q("inspector-scroll"), root.Q("ContainerB").Q("data-info"), root.Q("ContainerB").Q("Spliter"),
                VisualElementResizer.Direction.Vertical));

        }
        void BindInspector()
        {
            var root = rootVisualElement;

            var inspectorContainer = new IMGUIContainer(DrawInspector);
            inspectorContainer.AddToClassList("inspect-container");
            root.Q("inspector-scroll").Q("unity-content-container").Add(inspectorContainer);

        }
        void BuildTreeView()
        {
            var root = rootVisualElement;

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
        }
        void DrawTreeView()
        {
            if (treeRoot != null && target != null)
            {
                treeRoot.treeView.OnGUI(treeViewContainer.layout);
            }
        }

        DataGroup bookmarkGroup = null;
        void CreateNeededAsset()
        {
            //bookmark group
            bookmarkGroup = AssetDatabase.LoadAssetAtPath<DataGroup>($"{path}/OnionBookmarkGroup.asset");
            if (bookmarkGroup == null)
            {
                bookmarkGroup = CreateInstance<DataGroup>();
                Debug.Log("Auto create bookmark group.");
                AssetDatabase.CreateAsset(bookmarkGroup, $"{path}/OnionBookmarkGroup.asset");
            }

            //bookmark folder
            if (AssetDatabase.IsValidFolder($"{path}/Bookmark") == false)
                AssetDatabase.CreateFolder(path, "Bookmark");

        }


        public void SetTarget(IQueryableData newTarget)
        {
            SetTarget(newTarget as Object);

        }
        public void SetTarget(Object newTarget)
        {
            target = newTarget;
        }


        void OnFresh()
        {
            if (treeRoot != null)
            {
                treeRoot.CreateTreeView();
                if (treeViewState == null)
                    treeViewState = treeRoot.treeView.state;
            }
        }

        void OnToggleBookmark()
        {
            int index = -1;
            if (bookmarkGroup != null && target != bookmarkGroup)
            {
                index = bookmarkGroup.OfType<OnionBookmark>().Select(_ => _.target).ToList().IndexOf(target);

                if (index >= 0)
                    OnRemoveBookmark();
                else
                    OnAddBookmark();

                FreshBookmarkView(target);
            }

            void OnRemoveBookmark()
            {
                SerializedObject serializedObject = new SerializedObject(bookmarkGroup);
                SerializedProperty data = serializedObject.FindProperty("data");

                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(data.GetArrayElementAtIndex(index).objectReferenceValue));

                data.MoveArrayElement(index, data.arraySize - 1);
                data.arraySize = data.arraySize - 1;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(bookmarkGroup);
                AssetDatabase.SaveAssets();

            }

            void OnAddBookmark()
            {
                var bookmark = CreateInstance<OnionBookmark>();
                bookmark.target = target;

                AssetDatabase.CreateAsset(bookmark, $"{path}/Bookmark/B_{target.name}.asset");

                SerializedObject serializedObject = new SerializedObject(bookmarkGroup);
                SerializedProperty data = serializedObject.FindProperty("data");

                int arraySize = data.arraySize;
                data.InsertArrayElementAtIndex(arraySize);
                data.GetArrayElementAtIndex(arraySize).objectReferenceValue = bookmark;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(bookmarkGroup);
                AssetDatabase.SaveAssets();

            }

        }

        void OpenDocument()
        {
            OnionDocumentWindow.ShowWindow(OnionDocument.GetDocument(selectedNode.dataObj));
        }

        void OnSearchTarget()
        {
            int controlID = rootVisualElement.Q<Button>("btn-search-target").GetHashCode();
            EditorGUIUtility.ShowObjectPicker<ScriptableObject>(null, false, "", controlID);
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

        void OnTargetChange(Object newTarget)
        {
            VisualElement containerRoot = rootVisualElement.Q("tree-view-container");
            containerRoot.visible = (newTarget != null);

            if(newTarget != bookmarkGroup)
            {
                openedTarget = newTarget;
                SwitchTab(Tab.Opened);
            }
            else
            {
                SwitchTab(Tab.Bookmark);
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
                onSelectInspector = newNode.onInspectorAction;
            }

            DisplayInfo(newNode);

            var root = rootVisualElement;
            root.Q("btn-info-document").style.display = new StyleEnum<DisplayStyle>(newNode.dataObj == null ? DisplayStyle.None : DisplayStyle.Flex);
        }


        void SetIcon(VisualElement el, string iconName)
        {
            el.style.backgroundImage = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"{path}/Editor/Icons/{iconName}.png"));
        }


        //Inspector
        OnionAction onSelectInspector;
        void DrawInspector()
        {
            if (selectedNode != null && onSelectInspector != null)
            {
                onSelectInspector.action?.Invoke();
            }
        }

        List<Button> actionBtns = new List<Button>();
        void DisplayInfo(TreeNode node)
        {
            rootVisualElement.Q("btn-info-document").style.display = (node.dataObj != null) ? DisplayStyle.Flex : DisplayStyle.None;

            DisplayTextInfo(node);
            DisplayActionButtonInfo(node);
        }
        void DisplayTextInfo(TreeNode node)
        {
            var root = this.rootVisualElement;

            //text info
            string nodeTitle = node.GetTitle();
            string nodeDescription = node.GetDescription();

            root.Q<Label>("info-title").text = nodeTitle;
            root.Q<Label>("info-description").text = nodeDescription;

        }
        void DisplayActionButtonInfo(TreeNode node)
        {
            var root = this.rootVisualElement;

            var container = root.Q("data-info-btn-list");
            foreach (var actionBtn in actionBtns)
                container.Remove(actionBtn);
            actionBtns = new List<Button>();


            if (node.nodeActions != null)
            {
                foreach (var action in node.nodeActions)
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