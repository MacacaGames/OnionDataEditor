
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

        public static IEnumerable<TreeNode> GetElements(this Object dataObj)
        {
            if (dataObj != null)
            {
                List<TreeNode> nodeList = new List<TreeNode>();

                Type dataObjType = dataObj.GetType();

                if (dataObj is GameObject go)
                {
                    IEnumerable<TreeNode> nodes;
                    if (go.TryGetComponent(out OnionDataEditorGameObjectAgent gameobjectAgent) == true)
                    {
                        nodes = gameobjectAgent.GetNodes(go);
                    }
                    else
                    {
                        nodes = go.GetComponents<Component>()
                            .Select(c =>
                            {
                                string displayName = (c is IQueryableData q) ? q.GetID() : c.GetType().Name;

                                return new TreeNode(c)
                                {
                                    displayName = displayName
                                };
                            });
                    }

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
                                    nodeList.AddRange(GetChildNodeWithAttribute(dataObj, member, attr));
                            }
                        }
                    }
                }

                return nodeList;
            }

            return null;
        }

        static IEnumerable<TreeNode> GetChildNodeWithAttribute(Object dataObj, MemberInfo member, Attribute attr)
        { 
            //NodeElement
            if (attr.GetType() == typeof(NodeElementAttribute))
            {
                return
                    GetSingleOrMultipleType<Object>()
                    .Select(_ => new TreeNode(_));
            }

            //NodeGroupedElement
            if (attr.GetType() == typeof(NodeGroupedElementAttribute))
            {
                NodeGroupedElementAttribute groupAttr = attr as NodeGroupedElementAttribute;
                TreeNode groupedNode = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = groupAttr.displayName,
                };

                List<TreeNode> node = GetSingleOrMultipleType<Object>()
                    .Select(_ => new TreeNode(_))
                    .ToList();

                //若需要FindTree，則遍歷底下節點找
                if (groupAttr.findTree)
                    foreach (var item in node)
                        item.GetElementTree();
                        
                groupedNode.AddChildren(node);

                //如果Element是Empty則不加入Group
                if ((groupAttr.hideIfEmpty == true && groupedNode.childCount == 0) == false)
                    return new List<TreeNode> { groupedNode };
                
                return new List<TreeNode> { };
            }

            //NodeCustomElement
            if (attr.GetType() == typeof(NodeCustomElementAttribute))
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

                return new List<T> { };
            }

        }


        /// <summary>將特定TreeNode長出其下子節點。</summary>
        public static void GetElementTree(this TreeNode targetNode)
        {
            if (ReferenceCheck(targetNode) == false)
            {
                EditorWindow.GetWindow<OnionDataEditorWindow>().Close();
                throw new StackOverflowException($"{targetNode.displayName} is a parent of itself.");
            }


            var node = GetElements(targetNode.dataObj);

            targetNode.ClearChildren();
            targetNode.AddChildren(new List<TreeNode>(node));

            foreach (var el in node)
            {
                if (el.dataObj != null && 
                    el.isHideElementNodes == false)
                {
                    el.GetElementTree();
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
    
        public static string GetNodeTitle(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeTitleAttribute));
            return result;
        }
        public static string GetNodeDescription(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeDescriptionAttribute));
            return result;
        }
        public static Texture GetNodeIcon(this Object dataObj)
        {
            var result = TryGetNodeAttrValue<Texture>(dataObj, typeof(NodeIconAttribute));
            if (result == null)
            {
                var s = TryGetNodeAttrValue<Sprite>(dataObj, typeof(NodeIconAttribute));
                if (s) result = s.texture;
            }
            return result;
        }

        public static IEnumerable<OnionAction> GetNodeActions(this Object dataObj)
        {
            List<OnionAction> result = new List<OnionAction>();
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                result = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeActionAttribute))
                    .Where(_ => _.GetGenericArguments().Length == 0)
                    .Select(_ =>(methodInfo: _, attr:_.GetCustomAttribute<NodeActionAttribute>()))
                    .Where(_ => _.attr.userTags.Length == 0 || _.attr.userTags.Intersect(setting.userTags).Any())
                    .Select(_ => new OnionAction(_.methodInfo, dataObj, _.attr.actionName ?? _.methodInfo.Name))
                    .ToList();

            }
            return result;
        }
        public static OnionAction GetNodeOnSelectedAction(this Object dataObj)
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
        public static OnionAction GetNodeOnDoubleClickAction(this Object dataObj)
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