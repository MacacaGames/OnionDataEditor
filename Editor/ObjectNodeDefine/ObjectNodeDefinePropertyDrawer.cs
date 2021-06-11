using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

namespace OnionCollections.DataEditor.Editor {

    [CustomPropertyDrawer(typeof(ObjectNodeDefine))]
    public class ObjectNodeDefinePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return propertyHeight;
        }

        float propertyHeight = 10F;
        void RegHeight(Rect rect)
        {
            propertyHeight = Mathf.Max(rect.y + rect.height, propertyHeight);
        }

        UnityEngine.Object obj = null;

        const float gap = 3F;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;

            Rect r = position
                .SetHeight(h);

            var objectTypeSp = property.FindPropertyRelative("objectType");
            DrawObjectType(r, objectTypeSp);

            obj = EditorGUI.ObjectField(r, obj, typeof(UnityEngine.Object), false);

            //DefineGUI(position, property, !string.IsNullOrEmpty(objectTypeSp.stringValue));

            if (obj != null)
            {
                var t = obj.GetType();
                ObjectPropertyGUI(position.MoveDown(30), property, t);
            }

            void DrawObjectType(Rect rect, SerializedProperty sp)
            {
                EditorGUI.PropertyField(rect, sp);
            }
        }

        void DefineGUI(Rect position, SerializedProperty property, bool enable)
        {
            float h = EditorGUIUtility.singleLineHeight;

            GUI.enabled = enable;

            int index = 1;
            foreach (var ch in GetChildren(property))
            {
                Rect chRect = position
                    .SetHeight(h)
                    .MoveDown((h + gap) * index);
                RegHeight(chRect);

                switch (ch.name.Split('.').Last())
                {
                    case "objectType":
                        continue;

                    case "elementPropertyNames":
                        DrawElementPropertyName(chRect, ch);
                        break;

                    default:
                        DrawCommonProperty(chRect, ch);
                        break;

                }

                index++;
            }

            GUI.enabled = true;


            void DrawElementPropertyName(Rect rect, SerializedProperty sp)
            {
                var listElements = GetChildren(sp);

                EditorGUI.LabelField(rect, "Element Property Names");

                //size
                EditorGUI.PropertyField(
                    rect.ExtendLeft(-rect.width + 30),
                    listElements.First(),
                    GUIContent.none);

                int elListIndex = 0;
                foreach (var elListCh in listElements.Skip(1))
                {
                    if (elListCh.name == "size")
                        continue;

                    Rect elListChRect = rect
                        .ExtendLeft(-20F)
                        .MoveDown((elListIndex + 1) * (h + gap));

                    RegHeight(elListChRect);

                    EditorGUI.PropertyField(
                        elListChRect,
                        elListCh,
                        GUIContent.none);

                    elListIndex++;
                }
            }

            void DrawCommonProperty(Rect rect, SerializedProperty sp)
            {
                float btnWidth = 30F;
                EditorGUI.PropertyField(rect.ExtendRight(-btnWidth - gap), sp);
                if (GUI.Button(rect.ExtendLeft(-rect.width + btnWidth), "+"))
                {
                    Debug.Log(sp.name);
                }
            }
        }





        string objectTypeCache = null;
        MemberInfo[] memberInfosCache = null;
        readonly BindingFlags flag =
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly |
                    BindingFlags.GetProperty |
                    BindingFlags.GetField;


        void ObjectPropertyGUI(Rect position, SerializedProperty property, Type type)
        {
            if (type.FullName != objectTypeCache)
            {
                memberInfosCache = type.GetMembers(flag);
                objectTypeCache = type.FullName;
            }

            float h = EditorGUIUtility.singleLineHeight;

            int index = 0;
            foreach (var m in memberInfosCache)
            {
                var r = position
                    .MoveDown(index * (h + gap))
                    .SetHeight(h);
                RegHeight(r);

                Type propType = null;
                if (m.MemberType == MemberTypes.Property)
                {
                    propType = ((PropertyInfo)m).PropertyType;
                }
                else if (m.MemberType == MemberTypes.Field)
                {
                    propType = ((FieldInfo)m).FieldType;
                }
                else
                {
                    continue;
                }

                if (IsTitleType(propType) ||
                    IsDescriptionType(propType) ||
                    IsIconType(propType) ||
                    IsColorTagType(propType) ||
                    IsElementType(propType))
                {
                    EditorGUI.LabelField(r, $"{m.Name}({propType.Name})");
                    index++;
                }


            }

            bool IsTitleType(Type t) => IsType<string>(t);

            bool IsDescriptionType(Type t) => IsType<string>(t);

            bool IsIconType(Type t) => IsType<Texture>(t) || IsType<Sprite>(t);

            bool IsColorTagType(Type t) => IsType<Color>(t);

            bool IsElementType(Type t)
            {
                return
                    IsType<UnityEngine.Object>(t) ||
                    (t.IsSubclassOf(typeof(IEnumerable<>)) && IsType<UnityEngine.Object>(t.GenericTypeArguments[0])) ||
                    (t.IsArray && IsType<UnityEngine.Object>(t.GetElementType()));
            }

            bool IsType<T>(Type t)
            {
                return t == typeof(T) || t.IsSubclassOf(typeof(T));
            }
        }



        public static IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);
            while (true)
            {
                if ((SerializedProperty.EqualContents(property, nextElement)))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

    }

}