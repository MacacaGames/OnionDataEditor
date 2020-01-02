using System;
using OnionCollections.DataEditor.Editor;

#if (UNITY_EDITOR)

/// <summary>在OnionDataEditor中會被包裹為一個節點，此節點可以是任意自訂節點。</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class NodeGroupedElementAttribute : Attribute
{
    public TreeNode rootNode;
    public bool findTree;
    public bool hideIfEmpty;

    public NodeGroupedElementAttribute(string displayName)
    {
        rootNode = new TreeNode(TreeNode.NodeFlag.Pseudo)
        {
            displayName = displayName,
        };
    }

    public NodeGroupedElementAttribute(TreeNode rootNode)
    {
        this.rootNode = rootNode;
    }
}

#else

public class NodeGroupedElementAttribute : Attribute
{
    public TreeNode rootNode;
    public bool findTree;
    public bool hideIfEmpty;

    public NodeGroupedElementAttribute(string displayName, bool findTree = false, bool hideIfEmpty = false)
    {
    }

    public NodeGroupedElementAttribute(TreeNode rootNode)
    {
    }
}

#endif 
