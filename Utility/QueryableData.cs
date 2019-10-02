using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class QueryableData : ScriptableObject
{
    public virtual QueryableData this[string id] => QueryById(id);

    public virtual QueryableData this[int index] => GetData().ToList()[index];

    public abstract string GetID();

    public abstract IEnumerable<QueryableData> GetData();

    public IEnumerable<T> GetData<T>() where T:QueryableData
    {
        return GetData()?.OfType<T>();
    }
    
    protected QueryableData QueryById(string id)
    {
        var data = GetData().SingleOrDefault(_ => _.GetID() == id);
        if (data == null)
            throw new KeyNotFoundException($"Could not find key:[{id}]");

        return data;
    }
    
    public bool HasID(string id)
    {
        var data = GetData().SingleOrDefault(_ => _.GetID() == id);
        return data != null;
    }
}
