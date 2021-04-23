#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionSetting : QueryableData, IEnumerable<TreeNode>
    {
        const string nodeName = "Settings";

        [NodeTitle]
        string title => nodeName;

        [NodeDescription]
        string description => "Onion data editor settings.";

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Settings");


        public override string GetID() => nodeName;

        public IEnumerator<TreeNode> GetEnumerator()
        {
            yield return BookmarksNode;
            yield return UserTagsNode;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }
        
        [NodeCustomElement]
        IEnumerable<TreeNode> nodes => this.ToArray();


        #region Bookmark
        
        public string[] bookmarkPaths = new string[0];

        OnionReorderableList bookmarkList;
        public TreeNode BookmarksNode
        {
            get
            {
                const string bookmarkTitle = "Bookmark";

                SerializedObject so = new SerializedObject(this);
                bookmarkList = new OnionReorderableList(so.FindProperty("bookmarkPaths"), bookmarkTitle, inspectGUI)
                {
                    Editable = false
                };

                var node = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = bookmarkTitle,
                    icon = OnionDataEditorWindow.GetIconTexture("Bookmark_Fill"),
                    onInspectorAction = new OnionAction(() =>
                    {
                        bookmarkList.OnInspectorGUI();
                    }),
                    tags = new[] { "Bookmarks" },
                };



                var bookmarkNodes = bookmarkPaths.Select(path =>
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    TreeNode n = new TreeNode(obj, TreeNode.NodeFlag.HideElementNodes)
                    {
                        onDoubleClickAction = new OnionAction(() =>
                        {
                            var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
                            onionWindow.SetTarget(obj);
                        })
                    };
                    return n;
                })
                .Where(n => n.isNull == false);


                node.AddChildren(bookmarkNodes);

                return node;

                void inspectGUI(Rect r, SerializedProperty sp, int inx)
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(bookmarkPaths[inx]);

                    const float objWidth = 150;
                    const float spaceWidth = 10;

                    Rect objRect = r;
                    objRect.width = objWidth;

                    Rect pathRect = r;
                    pathRect.x += objWidth + spaceWidth;
                    pathRect.width -= objWidth + spaceWidth;

                    EditorGUI.ObjectField(objRect, obj, typeof(UnityEngine.Object), true);
                    bookmarkPaths[inx] = GUI.TextField(pathRect, bookmarkPaths[inx]);

                    if (obj == null)
                    {
                        //IS NULL
                    }
                }

            }
        }

        public void AddBookmark(Object bookmark)
        {
            string v = AssetDatabase.GetAssetPath(bookmark);
            if (bookmarkPaths.Contains(v) == false)
            {
                SerializedObject serializedObject = new SerializedObject(this);
                SerializedProperty data = serializedObject.FindProperty("bookmarkPaths");

                int arraySize = data.arraySize;
                data.InsertArrayElementAtIndex(arraySize);
                data.GetArrayElementAtIndex(arraySize).stringValue = v;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public void RemoveBookmark(Object bookmark)
        {
            string v = AssetDatabase.GetAssetPath(bookmark);
            if (bookmarkPaths.Contains(v) == true)
            {
                SerializedObject serializedObject = new SerializedObject(this);
                SerializedProperty data = serializedObject.FindProperty("bookmarkPaths");

                var removedList = bookmarkPaths.ToList();
                removedList.Remove(v);
                bookmarkPaths = removedList.ToArray();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public bool IsAddedInBookmark(Object target)
        {
            string v = AssetDatabase.GetAssetPath(target);
            return bookmarkPaths.Contains(v) == true;
        }

        #endregion


        #region UserTag

        public string[] userTags = new string[0];

        OnionReorderableList userTagsList;
        TreeNode _userTagsNode;
        public TreeNode UserTagsNode
        {
            get
            {
                if (_userTagsNode == null)
                {
                    const string propertyTitle = "User Tags";
                    _userTagsNode = new TreeNode(TreeNode.NodeFlag.Pseudo)
                    {
                        displayName = propertyTitle,
                        icon = EditorGUIUtility.IconContent("FilterByLabel").image,
                        onInspectorAction = new OnionAction(() =>
                        {
                            if (userTagsList == null)
                            {
                                userTagsList = new OnionReorderableList(
                                    new SerializedObject(this).FindProperty("userTags"),
                                    propertyTitle);
                            }

                            userTagsList.OnInspectorGUI();
                        }),
                        tags = new[] { "UserTags" },
                    };
                }
                return _userTagsNode;
            }
        }
        
        #endregion

    }

    [CustomEditor(typeof(OnionSetting))]
    public class OnionSettingEditor: UnityEditor.Editor
    {
        List<TreeNode> nodes;
        private void OnEnable()
        {
            nodes = (target as OnionSetting).ToList();
        }


        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();

            foreach(var node in nodes)
            {
                ve.Add(new IMGUIContainer(node.onInspectorAction.action));
            }

            return ve;
        }

    }
}

#endif
