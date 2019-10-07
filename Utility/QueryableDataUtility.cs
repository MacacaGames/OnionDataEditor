using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class QueryableDataUtility
{
    public static IQueryableData QueryByID(this IQueryableData queryableObj, string id)
    {
            if (TryQueryByID(queryableObj, id, out IQueryableData result))
                return result;

            throw new KeyNotFoundException($"Could not find key:[{id}]");        
    }

    public static bool TryQueryByID(this IQueryableData queryableObj, string id, out IQueryableData result)
    {
        IQueryableData data = queryableObj.GetData().SingleOrDefault(_ => _.GetID() == id);
        result = data;
        return data != null;
    }

    public static int Count(this IQueryableData queryableObj)
    {
        return queryableObj.GetData().Count();
    }

    public static bool HasID(this IQueryableData queryableObj,string id)
    {
        var data = queryableObj.GetData().SingleOrDefault(_ => _.GetID() == id);
        return data != null;
    }

    public static int IndexOf(this IQueryableData queryableObj,IQueryableData item)
    {
        return Array.IndexOf(queryableObj.GetData().ToArray(), item);
    }


}
