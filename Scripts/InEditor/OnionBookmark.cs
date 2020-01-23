#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionBookmark : QueryableData
    {
        [SerializeField]
        string title;

        public Object target;

        [NodeTitle]
        string bookmarkName => string.IsNullOrEmpty(title) ? target.name : title;

        [NodeDescription]
        [TextArea(1,3)]
        [SerializeField]
        string description;

        [NodeAction("Open")]
        [NodeOnDoubleClick]
        void OpenData()
        {
            var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
            onionWindow.SetTarget(target as IQueryableData);
        }

        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            throw new System.NotImplementedException();
        }
    }
}

#endif