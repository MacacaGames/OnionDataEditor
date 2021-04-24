using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>Define custom node in data editor. Only can attach on IEnumerable&lt;TreeNode&gt; and TreeNode.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NodeCustomElementAttribute : Attribute
    {

    }
}