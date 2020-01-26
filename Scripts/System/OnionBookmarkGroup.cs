#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionBookmarkGroup : QueryableData
    {
        [NodeTitle]
        string title => "Bookmarks";

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Bookmark_Fill");

        [SerializeField]
        [NodeElement]
        OnionBookmark[] data = new OnionBookmark[0];

        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            return data;
        }
    }
}

#endif