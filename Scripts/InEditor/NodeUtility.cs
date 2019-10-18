
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

                var elList =  members.FilterWithAttribute(typeof(OnionCollections.DataEditor.NodeElementAttribute)).ToList();
                if (elList.Count > 0)
                {

                    foreach (var member in elList)
                    {
                        if (member.TryGetValue(dataObj, out ScriptableObject result_single))
                            nodeList.Add(new TreeNode(result_single));
                        else if (member.TryGetValue(dataObj, out IEnumerable<ScriptableObject> result_ienumerable))
                            nodeList.AddRange(result_ienumerable.Select(_ => new TreeNode(_)));
                    }

                }
                var pseudoList = members.FilterWithAttribute(typeof(NodePseudoElementAttribute)).ToList();
                if (pseudoList.Count > 0)
                {
                    foreach (var member in pseudoList)
                    {
                        if (member.GetMemberInfoType() == typeof(TreeNode))
                        {
                            if (member.TryGetValue(dataObj, out TreeNode result_pseudo))
                            {
                                if (result_pseudo.isPseudo == false)
                                    throw new System.NotImplementedException("This node must be pseudo.");
                                nodeList.Add(result_pseudo);
                            }
                        }
                        else if(member.GetMemberInfoType() == typeof(IEnumerable<TreeNode>))
                        {
                            if (member.TryGetValue(dataObj, out IEnumerable<TreeNode> result_pseudo))
                            {
                                foreach(var pseudoNode in result_pseudo)
                                    if (pseudoNode.isPseudo == false)
                                        throw new System.NotImplementedException("This node must be pseudo.");
                                nodeList.AddRange(result_pseudo);
                            }
                        }
                    }
                }

                return nodeList;
            }

            return null;
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