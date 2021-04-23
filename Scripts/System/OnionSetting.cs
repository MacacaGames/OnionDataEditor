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
        string Title => nodeName;

        [NodeDescription]
        string Description => "Onion data editor settings.";

        [NodeIcon]
        Texture2D Icon => OnionDataEditor.GetIconTexture("Settings");

        public override string GetID() => nodeName;

        [NodeCustomElement]
        IEnumerable<TreeNode> Nodes => this.ToArray();


        public IEnumerator<TreeNode> GetEnumerator()
        {
            yield return BookmarksNode;
            yield return UserTagsNode;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }



        #region Bookmark

        [SerializeField]
        public string[] bookmarkPaths = new string[0];

        OnionReorderableList bookmarkList;
        public TreeNode BookmarksNode
        {
            get
            {
                const string bookmarkTitle = "Bookmarks";

                if (bookmarkList == null)
                {
                    bookmarkList = new OnionReorderableList(
                        new SerializedObject(this).FindProperty("bookmarkPaths"),
                        bookmarkTitle,
                        inspectGUI);
                }


                var node = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = bookmarkTitle,
                    icon = OnionDataEditor.GetIconTexture("Bookmark_Fill"),
                    OnInspectorAction = new OnionAction(() => bookmarkList.OnInspectorGUI()),
                    tags = new[] { "Bookmarks" },
                };
                

                var bookmarkNodes = bookmarkPaths.Select(path =>
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    TreeNode n = new TreeNode(obj, TreeNode.NodeFlag.HideElementNodes)
                    {
                        OnDoubleClickAction = new OnionAction(() =>
                        {
                            var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
                            onionWindow.SetTarget(obj);
                        })
                    };
                    return n;
                })
                .Where(n => n.IsNull == false);


                node.AddChildren(bookmarkNodes);

                return node;


                void inspectGUI(Rect r, SerializedProperty sp, int inx)
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(bookmarkPaths[inx]);

                    if (obj == null)
                    {
                        //IS NULL
                        GUI.color = new Color(1F, 0.8F, 0.8F);
                    }

                    const float objWidth = 150;
                    const float spaceWidth = 10;

                    Rect objRect = r;
                    objRect.width = objWidth;

                    Rect pathRect = r;
                    pathRect.x += objWidth + spaceWidth;
                    pathRect.width -= objWidth + spaceWidth;

                    using (var ch = new EditorGUI.ChangeCheckScope())
                    {
                        var o = EditorGUI.ObjectField(objRect, obj, typeof(UnityEngine.Object), true);
                        if (ch.changed)
                        {
                            bookmarkPaths[inx] = AssetDatabase.GetAssetPath(o);
                        }
                    }

                    using (var ch = new EditorGUI.ChangeCheckScope())
                    {
                        var p = GUI.TextField(pathRect, bookmarkPaths[inx]);
                        if (ch.changed)
                        {
                            bookmarkPaths[inx] = p;
                        }
                    }


                    GUI.color = Color.white;

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

        [SerializeField]
        public string[] userTags = new string[0];

        OnionReorderableList userTagsList;
        public TreeNode UserTagsNode
        {
            get
            {
                const string propertyTitle = "User Tags";

                if (userTagsList == null)
                {
                    userTagsList = new OnionReorderableList(
                        new SerializedObject(this).FindProperty("userTags"),
                        propertyTitle);
                }

                var node = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = propertyTitle,
                    icon = EditorGUIUtility.IconContent("FilterByLabel").image,
                    OnInspectorAction = new OnionAction(() => userTagsList.OnInspectorGUI()),
                    tags = new[] { "UserTags" },
                };


                return node;
            }
        }
        
        #endregion

    }

    [CustomEditor(typeof(OnionSetting))]
    public class OnionSettingEditor: UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            List<TreeNode> nodes = (target as OnionSetting).ToList();
            foreach (var node in nodes)
            {
                root.Add(new IMGUIContainer(node.OnInspectorAction.action));

                var space = new VisualElement();
                space.style.height = new StyleLength(10);
                root.Add(space);
            }

            return root;
        }

    }
}

#endif
