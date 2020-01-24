using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using OnionCollections.DataEditor;

[CreateAssetMenu(menuName = "Data/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData
{
    [SerializeField]
    string title;

    [SerializeField]
    string description;

    [NodeElement]
    [SerializeField]
    protected QueryableData[] data = new QueryableData[0];

    public override string GetID()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IQueryableData> GetData()
    {
        return data;
    }

    public QueryableData this[string id] => this.QueryByID<QueryableData>(id);
    public QueryableData this[int index] => this.GetDataAt<QueryableData>(index);


#if (UNITY_EDITOR)

    [NodeTitle]
    string nodeTitle => string.IsNullOrEmpty(title) ? name : title;

    [NodeDescription]
    string nodeDescription => description;

#endif

}
