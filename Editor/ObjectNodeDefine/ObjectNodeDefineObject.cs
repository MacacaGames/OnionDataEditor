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
        ObjectNodeDefine define;

        public ObjectNodeDefine GetDefine()
        {
            return define;
        }

    }
}