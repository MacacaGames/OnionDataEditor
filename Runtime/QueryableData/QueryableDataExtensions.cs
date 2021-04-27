using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class QueryableDataExtensions
{
    /// <summary>
    /// Try to get compare target by id.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="id"></param>
    /// <param name="result"></param>
    /// <returns>If get compare target success, return true.</returns>
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

    /// <summary>
    /// Get compare target by id.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static T QueryByID<T>(this IEnumerable<IQueryableData> target, string id) where T : IQueryableData
    {
        T result = (T)target.SingleOrDefault(_ => _.GetID() == id);
        if(result == null)
        {
            UnityEngine.Debug.LogError($"Query result is null. Maybe the id is duplicate, or the result is null.");
        }
        return result;
    }

    /// <summary>
    /// Get element at index.
    /// </summary>
    [System.Obsolete("Use Linq.ElementAt")]
    public static T GetDataAt<T>(this IEnumerable<IQueryableData> target, int index) where T : IQueryableData
    {
        return (T)target.ElementAt(index);
    }
}
