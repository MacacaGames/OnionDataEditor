using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class QueryableData : ScriptableObject, IQueryableData
{
    public abstract string GetID();

    public abstract IEnumerable<IQueryableData> GetData();

    public IEnumerable<T> GetData<T>() where T : IQueryableData
    {
        return GetData()?.OfType<T>();
    }
    
    IEnumerator<IQueryableData>  IEnumerable<IQueryableData>.GetEnumerator()
    {
        foreach (var item in GetData())
            yield return item;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in GetData())
            yield return item;
    }
}
