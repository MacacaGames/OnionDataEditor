using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class QueryableDataExtensions
{
    public static bool TryQueryByID<T>(this IEnumerable<IQueryableData> target, string id, out T result) where T : IQueryableData
    {
        foreach (var item in target)
        {
            if (item.GetID() == id)
            {
                result = (T)item;
                return true;
            }
        }

        result = default;
        return false;
    }

    public static T QueryByID<T>(this IEnumerable<IQueryableData> target, string id) where T : IQueryableData
    {
        T result = (T)target.SingleOrDefault(_ => _.GetID() == id);
        if(result == null)
        {
            UnityEngine.Debug.LogError("QueryableData在查找id時，找到的結果為null，可能是id重複或資料本身為null");
        }
        return result;
    }


    public static T GetDataAt<T>(this IEnumerable<IQueryableData> target, int index) where T : IQueryableData
    {
        return (T)target.ElementAt(index);
    }

    //public static int IndexOf(this IEnumerable<IQueryableData> target, IQueryableData item)
    //{
    //    return target.IndexOf(item);
    //}

    //public static int Count(this IEnumerable<IQueryableData> target)
    //{
    //    return target.Count();
    //}

}
