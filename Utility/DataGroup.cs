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

    [Onion.NodeElement]
    [SerializeField]
    QueryableData[] data;

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

    public QueryableData[] elementData
    {
        set
        {
            data = value;
        }
        get
        {
            return data;
        }
    }

    [Onion.NodeTitle]
    string nodeTitle => string.IsNullOrEmpty(title) ? name : title;

    [Onion.NodeDescription]
    string nodeDescription => description;

#endif

}
