using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OnionCollections.DataEditor
{
    [OpenWithOnionDataEditor(true)]
    [CreateAssetMenu(menuName = "Onion Data Editor/Queryable Group", fileName = "QueryableGroup")]
    public class QueryableGroup : QueryableData, IEnumerable<IQueryableData>
    {
        [SerializeField]
        string title;

        [SerializeField]
        string description;

        [NodeElement]
        [SerializeField]
        protected QueryableData[] data = new QueryableData[0];

        public IEnumerator<IQueryableData> GetEnumerator()
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

        public QueryableData this[string id] => QueryByID(id);

        public QueryableData QueryByID(string id)
        {
            return this.QueryByID<QueryableData>(id);
        }


#if (UNITY_EDITOR)

        [NodeTitle]
        string Title => string.IsNullOrEmpty(title) ? name : title;

        [NodeDescription]
        string Description => description;

        [NodeIcon]
        Texture Icon => (IsDataHaveNull || IsDataHaveRepeatId) ?
            UnityEditor.EditorGUIUtility.FindTexture("console.erroricon.sml") :
            UnityEditor.EditorGUIUtility.IconContent("d_Folder Icon").image;

        public bool IsDataHaveNull => this.Any(_ => _ == null);
        public bool IsDataHaveRepeatId => this.GroupBy(_ => _.GetID()).Any(_ => _.Count() > 1);

#endif

    }
}