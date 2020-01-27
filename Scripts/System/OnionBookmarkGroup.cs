﻿#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionBookmarkGroup : QueryableData
    {
        const string nodeName = "Bookmarks";

        [NodeTitle]
        string title => nodeName;

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Bookmark_Fill");

        [SerializeField]
        [NodeElement]
        OnionBookmark[] data = new OnionBookmark[0];

        public override string GetID()
        {
            return nodeName;
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            return data;
        }
    }

    [CustomEditor(typeof(OnionBookmarkGroup))]
    public class OnionBookmarkGroupEditor : UnityEditor.Editor
    {
        OnionReorderableList userTagsList;

        private void OnEnable()
        {
            SerializedObject so = new SerializedObject(target);
            userTagsList = new OnionReorderableList(
                so.FindProperty("data"),
                (target as IQueryableData).GetID());
        }

        public override void OnInspectorGUI()
        {
            userTagsList.OnInspectorGUI();
        }

    }

}

#endif