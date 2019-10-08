using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        foreach(var data in queryableObj)
        {
            if (data.GetID() == id)
            {
                result = data;
                return true;
            }
        }
        result = null;
        return false;
    }

    public static bool HasID(this IQueryableData queryableObj, string id)
    {
        foreach (var data in queryableObj)
            if (data.GetID() == id)
                return true;

        return false;
    }

    public static int IndexOf(this IQueryableData queryableObj, IQueryableData item)
    {
        int index = -1;
        foreach (var data in queryableObj)
        {
            index++;
            if (data == item)
                return index;
        }

        return -1;
    }


}
