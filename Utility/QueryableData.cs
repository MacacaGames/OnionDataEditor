using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class QueryableData : ScriptableObject
{
    public int Count => GetData().Count();

    public abstract string GetID();

    public abstract IEnumerable<QueryableData> GetData();

    public IEnumerable<T> GetData<T>() where T:QueryableData
    {
        return GetData()?.OfType<T>();
    }

    public QueryableData QueryByID(string id)
    {
        if (TryQueryByID(id, out QueryableData result))
            return result;

        throw new KeyNotFoundException($"Could not find key:[{id}]");
    }
    
    public bool HasID(string id)
    {
        var data = GetData().SingleOrDefault(_ => _.GetID() == id);
        return data != null;
    }

    public bool TryQueryByID<T>(string id, out T result) where T: QueryableData
    {
        var data = GetData().SingleOrDefault(_ => _.GetID() == id);

        result = data as T;

        return data != null;
    }

    public int IndexOf(QueryableData item)
    {
        return Array.IndexOf(GetData().ToArray(), item);
    }
}
