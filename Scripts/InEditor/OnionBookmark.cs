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
        [OnionCollections.DataEditor.NodeTitle]
        string bookmarkName => target.name;

        public ScriptableObject target;

        [NodeOnSelected]
        void OnSelected()
        {
            var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();

            onionWindow.target = target;

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