using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Define nodes in data editor.
    /// Nodes will be grouped in a pseudo node.
    /// Only can attach on IEnumerable&lt;UnityEngine.Object&gt; and UnityEngine.Object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeGroupedElementAttribute : Attribute
    {
        public bool findTree;
        public bool hideIfEmpty;
        public string displayName;

        public NodeGroupedElementAttribute(string displayName, bool findTree = false, bool hideIfEmpty = false)
        {
            this.displayName = displayName;
            this.findTree = findTree;
            this.hideIfEmpty = hideIfEmpty;
        }

    }
}