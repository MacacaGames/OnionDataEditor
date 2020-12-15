using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace OnionCollections.DataEditor
{
    internal static class ReflectionUtility
    {
        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>取得MemberInfo的值。</summary>
        internal static T GetValue<T>(this MemberInfo memberInfo, object forObject) where T : class
        {
            if (memberInfo.TryGetValue(forObject, out T result))
                return result;
            else
                throw new NotImplementedException();
        }

        internal struct GetValueResult
        {
            public bool hasValue;
            public object value;
        }
        /// <summary>嘗試取得MemberInfo的值，並回傳取得成功與否。</summary>
        internal static bool TryGetValue<T>(this MemberInfo memberInfo, object forObject, out T value) where T : class
        {
            var result = memberInfo.TryGetValue<T>(forObject);
            value = result.value as T;
            return result.hasValue;
        }
        /// <summary>嘗試取得MemberInfo的值，並回傳取得資訊。</summary>
        internal static GetValueResult TryGetValue<T>(this MemberInfo memberInfo, object forObject) where T : class
        {
            object tempValue = null;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    tempValue = ((FieldInfo)memberInfo).GetValue(forObject);
                    break;
                case MemberTypes.Property:
                    tempValue = ((PropertyInfo)memberInfo).GetValue(forObject);
                    break;
            }

            if (tempValue == null)
            {
                return new GetValueResult
                {
                    hasValue = true,
                    value = null
                };
            }

            T resultValue = tempValue as T;
            return new GetValueResult
            {
                hasValue = tempValue is T,
                value = resultValue
            };
        }

        /// <summary>從MemberInfo集合中，篩選出有特定Attribute的項目。</summary>
        internal static IEnumerable<T> FilterWithAttribute<T>(this IEnumerable<T> target, Type attribute) where T : MemberInfo
        {
            List<T> result = new List<T>();
            foreach (var item in target)
            {
                var fieldAttrs = Attribute.GetCustomAttributes(item, attribute, true);
                if (fieldAttrs.Length > 0)
                    result.Add(item as T);
            }
            return result;
        }

        /// <summary>取得MemberInfo的System.Type。</summary>
        internal static Type GetMemberInfoType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    throw new NotImplementedException();
            }
        }


    }
}
