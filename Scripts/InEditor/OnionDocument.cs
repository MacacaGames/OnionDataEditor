#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionDocument
    {
        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static DocumentObject GetDocument(Object dataObj)
        {
            if (dataObj == null)
                return null;

            Type type = dataObj.GetType();

            List<DocumentObject.ElementFieldData> elData = new List<DocumentObject.ElementFieldData>();
            foreach (var el in type.GetMembers(defaultBindingFlags))
            {
                var attr = el.GetCustomAttribute<FieldDescriptionAttribute>();
                if (attr != null)
                {
                    elData.Add(new DocumentObject.ElementFieldData
                    {
                        name = el.Name,
                        description = attr.description
                    });
                }
            }
            
            return new DocumentObject
            {
                title = type.ToString(),
                data = elData
            };
        }

        public class DocumentObject
        {
            public string title;
            public List<ElementFieldData> data;

            public struct ElementFieldData
            {
                public string name;
                public string description;
            }
        }

    }
}

#endif