
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
        Texture Icon => OnionDataEditor.GetIconTexture("Settings");

        public override string GetID() => nodeName;

        [NodeCustomElement]
        IEnumerable<TreeNode> Nodes => this.ToArray();


        public IEnumerator<TreeNode> GetEnumerator()
        {
            yield return BookmarksNode;
            yield return UserTagsNode;
            yield return CustomObjectNodeDefineNode;
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
                    bookmarkList = new OnionReorderableList(new SerializedObject(this).FindProperty("bookmarkPaths"))
                    {
                        title = bookmarkTitle,
                        titleIcon = OnionDataEditor.GetIconTexture("Bookmark_Fill"),
                        customGUI = inspectGUI,
                    };
                }


                var node = new TreeNode()
                {
                    displayName = bookmarkTitle,
                    icon = OnionDataEditor.GetIconTexture("Bookmark_Fill"),
                    OnInspectorAction = new OnionAction(bookmarkList.OnInspectorGUI),
                    tags = new[] { "Bookmarks" },
                };

                node.OnRebuildNode = new OnionAction(BuildChildren);

                BuildChildren();


                return node;

                void BuildChildren()
                {
                    node.ClearChildren();

                    var bookmarkNodes = bookmarkPaths
                        .Distinct()
                        .Select(path =>
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
                }

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

                serializedObject.ApplyModifiedProperties();
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
                    userTagsList = new OnionReorderableList(new SerializedObject(this).FindProperty("userTags"))
                    {
                        title = propertyTitle,
                        titleIcon = OnionDataEditor.GetIconTexture("Tag"),
                    };
                }

                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Tag"),
                    OnInspectorAction = new OnionAction(userTagsList.OnInspectorGUI),
                    tags = new[] { "UserTags" },
                };


                return node;
            }
        }

        #endregion


        #region CustomObjectNodeDefine


        [SerializeField]
        public ObjectNodeDefineObject[] objectNodeDefineObjects = new ObjectNodeDefineObject[0];

        OnionReorderableList objectNodeDefineObjectList;

        public ObjectNodeDefineObject[] GetObjectNodeDefineObjectsInProject()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(ObjectNodeDefineObject)}");

            var result = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<ObjectNodeDefineObject>(path))
                .Where(n => n != null)
                .ToArray();

            return result;
        }

        public TreeNode CustomObjectNodeDefineNode
        {
            get
            {
                const string propertyTitle = "Custom Object Node Define";
                SerializedObject so = new SerializedObject(this);

                if (objectNodeDefineObjectList == null)
                {
                    objectNodeDefineObjectList = new OnionReorderableList(new SerializedObject(this).FindProperty("objectNodeDefineObjects"))
                    {
                        title = propertyTitle,
                        titleIcon = OnionDataEditor.GetIconTexture("Tag"),
                    };
                }

                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Add"),
                    tags = new[] { "CustomObjectNodeDefine" },
                    OnInspectorAction = new OnionAction(() => objectNodeDefineObjectList.OnInspectorGUI()),
                    
                };

                node.NodeActions = new List<OnionAction>
                {
                    new OnionAction(GetAllDefineInProject,"Get All Define In Project")
                };

                UpdateNodeChildren();

                return node;

                void GetAllDefineInProject()
                {
                    objectNodeDefineObjects = GetObjectNodeDefineObjectsInProject();
                    EditorUtility.SetDirty(this);
                    UpdateNodeChildren();
                    OnionDataEditorWindow.UpdateTreeView();
                }

                void UpdateNodeChildren()
                {
                    node.ClearChildren();
                    var ch = objectNodeDefineObjects.Select(n => new TreeNode(n));
                    node.AddChildren(ch);
                }
            }
        }

        #endregion
    }



}
