using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中會被視為一個節點，此節點會包裹一層偽節點。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeGroupedElementAttribute : Attribute
    {
        public string displayName;
        public bool findTree;

        public NodeGroupedElementAttribute(string displayName, bool findTree = false)
        {
            this.displayName = displayName;
            this.findTree = findTree;
        }
    }
}
