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


    public QueryableData this[string id] => this.QueryByID<QueryableData>(id);
    public QueryableData this[int index] => data[index];


    public override string GetID()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IQueryableData> GetData()
    {
        return data;
    }



#if (UNITY_EDITOR)
    
    [OnionCollections.DataEditor.NodeTitle]
    string nodeTitle => string.IsNullOrEmpty(title) ? name : title;

    [OnionCollections.DataEditor.NodeDescription]
    string nodeDescription => description;

#endif

}
