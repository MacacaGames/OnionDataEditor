using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Data/Data Group", fileName = "DataGroup")]
public class DataGroup : QueryableData, IEnumerable, IEnumerable<ScriptableObject>
{

    [SerializeField]
    string title;

    [SerializeField]
    string description;

    [Onion.NodeElement]
    [SerializeField]
    ScriptableObject[] data;
    
    public QueryableData this[string id] => QueryByID(id);
    public QueryableData this[int index] => data[index] as QueryableData;
        
    public override string GetID()
    {
        throw new NotImplementedException();
    }
    public override IEnumerable<QueryableData> GetData()
    {        
        return data?.OfType<QueryableData>();
    }
    
    public T Get<T>(int index) where T : ScriptableObject
    {
        return Get(index) as T;
    }
    public ScriptableObject Get(int index)
    {
        return data[index];
    }

    /// <summary>隨機取得一個。</summary>
    public T RandomPickOne<T>() where T : ScriptableObject
    {
        return Get<T>(UnityEngine.Random.Range(0, Count));
    }


    public IEnumerator GetEnumerator()
    {
        return data.GetEnumerator();
    }
    IEnumerator<ScriptableObject> IEnumerable<ScriptableObject>.GetEnumerator()
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
    
    public ScriptableObject[] elementData
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
