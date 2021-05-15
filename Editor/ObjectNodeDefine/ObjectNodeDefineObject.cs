using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    [OpenWithOnionDataEditor(true)]
    [CreateAssetMenu(menuName = "Onion Data Editor/Object Node Define Object", fileName = "ObjectNodeDefineObject")]
    internal class ObjectNodeDefineObject : ScriptableObject
    {
        [SerializeField]
        bool isActive;

        public bool Active => isActive;

        [NodeTitle]
        public string GetDisplayName
        {
            get
            {
                if (define.objectType == null)
                    return null;

                int startIndex = define.objectType.LastIndexOf('.');
                if (startIndex < 0)
                    return define.objectType;
                else
                    return define.objectType.Substring(startIndex + 1);
            }
        }

        [SerializeField]
        ObjectNodeDefine define;

        public ObjectNodeDefine GetDefine()
        {
            return define;
        }

    }
}