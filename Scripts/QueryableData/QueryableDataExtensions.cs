﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class QueryableDataExtensions
{
    public static bool TryQueryByID<T>(this IQueryableData target, string id, out T result) where T : IQueryableData
    {
        foreach (var item in target)
        {
            if(item.GetID() == id)
            {
                result = (T)item;
                return true;
            }
        }

        result = default;
        return false;
    }

    public static T QueryByID<T>(this IQueryableData target, string id) where T : IQueryableData
    {
        T result = (T)target.SingleOrDefault(_ => _.GetID() == id);
        if(result == null)
        {
            UnityEngine.Debug.LogError("QueryableData在查找id時，找到的結果為null，可能是id重複或資料本身為null");
        }
        return result;
    }


    public static T GetDataAt<T>(this IQueryableData target, int index) where T :IQueryableData
    {
        return (T)target.ElementAt(index);
    }

    public static int IndexOf(this IQueryableData target, IQueryableData item)
    {
        int currentIndex = 0;

        foreach (var el in target)
        {
            if (el == item)
                return currentIndex;

            currentIndex++;
        }

        return -1;
    }

    public static int Count(this IQueryableData target)
    {
        int currentIndex = 0;
        foreach (var _ in target)
            currentIndex++;

        return currentIndex;
    }

}
