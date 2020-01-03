
#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

using System.Reflection;
using OnionCollections;

namespace OnionCollections.DataEditor.Editor
{

    public static class NodeUtility
    {
        const BindingFlags defaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        public static IEnumerable<TreeNode> GetElements(this ScriptableObject dataObj)
        {
            if (dataObj != null)
            {
                List<TreeNode> nodeList = new List<TreeNode>();

                var members = dataObj.GetType().GetMembers(defaultBindingFlags);

                foreach(var member in members)
                {
                    List<Type> attrTypes = new List<Type>
                    {
                        typeof(NodeElementAttribute),
                        typeof(NodeGroupedElementAttribute),
                        typeof(NodeCustomElementAttribute),
                        //++
                    };

                    foreach (var attrType in attrTypes)
                    {
                        Attribute attr = member.GetCustomAttribute(attrType);
                        if (attr != null)
                            nodeList.AddRange(GetChildNodeWithAttribute(dataObj, member, attr));
                    }
                }

                return nodeList;
            }

            return null;
        }

        static IEnumerable<TreeNode> GetChildNodeWithAttribute(ScriptableObject dataObj, MemberInfo member, Attribute attr)
        {
            List<TreeNode> result = new List<TreeNode>();


            //NodeElement
            if (attr.GetType() == typeof(NodeElementAttribute))
            {
                result.AddRange(
                    GetSingleOrMultipleType<ScriptableObject>(dataObj, member)
                    .Select(_ => new TreeNode(_))
                    );
            }

            //NodeGroupedElement
            else if (attr.GetType() == typeof(NodeGroupedElementAttribute))
            {
                NodeGroupedElementAttribute groupAttr = attr as NodeGroupedElementAttribute;
                TreeNode groupedNode = groupAttr.rootNode;

                List<TreeNode> node = new List<TreeNode>();

                node.AddRange(
                    GetSingleOrMultipleType<ScriptableObject>(dataObj, member)
                    .Select(_ => new TreeNode(_))
                    );
                
                
                //若需要FindTree，則遍歷底下節點找
                if (groupAttr.findTree)
                    foreach (var item in node)
                        item.GetElementTree();
                        
                groupedNode.nodes.AddRange(node);

                //如果Element是Empty則不加入Group
                if ((groupAttr.hideIfEmpty == true && groupedNode.nodes.Count == 0) == false)
                    result.Add(groupedNode);
            }

            //NodeCustomElement
            else if (attr.GetType() == typeof(NodeCustomElementAttribute))
            {
                result.AddRange(GetSingleOrMultipleType<TreeNode>(dataObj, member));
            }

            return result;
        }

        static IEnumerable<T> GetSingleOrMultipleType<T>(ScriptableObject dataObj, MemberInfo member) where T: class
        {
            if (member.TryGetValue(dataObj, out T resultCustomSingle))
                return new List<T> { resultCustomSingle };

            else if (member.TryGetValue(dataObj, out IEnumerable<T> resultCustom))
                return resultCustom;

            else
                return new List<T> { };
        }


        /// <summary>將特定TreeNode長出其下子節點。</summary>
        public static void GetElementTree(this TreeNode tagetNode)
        {
            if (ReferenceCheck(tagetNode) == false)
            {
                EditorWindow.GetWindow<OnionDataEditorWindow>().Close();
                throw new System.StackOverflowException($"{tagetNode.displayName} is a parent of itself.");
            }

            var node = GetElements(tagetNode.dataObj);

            tagetNode.nodes = new List<TreeNode>(node);

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
        
        static T TryGetNodeAttrValue<T>(ScriptableObject dataObj, Type attrType) where T : class
        {
            Type type = dataObj.GetType();
            var members = type.GetMembers(defaultBindingFlags);

            foreach (var member in members)
                if (member.GetCustomAttribute(attrType, true) != null)
                {
                    var result = ReflectionUtility.TryGetValue<T>(member, dataObj);
                    if (result.hasValue)
                        return result.value as T;
                }

            return null;
        }
    
        public static string GetNodeTitle(this ScriptableObject dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeTitleAttribute));
            return result as string;
        }
        public static string GetNodeDescription(this ScriptableObject dataObj)
        {
            var result = TryGetNodeAttrValue<string>(dataObj, typeof(NodeDescriptionAttribute));
            return result as string;
        }
        public static Texture GetNodeIcon(this ScriptableObject dataObj)
        {
            var result = TryGetNodeAttrValue<Texture>(dataObj, typeof(NodeIconAttribute));
            if (result == null)
            {
                var s = TryGetNodeAttrValue<Sprite>(dataObj, typeof(NodeIconAttribute));
                if (s) result = s.texture;
            }
            return result;
        }

        public static IEnumerable<OnionAction> GetNodeActions(this ScriptableObject dataObj)
        {
            List<OnionAction> result = new List<OnionAction>();
            if (dataObj != null)
            {
                var type = dataObj.GetType();
                var methods = type.GetMethods(defaultBindingFlags).FilterWithAttribute(typeof(OnionCollections.DataEditor.NodeActionAttribute)).ToList();
                foreach (var method in methods)
                {
                    if (method.GetGenericArguments().Length == 0)    //只接受沒有參數的Method
                    {
                        var attr = method.GetCustomAttribute<OnionCollections.DataEditor.NodeActionAttribute>();
                        result.Add(new OnionAction(method, dataObj, attr.actionName ?? method.Name));
                    }
                }
            }
            return result;
        }
        public static OnionAction GetNodeOnSelectedAction(this ScriptableObject dataObj)
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
        public static OnionAction GetNodeOnDoubleClickAction(this ScriptableObject dataObj)
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

    }
}

#endif