using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Data/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData, IEnumerable, IEnumerable<QueryableData>
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

    public override string GetID()
    {
        throw new NotImplementedException();
    }
    public override IEnumerable<IQueryableData> GetData()
    {
        return data;
    }

    public T Get<T>(int index) where T : ScriptableObject
    {
        return Get(index) as T;
    }
    public QueryableData Get(int index)
    {
        return data[index];
    }

    /// <summary>隨機取得一個。</summary>
    public T RandomPickOne<T>() where T : ScriptableObject
    {
        return Get<T>(UnityEngine.Random.Range(0, this.Count()));
    }


    IEnumerator<QueryableData> IEnumerable<QueryableData>.GetEnumerator()
    {
        foreach (var item in data)
            yield return item;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in data)
            yield return item;
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
/*
public class DataGroup<T> : DataGroup where T: IQueryableData
{
    public new T this[string id] => QueryByID(id);
    public new T this[int index] => data[index];
}
*/
