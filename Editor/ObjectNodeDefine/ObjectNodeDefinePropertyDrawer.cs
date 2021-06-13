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

        const float gap = 3F;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;

            var objectTypeSp = property.FindPropertyRelative("objectType");
            ObjectTypeGUI(position, objectTypeSp);

            Type resultType = NodeExtensions.GetTypeByName(objectTypeSp.stringValue);
            if (resultType != null)
            {
                EditorGUI.LabelField(position.MoveDown(h + gap).SetHeight(h), $"type in {resultType.FullName}", EditorStyles.miniLabel);
                ObjectPropertyGUI(position.MoveDown(50), property, resultType);
            }
            else
            {
                EditorGUI.LabelField(position.MoveDown(h + gap).SetHeight(h), $"Nothing");
            }
        }

        void ObjectTypeGUI(Rect position, SerializedProperty property)
        {
            float h = EditorGUIUtility.singleLineHeight;

            Rect r = position
                .SetHeight(h);

            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                string objectTypeStr = EditorGUI.DelayedTextField(r, property.stringValue);

                if (ch.changed)
                {
                    property.stringValue = objectTypeStr;
                }
            }

        }


        [System.Obsolete]
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


        enum DefineType
        {
            Title = 0,
            Description = 1,
            Icon = 2,
            TagColor = 3,
            Element = 4,
        }



        void ObjectPropertyGUI(Rect position, SerializedProperty property, Type type)
        {
            if (type.FullName != objectTypeCache)
            {
                memberInfosCache = type.GetMembers(flag);
                objectTypeCache = type.FullName;
            }

            float h = EditorGUIUtility.singleLineHeight;

            List<(DefineType defineType, MemberInfo memberInfo, Type memberType)> members = new List<(DefineType, MemberInfo, Type)>();

            foreach (var m in memberInfosCache)
            {

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

                if (IsTitleType(propType))
                    members.Add((DefineType.Title, m, propType));

                if(IsDescriptionType(propType))
                    members.Add((DefineType.Description, m, propType));

                if (IsIconType(propType))
                    members.Add((DefineType.Icon, m, propType));

                if (IsColorTagType(propType))
                    members.Add((DefineType.TagColor, m, propType));

                if (IsElementType(propType))
                    members.Add((DefineType.Element, m, propType));


            }

            //


            const float titleWidth = 150F;
            int index = 0;
            foreach (DefineType defineType in Enum.GetValues(typeof(DefineType)).Cast<DefineType>())
            {

                var rLeft = position
                    .MoveDown(index * (h + gap))
                    .SetWidth(titleWidth)
                    .SetHeight(h);

                Texture icon = OnionDataEditor.GetIconTexture($"Node_{defineType}");
                EditorGUI.LabelField(rLeft, new GUIContent($"{defineType}", icon), EditorStyles.boldLabel);


                var rRight = position
                    .MoveDown(index * (h + gap))
                    .ExtendLeft(-titleWidth)
                    .SetHeight(h);
                RegHeight(rRight);

                var list = members.Where(n => n.defineType == defineType).Select(n => n.memberInfo.Name).ToList();

                if (list.Count > 0)
                {
                    const string defaultString = "( Default )";
                    list.Insert(0, defaultString);

                    if (defineType == DefineType.Element)
                    {

                        var sp = GetSpByDefineType(defineType);

                        HashSet<string> elList = new HashSet<string>();
                        for(int i = 0; i < sp.arraySize; i++)
                        {
                            elList.Add(sp.GetArrayElementAtIndex(i).stringValue);
                        }

                        string displayName = elList.Count == 0 ? defaultString : string.Join(", ", elList);
                        if (GUI.Button(rRight, displayName, EditorStyles.popup))
                        {
                            GenericMenu menu = new GenericMenu();
                            for (int i = 0; i < list.Count; i++)
                            {
                                int iCache = i;
                                bool isOn = elList.Contains(list[i]);
                                menu.AddItem(
                                    new GUIContent(list[i]),
                                    iCache == 0 ? elList.Count == 0 : isOn,
                                    () =>
                                    {
                                        if (iCache == 0)
                                        {
                                            //Default
                                            elList.Clear();
                                            sp.ClearArray();
                                        }
                                        else
                                        {
                                            if (isOn)
                                            {
                                                elList.Remove(list[iCache]);
                                            }
                                            else
                                            {
                                                elList.Add(list[iCache]);
                                            }
                                            sp.ClearArray();

                                            foreach (var elString in elList)
                                            {
                                                sp.InsertArrayElementAtIndex(sp.arraySize);
                                                sp.GetArrayElementAtIndex(sp.arraySize - 1).stringValue = elString;
                                            }
                                        }

                                        sp.serializedObject.ApplyModifiedProperties();
                                        Debug.Log(string.Join(",", elList));
                                    });
                            }

                            menu.DropDown(rRight.MoveDown(h).SetSize(0, 0));
                        }
                    }
                    else
                    {
                        var sp = GetSpByDefineType(defineType);

                        EditorGUI.BeginChangeCheck();
                        int originIndex = list.IndexOf(sp.stringValue);
                        originIndex = Mathf.Max(originIndex, 0);

                        int selectIndex = EditorGUI.Popup(rRight, originIndex , list.ToArray());
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (selectIndex != 0)
                            {
                                sp.stringValue = list[selectIndex];
                            }
                            else
                            {
                                sp.stringValue = "";
                            }
                            sp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                else
                {
                    GUI.color = new Color(1, 1, 1, 0.25F);

                    EditorGUI.LabelField(rRight, $"Nothing can select.", EditorStyles.miniLabel);

                    GUI.color = Color.white;
                }

                index++;
            }


            SerializedProperty GetSpByDefineType(DefineType defineType)
            {
                switch (defineType)
                {
                    case DefineType.Title:
                        return property.FindPropertyRelative("titlePropertyName");

                    case DefineType.Description:
                        return property.FindPropertyRelative("descriptionPropertyName");

                    case DefineType.Icon:
                        return property.FindPropertyRelative("iconPropertyName");

                    case DefineType.TagColor:
                        return property.FindPropertyRelative("tagColorPorpertyName");

                    case DefineType.Element:
                        return property.FindPropertyRelative("elementPropertyNames");

                    default:
                        throw new Exception($"Can not find compare serialized property by {defineType}");
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