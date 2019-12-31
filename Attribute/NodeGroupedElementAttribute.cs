using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>在OnionDataEditor中會被包裹為一個節點，此節點可以是任意自訂節點。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeGroupedElementAttribute : Attribute
    {
        public TreeNode rootNode;
        public bool findTree;
        
        public NodeGroupedElementAttribute(string displayName, bool findTree = false)
        {
            this.findTree = findTree;

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
}
