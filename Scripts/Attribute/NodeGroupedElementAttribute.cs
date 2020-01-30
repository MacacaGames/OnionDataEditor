using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中會被包裹到一個自訂節點，只能掛於IEnumerable&lt;UnityEngine.Object&gt;、UnityEngine.Object上。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeGroupedElementAttribute : Attribute
    {
        public bool findTree;
        public bool hideIfEmpty;
        public string displayName;

        public NodeGroupedElementAttribute(string displayName, bool findTree = false, bool hideIfEmpty = false)
        {
            this.displayName = displayName;
        }

    }
}