using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if (UNITY_EDITOR)
using OnionCollections.DataEditor.Editor;
#endif

namespace OnionCollections.DataEditor
{
    [OpenWithOnionDataEditor(true)]
    [CreateAssetMenu(menuName = "Onion Data Editor/Group", fileName = "Group")]
    public class Group : QueryableData, IEnumerable<Object>
    {
        [SerializeField]
        string title;

        [SerializeField]
        string description;

        [NodeElement]
        [SerializeField]
        protected Object[] data = new Object[0];

        public IEnumerator<Object> GetEnumerator()
        {
            foreach (var item in data)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in data)
                yield return item;
        }

        public override string GetID() => title;

        public int Count => data.Length;

        public Object this[int index] => GetElementAt(index);


        public Object GetElementAt(int index)
        {
            return this.ElementAt(index);
        }


#if (UNITY_EDITOR)

        [NodeTitle]
        string Title => string.IsNullOrEmpty(title) ? name : title;

        [NodeDescription]
        string Description => description;

        [NodeIcon]
        Texture Icon => (IsDataHaveNull) ?
            UnityEditor.EditorGUIUtility.FindTexture("console.erroricon.sml") :
            UnityEditor.EditorGUIUtility.IconContent("d_Folder Icon").image;

        public bool IsDataHaveNull => this.Any(_ => _ == null);

#endif

    }
}