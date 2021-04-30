using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using OnionCollections.DataEditor;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

[OpenWithOnionDataEditorAttribute(true)]
[CreateAssetMenu(menuName = "Onion Data Editor/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData, IEnumerable<IQueryableData>
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
        foreach(var item in data)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in data)
            yield return item;
    }

    public override string GetID() => title;

    public QueryableData this[string id] => this.QueryByID<QueryableData>(id);
    public QueryableData this[int index] => this.ElementAt(index) as QueryableData;


#if (UNITY_EDITOR)

    [NodeTitle]
    string Title => string.IsNullOrEmpty(title) ? name : title;

    [NodeDescription]
    string Description => description;

    [NodeIcon]
    Texture Icon => (IsDataHaveNull || IsDataHaveRepeatId) ?
        EditorGUIUtility.FindTexture("console.erroricon.sml") :
        EditorGUIUtility.IconContent("d_Folder Icon").image;

    public bool IsDataHaveNull => this.Any(_ => _ == null);
    public bool IsDataHaveRepeatId => this.GroupBy(_ => _.GetID()).Any(_ => _.Count() > 1);

#endif

}
