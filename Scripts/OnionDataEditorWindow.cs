#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace OnionCollections.DataEditor
{
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

        public TreeViewState treeViewState;
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
            root.Q<Button>("btn-bookmark").clickable.clicked += () => { target = bookmarkGroup as ScriptableObject; };
            root.Q("btn-bookmark-icon").style.backgroundImage = EditorGUIUtility.FindTexture("FolderFavorite Icon");

            //綁定btn-add-bookmark
            root.Q<Button>("btn-add-bookmark").clickable.clicked += () =>
            {
                if (bookmarkGroup != null && target != bookmarkGroup)
                {
                    IEnumerable<OnionBookmark> bookmarks = bookmarkGroup.GetData<OnionBookmark>();

                    if (bookmarks == null || bookmarks.Select(_ => _.target).Contains(target) == false)
                    {
                        var bookmark = CreateInstance<OnionBookmark>();
                        bookmark.target = target;

                        AssetDatabase.CreateAsset(bookmark, $"{path}/Bookmark/B_{target.name}.asset");
                        var tempList = (bookmarkGroup.elementData == null) ? new List<QueryableData>() : new List<QueryableData>(bookmarkGroup.elementData);
                        tempList.Add(bookmark);
                        bookmarkGroup.elementData = tempList.ToArray();
                    }
                    else
                    {
                        Debug.LogError("This bookmark already exists.");
                    }
                }
            };
            root.Q("btn-add-bookmark-icon").style.backgroundImage = EditorGUIUtility.FindTexture("Favorite Icon");

            //綁定btn-info-document
            root.Q<Button>("btn-info-document").clickable.clicked += () => 
            {
                OnionDocumentWindow.ShowWindow(OnionDocument.GetDocument(selectObject));
            };
            root.Q("btn-info-document-icon").style.backgroundImage = EditorGUIUtility.FindTexture("_Help");



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
                bookmarkGroup.elementData = new QueryableData[0];
                AssetDatabase.CreateAsset(bookmarkGroup, $"{path}/OnionBookmarkGroup.asset");
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
        ScriptableObject selectObject;
        public void OnTriggerItem(TreeNode node)
        {
            selectObject = node.dataObj;

            DisplayInfo(node);

            int selectionId = tree.treeView.GetSelection()[0];
            OnionAction onSelectedAction = tree.treeView.treeQuery[selectionId].onSelectedAction;
            if (onSelectedAction != null)
                onSelectedAction.action.Invoke();

        }
        public void OnDoubleClick(TreeNode node)
        {
            int selectionId = tree.treeView.GetSelection()[0];

            OnionAction onDoubleClickAction = tree.treeView.treeQuery[selectionId].onDoubleClickAction;
            if (onDoubleClickAction != null)
            {
                onDoubleClickAction.action.Invoke();
            }
            else
            {
                var selectionObject = tree.treeView.treeQuery[selectionId].dataObj;
                EditorGUIUtility.PingObject(selectionObject);   //Ping
            }
        }


    }

}
#endif