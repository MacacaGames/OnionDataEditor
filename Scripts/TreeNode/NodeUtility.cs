
#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

using Object = UnityEngine.Object;

namespace OnionCollections.DataEditor.Editor
{
    public static class NodeUtility
    {
        static OnionSetting setting => OnionDataEditor.setting;



        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        readonly static List<Type> nodeAttrTypeList = new List<Type>
        {
            typeof(NodeElementAttribute),
            typeof(NodeGroupedElementAttribute),
            typeof(NodeCustomElementAttribute),
            //++
        };

        static Dictionary<Type, MemberInfo[]> memberCache = new Dictionary<Type, MemberInfo[]>();
        static Dictionary<(Type objectType, Type memberType, string memberName, Type attr), bool> attrResultCache = new Dictionary<(Type objectType, Type memberType, string memberName, Type attr), bool>();

        internal static List<TreeNode> GetElements(this TreeNode rootNode)
        {
            if (rootNode.isPseudo == true)
                return rootNode.GetChildren().ToList();

            if (rootNode.isNull == true)
                return new List<TreeNode>();
            
            List<TreeNode> nodeList = new List<TreeNode>();

            Type dataObjType = rootNode.dataObj.GetType();

            if (rootNode.dataObj is GameObject go)
            {
                if (go.TryGetComponent(out IOnionDataEditorGameObjectAgent gameobjectAgent) == false)
                {
                    gameobjectAgent = new DefaultOnionDataEditorGameObjectAgent();
                }

                IEnumerable<TreeNode> nodes = gameobjectAgent.GetNodes(go);

                rootNode.onInspectorAction = new OnionAction(() =>
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
                                nodeList.AddRange(GetChildNodeWithAttribute(rootNode.dataObj, member, attr));
                        }
                    }
                }
            }

            return nodeList;

        }

        static IEnumerable<TreeNode> GetChildNodeWithAttribute(Object dataObj, MemberInfo member, Attribute attr)
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
                if ((groupAttr.hideIfEmpty == true && groupedNode.childCount == 0) == false)
                    return new List<TreeNode> { groupedNode };
                
                return Enumerable.Empty<TreeNode>();
            }

            //NodeCustomElement
            if (attr is NodeCustomElementAttribute)
            {
                return GetSingleOrMultipleType<TreeNode>();
            }

            throw new Exception($"Unknown Attribute {dataObj.name}.{member.Name}(Attr:{attr})");


            //取得指定型別的T或IEnumerable<T>
            IEnumerable<T> GetSingleOrMultipleType<T>() where T : class
            {
                Type memberType = member.GetMemberInfoType();

                //Single
                if (memberType == typeof(T) || memberType.IsSubclassOf(typeof(T)))
                {
                    if (member.TryGetValue(dataObj, out T resultCustomSingle))
                    {
                        return new List<T> { resultCustomSingle };
                    }
                }
                //Multiple
                if (typeof(IEnumerable).IsAssignableFrom(memberType))
                {
                    if (member.TryGetValue(dataObj, out IEnumerable<T> resultCustom))
                        return resultCustom;
                }

                return Enumerable.Empty<T>();
            }

        }


        /// <summary>
        /// Auto create nodes tree under the target node.
        /// </summary>
        public static void GetElementTree(this TreeNode targetNode, int depth = 0)
        {
            if(depth > 255)
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
                if (el.isHideElementNodes == false)
                {
                    el.GetElementTree(depth + 1);
                }
            }

            //檢查是否無限循環參照
            bool ReferenceCheck(TreeNode n)
            {
                TreeNode checkNode = n;
                while (checkNode.parent != null)
                {
                    checkNode = checkNode.parent;
                    if (n.dataObj == checkNode.dataObj)
                        return false;
                }
                return true;
            }

        }


        #region 取得target身上的特定屬性Attribute

        //取得屬性值的通用方法
        static T TryGetNodeAttrValue<T>(Object dataObj, Type attrType) where T : class
        {
            Type dataObjType = dataObj.GetType();

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
                    var r = member.TryGetValue<T>(dataObj);
                    if (r.hasValue)
                        return r.value as T;
                }
            }

            return null;
        }

        static T GetNodeAttrValue<T>(Object dataObj, Type attrType, T defaultValue) where T : struct
        {
            Type dataObjType = dataObj.GetType();

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
                    T r = member.GetValue<T>(dataObj);
                    return r;
                }
            }

            return defaultValue;
        }

        internal static string GetNodeTitle(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeTitleAttribute));
            return result;
        }
        internal static string GetNodeDescription(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeDescriptionAttribute));
            return result;
        }
        internal static Texture GetNodeIcon(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<Texture>(dataObj, typeof(NodeIconAttribute));
            if (result == null)
            {
                var s = TryGetNodeAttrValue<Sprite>(dataObj, typeof(NodeIconAttribute));
                if (s) result = s.texture;
            }
            return result;
        }
        internal static Color GetNodeTagColor(this Object dataObj)
        {
            var result = GetNodeAttrValue(dataObj, typeof(NodeColorTagAttribute), new Color(0, 0, 0, 0));
            return result;
        }

        internal static IEnumerable<OnionAction> GetNodeActions(this Object dataObj)
        {
            IEnumerable<OnionAction> result = null;
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                result = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeActionAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeActionAttribute>()))
                    .Where(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(setting.userTags).Any())
                    .Select(_ => new OnionAction(_.methodInfo, dataObj, _.attr.actionName ?? _.methodInfo.Name));

            }
            return result;
        }
        internal static OnionAction GetNodeOnSelectedAction(this Object dataObj)
        {
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnSelectedAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeOnSelectedAttribute>()))
                    .SingleOrDefault(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(setting.userTags).Any())
                    .methodInfo;

                if (method != null)
                    return new OnionAction(method, dataObj, method.Name);
            }
            return null;
        }
        internal static OnionAction GetNodeOnDoubleClickAction(this Object dataObj)
        {
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnDoubleClickAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ => (methodInfo: _, attr: _.GetCustomAttribute<NodeOnDoubleClickAttribute>()))

                    .SingleOrDefault(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(setting.userTags).Any())
                    .methodInfo;

                if (method != null)
                    return new OnionAction(method, dataObj, method.Name);
            }
            return null;
        }

        #endregion
    }
}

#endif