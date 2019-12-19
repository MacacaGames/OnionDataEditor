using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Data/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData
{

    [SerializeField]
    string title;

    [SerializeField]
    string description;

    [OnionCollections.DataEditor.NodeElement]
    [SerializeField]
    protected QueryableData[] data = new QueryableData[0];

    public QueryableData this[string id] => this.QueryByID(id) as QueryableData;
    public QueryableData this[int index] => data[index];

    public override IEnumerable<IQueryableData> GetData()
    {
        return data;
    }

    public override string GetID()
    {
        throw new NotImplementedException();
    }

    public T Get<T>(int index) where T : QueryableData
    {
        return Get(index) as T;
    }
    public QueryableData Get(int index)
    {
        return data[index];
    }

    /// <summary>隨機取得一個。</summary>
    public T RandomPickOne<T>() where T : QueryableData
    {
        return Get<T>(UnityEngine.Random.Range(0, this.Count()));
    }


#if (UNITY_EDITOR)
    
    [OnionCollections.DataEditor.NodeTitle]
    string nodeTitle => string.IsNullOrEmpty(title) ? name : title;

    [OnionCollections.DataEditor.NodeDescription]
    string nodeDescription => description;

#endif

}
