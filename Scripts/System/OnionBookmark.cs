#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionBookmark : QueryableData
    {
        public Object target;

        [SerializeField]
        string title;

        [NodeTitle]
        string bookmarkName
        {
            get
            {
                if (string.IsNullOrEmpty(title))
                {
                    if (TargetIsNull() == false)
                        return target.name;
                    return "<Object is null>";
                }

                return title;
            }
        }

        [NodeDescription]
        [TextArea(1,5)]
        [SerializeField]
        string description;

        [NodeAction("Open")]
        [NodeOnDoubleClick]
        void OpenData()
        {
            if (TargetIsNull())
            {
                EditorUtility.DisplayDialog("Oh no!", "Target is null. Can not be opened.", "Ok");
            }
            else
            {
                var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();
                onionWindow.SetTarget(target);
            }
        }

        bool TargetIsNull() => target == null;

        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

    }
}

#endif