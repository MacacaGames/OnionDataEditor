
#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEditor.IMGUI.Controls;

using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    public static class NodeExtensions
    {

        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        readonly static List<Type> nodeAttrTypeList = new List<Type>
        {
            typeof(NodeElementAttribute),
            typeof(NodeGroupedElementAttribute),
            typeof(NodeCustomElementAttribute),
            //++
        };

        static readonly Dictionary<Type, MemberInfo[]> memberCache = new Dictionary<Type, MemberInfo[]>();
        static readonly Dictionary<(Type objectType, Type memberType, string memberName, Type attr), bool> attrResultCache = new Dictionary<(Type objectType, Type memberType, string memberName, Type attr), bool>();

        static IEnumerable<TreeNode> GetChildNodeWithAttribute(Object target, MemberInfo member, Attribute attr)
        {
            //NodeElement
            if (attr is NodeElementAttribute)
            {
                return
                    GetSingleOrMultipleType<Object>()
                    .Select(_ => new TreeNode(_));
            }

            //NodeGroupedElement
            if (attr is NodeGroupedElementAttribute groupAttr)
            {
                TreeNode groupedNode = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = groupAttr.displayName,
                };

                IEnumerable<TreeNode> node = GetSingleOrMultipleType<Object>()
                    .Select(_ => new TreeNode(_));

                groupedNode.AddChildren(node);

                //若需要FindTree，則遍歷底下節點找
                if (groupAttr.findTree)
                {
                    foreach (var item in node)
                    {
                        item.GetElementTree();
                    }
                }

                //如果Element是Empty則不加入Group
                if ((groupAttr.hideIfEmpty == true && groupedNode.ChildCount == 0) == false)
                    return new List<TreeNode> { groupedNode };

                return Enumerable.Empty<TreeNode>();
            }

            //NodeCustomElement
            if (attr is NodeCustomElementAttribute)
            {
                return GetSingleOrMultipleType<TreeNode>();
            }

            throw new Exception($"Unknown Attribute {target.name}.{member.Name}(Attr:{attr})");


            //取得指定型別的T或IEnumerable<T>
            IEnumerable<T> GetSingleOrMultipleType<T>() where T : class
            {
                Type memberType = member.GetMemberInfoType();

                //Single
                if (memberType == typeof(T) || memberType.IsSubclassOf(typeof(T)))
                {
                    if (member.TryGetValue(target, out T resultCustomSingle))
                    {
                        return new List<T> { resultCustomSingle };
                    }
                }
                //Multiple
                if (typeof(IEnumerable).IsAssignableFrom(memberType))
                {
                    if (member.TryGetValue(target, out IEnumerable<T> resultCustom))
                        return resultCustom;
                }

                return Enumerable.Empty<T>();
            }

        }

        internal static List<TreeNode> GetElements(this TreeNode rootNode)
        {
            if (rootNode.IsPseudo == true)
                return rootNode.GetChildren().ToList();

            if (rootNode.IsNull == true)
                return new List<TreeNode>();
            
            List<TreeNode> nodeList = new List<TreeNode>();

            Type dataObjType = rootNode.Target.GetType();

            if (rootNode.Target is GameObject go)
            {
                if (go.TryGetComponent(out IOnionDataEditorGameObjectAgent gameobjectAgent) == false)
                {
                    gameobjectAgent = new DefaultOnionDataEditorGameObjectAgent();
                }

                IEnumerable<TreeNode> nodes = gameobjectAgent.GetNodes(go);

                rootNode.OnInspectorAction = new OnionAction(() =>
                {
                    if (rootNode != null)
                    {
                        gameobjectAgent.OnInspectorGUI(rootNode);
                    }
                });

                nodeList.AddRange(nodes);
            }
            else
            {
                if (memberCache.TryGetValue(dataObjType, out MemberInfo[] members) == false)
                {
                    members = dataObjType.GetMembers(defaultBindingFlags);
                    memberCache.Add(dataObjType, members);
                }

                foreach (var member in members)
                {
                    foreach (var attrType in nodeAttrTypeList)
                    {
                        foreach (Attribute attr in member.GetCustomAttributes(attrType, true))
                        {
                            if (attr != null)
                                nodeList.AddRange(GetChildNodeWithAttribute(rootNode.Target, member, attr));
                        }
                    }
                }
            }

            return nodeList;

        }

        /// <summary>Auto create nodes tree under the target node.</summary>
        public static void GetElementTree(this TreeNode targetNode, int depth = 0)
        {
            if (depth > 255)
            {
                EditorWindow.GetWindow<OnionDataEditorWindow>().Close();
                throw new StackOverflowException($"{targetNode.displayName} may have infinite reference loop.");
            }

            //if (ReferenceCheck(targetNode) == false)
            //{
            //    EditorWindow.GetWindow<OnionDataEditorWindow>().Close();
            //    throw new StackOverflowException($"{targetNode.displayName} is a parent of itself.");
            //}

            var node = targetNode.GetElements();

            targetNode.ClearChildren();
            targetNode.AddChildren(node);

            foreach (var el in node)
            {
                if (el.IsHideElementNodes == false)
                {
                    el.GetElementTree(depth + 1);
                }
            }

            //檢查是否無限循環參照
            //bool ReferenceCheck(TreeNode n)
            //{
            //    TreeNode checkNode = n;
            //    while (checkNode.Parent != null)
            //    {
            //        checkNode = checkNode.Parent;
            //        if (n.Target == checkNode.Target)
            //            return false;
            //    }
            //    return true;
            //}

        }

        internal static void CreateTreeView(this TreeNode node, out DataObjTreeView treeView)
        {
            if (node == null)
            {
                treeView = null;
                return;
            }

            node.GetElementTree();

            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();

            if (window.treeViewState == null)
                treeView = new DataObjTreeView(node, new TreeViewState());
            else
                treeView = new DataObjTreeView(node, window.treeViewState);

        }


        #region 取得target身上的特定屬性Attribute

        //取得屬性值的通用方法
        static T TryGetTargetAttrValue<T>(Object target, Type attrType) where T : class
        {
            Type dataObjType = target.GetType();

            if (memberCache.TryGetValue(dataObjType, out MemberInfo[] members) == false)
            {
                members = dataObjType.GetMembers(defaultBindingFlags);
                memberCache.Add(dataObjType, members);
            }

            foreach (var member in members)
            {
                var key = (dataObjType, member.ReflectedType, member.Name, attrType);
                if (attrResultCache.TryGetValue(key, out bool attrResult) == false)
                {
                    //Debug.Log(key);
                    //重找一次這個member有沒有這個attribut，並記錄結果
                    if (member.GetCustomAttribute(attrType, true) != null)
                        attrResult = true;
                    else
                        attrResult = false;

                    attrResultCache.Add(key, attrResult);
                }

                if (attrResult == true)
                {
                    var r = member.TryGetValue<T>(target);
                    if (r.hasValue)
                        return r.value as T;
                }
            }

            return null;
        }

        static T GetTargetAttrValue<T>(Object target, Type attrType, T defaultValue) where T : struct
        {
            Type dataObjType = target.GetType();

            if (memberCache.TryGetValue(dataObjType, out MemberInfo[] members) == false)
            {
                members = dataObjType.GetMembers(defaultBindingFlags);
                memberCache.Add(dataObjType, members);
            }

            foreach (var member in members)
            {
                var key = (dataObjType, member.ReflectedType, member.Name, attrType);
                if (attrResultCache.TryGetValue(key, out bool attrResult) == false)
                {
                    //Debug.Log(key);
                    //重找一次這個member有沒有這個attribut，並記錄結果
                    if (member.GetCustomAttribute(attrType, true) != null)
                        attrResult = true;
                    else
                        attrResult = false;

                    attrResultCache.Add(key, attrResult);
                }

                if (attrResult == true)
                {
                    T r = member.GetValue<T>(target);
                    return r;
                }
            }

            return defaultValue;
        }



        readonly static Dictionary<string, string> extensionIconQuery = new Dictionary<string, string>
        {
            [".cs"] = "cs Script Icon",
            [".prefab"] = "Prefab Icon",
            [".assembly"] = "AssemblyDefinitionAsset Icon",
            [".uxml"] = "UxmlScript Icon",
            [".uss"] = "UssScript Icon",
        };


        internal static string GetTargetTitle(this Object target)
        {
            var result = TryGetTargetAttrValue<string>(target, typeof(NodeTitleAttribute));
            return result;
        }

        internal static string GetTargetDescription(this Object target)
        {
            var result = TryGetTargetAttrValue<string>(target, typeof(NodeDescriptionAttribute));
            return result;
        }

        internal static Texture GetTargetIcon(this Object target)
        {
            //Try get texture
            var textureResult = TryGetTargetAttrValue<Texture>(target, typeof(NodeIconAttribute));
            if (textureResult != null)
            {
                return textureResult;
            }

            //Try get sprite
            var spriteResult = TryGetTargetAttrValue<Sprite>(target, typeof(NodeIconAttribute));
            if (spriteResult != null)
            {
                return spriteResult.texture;
            }

            //Custom extension
            if (AssetDatabase.IsMainAsset(target))
            {
                string path = AssetDatabase.GetAssetPath(target);
                string extension = Path.GetExtension(path);

                //Is folder?
                if (string.IsNullOrEmpty(extension))
                {
                    bool isDarkTheme = EditorGUIUtility.isProSkin;
                    return EditorGUIUtility.IconContent((isDarkTheme ? "d_" : "") + "Folder Icon").image;
                }
                else if(extensionIconQuery.TryGetValue(extension, out string iconName))
                {
                    //bool isDarkTheme = EditorGUIUtility.isProSkin;
                    //var icon = EditorGUIUtility.IconContent((isDarkTheme ? "d_" : "") + iconName);

                    return EditorGUIUtility.IconContent(iconName).image;
                }
            }

            //Use object default icon
            var defaultResult = EditorGUIUtility.ObjectContent(null, target.GetType()).image;

            if(defaultResult.name == "d_DefaultAsset Icon" || defaultResult.name == "DefaultAsset Icon")
            {
                return null;
            }
            else
            {
                return defaultResult;
            }

        }

        internal static Color GetTargetTagColor(this Object target)
        {
            var result = GetTargetAttrValue(target, typeof(NodeColorTagAttribute), new Color(0, 0, 0, 0));
            return result;
        }

        internal static IEnumerable<OnionAction> GetTargetActions(this Object target)
        {
            IEnumerable<OnionAction> result = null;
            if (target != null)
            {
                var type = target.GetType();
                result = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeActionAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeActionAttribute>()))
                    .Where(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(OnionDataEditor.Setting.userTags).Any())
                    .Select(_ => new OnionAction(_.methodInfo, target, _.attr.actionName ?? _.methodInfo.Name));

            }
            return result;
        }

        internal static OnionAction GetTargetOnSelectedAction(this Object target)
        {
            if (target != null)
            {
                var type = target.GetType();
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnSelectedAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeOnSelectedAttribute>()))
                    .SingleOrDefault(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(OnionDataEditor.Setting.userTags).Any())
                    .methodInfo;

                if (method != null)
                    return new OnionAction(method, target, method.Name);
            }
            return null;
        }

        internal static OnionAction GetTargetOnDoubleClickAction(this Object target)
        {
            if (target != null)
            {
                var type = target.GetType();
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnDoubleClickAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeOnDoubleClickAttribute>()))

                    .SingleOrDefault(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(OnionDataEditor.Setting.userTags).Any())
                    .methodInfo;

                if (method != null)
                    return new OnionAction(method, target, method.Name);
            }
            return null;
        }


        #endregion


        #region Dependent

        /// <summary>取得MemberInfo的值。</summary>
        internal static T GetValue<T>(this MemberInfo memberInfo, object forObject) where T : new()
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

            T resultValue = (T)tempValue;

            return resultValue;
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

        #endregion
    }
}

#endif