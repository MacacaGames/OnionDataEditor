using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class QueryableData : ScriptableObject, IQueryableData
{
    public abstract string GetID();

    //public abstract IEnumerable<IQueryableData> GetData();

    //public IEnumerable<T> GetData<T>() where T : IQueryableData
    //{
    //    if (this is IEnumerable<IQueryableData> q)
    //    {

    //        return q.OfType<T>();
    //    }
    //    else
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
    
}
