
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
        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        readonly static List<Type> nodeAttrTypeList = new List<Type>
        {
            typeof(NodeElementAttribute),
            typeof(NodeGroupedElementAttribute),
            typeof(NodeCustomElementAttribute),
            //++
        };

        public static IEnumerable<TreeNode> GetElements(this Object dataObj)
        {
            if (dataObj != null)
            {
                List<TreeNode> nodeList = new List<TreeNode>();

                var members = dataObj.GetType().GetMembers(defaultBindingFlags);

                foreach(var member in members)
                {
                    foreach (var attrType in nodeAttrTypeList)
                    {
                        foreach (Attribute attr in member.GetCustomAttributes(attrType,true))
                        {
                            if (attr != null)
                                nodeList.AddRange(GetChildNodeWithAttribute(dataObj, member, attr));
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
                TreeNode groupedNode = groupAttr.rootNode;

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

            throw new Exception($"Unknown Attribute {dataObj.name}.{member.Name}(Attr:{attr.ToString()})");


            //取得指定型別的T或IEnumerable<T>
            IEnumerable<T> GetSingleOrMultipleType<T>() where T : class
            {
                Type memberType = member.ReflectedType;

                //Single
                if (memberType == typeof(T) || memberType.IsSubclassOf(typeof(T)))
                {
                    if (member.TryGetValue(dataObj, out T resultCustomSingle))
                    {
                        Debug.Log(resultCustomSingle.ToString());
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
        public static void GetElementTree(this TreeNode tagetNode)
        {
            if (ReferenceCheck(tagetNode) == false)
            {
                EditorWindow.GetWindow<OnionDataEditorWindow>().Close();
                throw new StackOverflowException($"{tagetNode.displayName} is a parent of itself.");
            }


            var node = GetElements(tagetNode.dataObj);

            tagetNode.ClearChildren();
            tagetNode.AddChildren(new List<TreeNode>(node));

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
            Type type = dataObj.GetType();
            var members = type.GetMembers(defaultBindingFlags);

            foreach (var member in members)
                if (member.GetCustomAttribute(attrType, true) != null)
                {
                    var result = member.TryGetValue<T>(dataObj);
                    if (result.hasValue)
                        return result.value as T;
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
                    .Select(_ => new OnionAction(_, dataObj, _.GetCustomAttribute<NodeActionAttribute>().actionName ?? _.Name))
                    .ToList();

            }
            return result;
        }
        public static OnionAction GetNodeOnSelectedAction(this Object dataObj)
        {
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnSelectedAttribute)).SingleOrDefault(_ => _.GetGenericArguments().Length == 0);
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
                var method = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(NodeOnDoubleClickAttribute)).SingleOrDefault(_ => _.GetGenericArguments().Length == 0);
                if (method != null)
                    return new OnionAction(method, dataObj, method.Name);
            }
            return null;
        }

        #endregion
    }
}

#endif