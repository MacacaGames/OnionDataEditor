
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
            yield return OnboardingPageNode;
            yield return UserTagsNode;
            yield return CustomObjectNodeDefineNode;
            yield return OtherNode;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }

        public TreeNode OnboardingNode =>
            (onboardingPageType == OnboardingPageType.Bookmark && customOnboardingPage != null) ?
            OnionDataEditor.Bookmarks :
            new TreeNode(customOnboardingPage);

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
                        customGUI = InspectGUI,
                    };
                }


                var node = new TreeNode()
                {
                    displayName = bookmarkTitle,
                    icon = OnionDataEditor.GetIconTexture("Bookmark_Fill"),
                    OnInspectorGUI = bookmarkList.OnInspectorGUI,
                    tags = new[] { "Bookmarks" },
                };

                node.OnRebuild = BuildChildren;

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
                                OnDoubleClick = () =>
                                {
                                    OnionDataEditorWindow.OpenInNewTab(obj);
                                },
                            };
                            return n;
                        })
                        .Where(n => n.IsNull == false);

                    node.AddChildren(bookmarkNodes);
                }

                void InspectGUI(Rect r, SerializedProperty sp, int inx)
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
                SerializedObject so = new SerializedObject(this);

                if (userTagsList == null)
                {
                    userTagsList = new OnionReorderableList(so.FindProperty("userTags"))
                    {
                        title = propertyTitle,
                        titleIcon = OnionDataEditor.GetIconTexture("Tag"),
                    };
                }

                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Tag"),
                    OnInspectorGUI = userTagsList.OnInspectorGUI,
                    //OnInspectorVisualElementRoot = CreateInspectorVisualElement(),
                    tags = new[] { "UserTags" },
                };


                return node;


                VisualElement CreateInspectorVisualElement()
                {
                    VisualElement content = new VisualElement();

                    //不知道為什麼ListView就是無法顯示
                    ListView listView = new ListView()
                    {
                        bindingPath = "userTags",
                        itemHeight = 30,
                        itemsSource = userTags,


                    }.AddTo(content);

                    listView.Bind(so);

                    OnionPanel root = new OnionPanel(propertyTitle, OnionDataEditor.GetIconTexture("Tag"), content);
                    root.Bind(so);

                    return root;
                }

            }
        }

        #endregion



        #region CustomObjectNodeDefine

        [SerializeField]
        public ObjectNodeDefineObject[] objectNodeDefineObjects = new ObjectNodeDefineObject[0];

        OnionReorderableList objectNodeDefineObjectList;

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
                        titleIcon = OnionDataEditor.GetIconTexture("Add"),
                        customGUI = InspectGUI,
                    };
                }

                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Add"),
                    tags = new[] { "CustomObjectNodeDefine" },
                    OnInspectorGUI = objectNodeDefineObjectList.OnInspectorGUI,

                };

                node.NodeActions = new List<OnionAction>
                {
                    new OnionAction(GetAllDefineInProject,"Get All Define In Project")
                };

                UpdateNodeChildren();

                return node;



                void InspectGUI(Rect r, SerializedProperty sp, int inx)
                {

                    float activeWidth = r.height;
                    float typeNameWidth = 150F;

                    Rect activeRect = new Rect(r)
                        .SetWidth(activeWidth);

                    Rect typeNameRect = new Rect(r)
                        .MoveRight(activeWidth)
                        .SetWidth(typeNameWidth);

                    Rect propertyRect = new Rect(r)
                        .ExtendLeft(-activeWidth)
                        .ExtendLeft(-typeNameWidth);

                    if (sp.objectReferenceValue != null)
                    {

                        var itemSo = new SerializedObject(sp.objectReferenceValue);

                        bool v = itemSo.FindProperty("isActive").boolValue;
                        using (var ch = new EditorGUI.ChangeCheckScope())
                        {
                            //Active
                            v = EditorGUI.Toggle(activeRect, v);

                            if (ch.changed)
                            {
                                itemSo.FindProperty("isActive").boolValue = v;
                                itemSo.ApplyModifiedProperties();
                            }
                        }


                        GUI.color = v ? Color.white : new Color(1, 1, 1, 0.5F);

                        //Name
                        EditorGUI.LabelField(typeNameRect, objectNodeDefineObjects[inx].Title);

                        //Property
                        EditorGUI.PropertyField(propertyRect, sp, GUIContent.none);

                        GUI.color = Color.white;
                    }
                    else
                    {
                        EditorGUI.PropertyField(propertyRect, sp, GUIContent.none);
                    }
                }


                ObjectNodeDefineObject[] GetObjectNodeDefineObjectsInProject()
                {
                    string[] guids = AssetDatabase.FindAssets($"t:{nameof(ObjectNodeDefineObject)}");

                    var result = guids
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Select(path => AssetDatabase.LoadAssetAtPath<ObjectNodeDefineObject>(path))
                        .Where(n => n != null)
                        .ToArray();

                    return result;
                }

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



        #region BoardingPage

        enum OnboardingPageType
        {
            Bookmark = 0,
            Custom = 1,
        }

        [SerializeField]
        OnboardingPageType onboardingPageType;

        [SerializeField]
        Object customOnboardingPage;

        public TreeNode OnboardingPageNode
        {
            get
            {
                const string propertyTitle = "Onoarding Page";
                SerializedObject so = new SerializedObject(this);


                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Edit"),
                    tags = new[] { "OnboardingPage" },
                    OnInspectorVisualElementRoot = CreateOnBoardingPageOnInspector(),
                };

                return node;

                VisualElement CreateOnBoardingPageOnInspector()
                {
                    VisualElement content = new VisualElement();

                    PropertyField em = new PropertyField()
                    {
                        label = "Onboarding Page",
                        bindingPath = "onboardingPageType",
                    }.AddTo(content);

                    PropertyField objectField = new PropertyField()
                    {
                        label = "Custom Onboarding",
                        bindingPath = "customOnboardingPage",
                    }.AddTo(content);


                    em.RegisterValueChangeCallback(n =>
                    {
                        objectField.ShowIf((OnboardingPageType)(n.changedProperty.intValue) == OnboardingPageType.Custom);
                    });

                    OnionPanel root = new OnionPanel("Onboarding Page", OnionDataEditor.GetIconTexture("Edit"), content);
                    root.Bind(so);

                    return root;
                }
            }
        }

        #endregion


        #region Other

        [SerializeField]
        public bool isFullWidth = true;

        [SerializeField]
        public bool isRowGUIActive = true;


        public TreeNode OtherNode
        {
            get
            {
                const string propertyTitle = "Other";
                SerializedObject so = new SerializedObject(this);


                var node = new TreeNode()
                {
                    displayName = propertyTitle,
                    icon = OnionDataEditor.GetIconTexture("Trash"),
                    tags = new[] { "Other" },
                    OnInspectorVisualElementRoot = CreateOnBoardingPageOnInspector(),
                };

                return node;

                VisualElement CreateOnBoardingPageOnInspector()
                {
                    VisualElement content = new VisualElement();


                    //Full Width
                    OnionToggle isFullWidthField = new OnionToggle()
                    {
                        label = "Full Width",
                        bindingPath = "isFullWidth",
                    }
                    .AddTo(content);

                    isFullWidthField.RegisterValueChangedCallback(n =>
                    {
                        OnionDataEditorWindow.SetInspectorWidthFull(n.newValue);
                    });


                    //Full Width
                    OnionToggle isRowGUIActiveField = new OnionToggle()
                    {
                        label = "Show Row Additive GUI",
                        bindingPath = "isRowGUIActive",
                    }
                    .AddTo(content);

                    isRowGUIActiveField.RegisterValueChangedCallback(n =>
                    {
                        OnionDataEditorWindow.SetRowGUIActive(n.newValue);
                    });



                    OnionPanel root = new OnionPanel("Other", OnionDataEditor.GetIconTexture("Trash"), content);
                    root.Bind(so);

                    return root;
                }
            }
        }


        #endregion

        
    }
}
