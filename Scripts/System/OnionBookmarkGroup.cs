#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionBookmarkGroup : QueryableData, IEnumerable<OnionBookmark>
    {
        const string nodeName = "Bookmarks";

        [NodeTitle]
        string title => nodeName;

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Bookmark_Fill");

        [SerializeField]
        //[NodeElement]
        OnionBookmark[] data = new OnionBookmark[0];

        [SerializeField]
        public int[] bookmarkGUIDs = new int[0];



        public override string GetID() => nodeName;

        public IEnumerator<OnionBookmark> GetEnumerator()
        {
            foreach (var item in data)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in data)
                yield return item;
        }

        public void AddBookmark(Object bookmark)
        {
            int v = bookmark.GetInstanceID();
            if (bookmarkGUIDs.Contains(v) == false)
            {
                SerializedObject serializedObject = new SerializedObject(this);
                SerializedProperty data = serializedObject.FindProperty("bookmarkGUIDs");

                int arraySize = data.arraySize;
                data.InsertArrayElementAtIndex(arraySize);
                data.GetArrayElementAtIndex(arraySize).intValue = v;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public void RemoveBookmark(Object bookmark)
        {

            int v = bookmark.GetInstanceID();
            if (bookmarkGUIDs.Contains(v) == true)
            {
                SerializedObject serializedObject = new SerializedObject(this);
                SerializedProperty data = serializedObject.FindProperty("bookmarkGUIDs");

                var removedList = bookmarkGUIDs.ToList();
                removedList.Remove(v);
                bookmarkGUIDs = removedList.ToArray();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public bool IsAddedInBookmark(Object target)
        {
            return OnionDataEditor.bookmarkGroup.bookmarkGUIDs.Contains(target.GetInstanceID());
        }


        [NodeCustomElement]
        IEnumerable< TreeNode> nodes
        {
            get
            {
                return bookmarkGUIDs
                    .Select(id =>
                    {
                        var obj = EditorUtility.InstanceIDToObject(id);
                        TreeNode node = new TreeNode(obj, TreeNode.NodeFlag.HideElementNodes)
                        {
                            onDoubleClickAction = new OnionAction(() =>
                            {
                                var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
                                onionWindow.SetTarget(obj);
                            })
                        };
                        return node;
                    });                    
            }
        }



    }

    [CustomEditor(typeof(OnionBookmarkGroup))]
    public class OnionBookmarkGroupEditor : UnityEditor.Editor
    {
        OnionReorderableList bookmarkList;

        private void OnEnable()
        {
            var bookmarkGUIDs = (target as OnionBookmarkGroup).bookmarkGUIDs;


            SerializedObject so = new SerializedObject(target);
            bookmarkList = new OnionReorderableList(
                //so.FindProperty("data"),
                so.FindProperty("bookmarkGUIDs"),
                (target as IQueryableData).GetID(),
                (r, sp, inx) => 
                {
                    var o = EditorUtility.InstanceIDToObject(bookmarkGUIDs[inx]);
                    if (o != null)
                    {
                        EditorGUI.ObjectField(r, o, typeof(Object), true);
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