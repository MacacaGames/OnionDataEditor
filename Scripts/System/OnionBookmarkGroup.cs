#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionBookmarkGroup : QueryableData, IEnumerable<TreeNode>
    {
        const string nodeName = "Bookmarks";

        [NodeTitle]
        string title => nodeName;

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Bookmark_Fill");
        
        
        [HideInInspector]
        public string[] bookmarkPaths = new string[0];

        
        public override string GetID() => nodeName;


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

        public IEnumerator<TreeNode> GetEnumerator()
        {
            foreach(var node in nodes)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var node in nodes)
                yield return node;
        }

        [NodeCustomElement]
        IEnumerable< TreeNode> nodes
        {
            get
            {
                return bookmarkPaths
                    .Select(path =>
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        TreeNode node = new TreeNode(obj, TreeNode.NodeFlag.HideElementNodes)
                        {
                            onDoubleClickAction = new OnionAction(() =>
                            {
                                var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
                                onionWindow.SetTarget(obj);
                            })
                        };
                        return node;
                    })
                    .Where(n => n.isNull == false);                    
            }
        }



    }

    [CustomEditor(typeof(OnionBookmarkGroup))]
    public class OnionBookmarkGroupEditor : UnityEditor.Editor
    {
        OnionReorderableList bookmarkList;

        private void OnEnable()
        {
            var bookmarkPaths = (target as OnionBookmarkGroup).bookmarkPaths;


            SerializedObject so = new SerializedObject(target);
            bookmarkList = new OnionReorderableList(
                so.FindProperty("bookmarkPaths"),
                (target as IQueryableData).GetID(),
                (r, sp, inx) =>
                {

                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(bookmarkPaths[inx]);

                    const float objWidth = 150;
                    const float spaceWidth = 10;

                    Rect objRect = r;
                    objRect.width = objWidth;

                    Rect pathRect = r;
                    pathRect.x += objWidth + spaceWidth;
                    pathRect.width -= objWidth + spaceWidth;

                    EditorGUI.ObjectField(objRect, obj, typeof(Object), true);
                    bookmarkPaths[inx] = GUI.TextField(pathRect, bookmarkPaths[inx]);

                    if (obj == null)
                    {
                        //IS NULL
                    }
                });
            bookmarkList.Editable = false;
        }

        public override void OnInspectorGUI()
        {
            bookmarkList.OnInspectorGUI();
        }

    }

}

#endif