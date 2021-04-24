using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Define child nodes in data editor.
    /// Only can attach on IEnumerable&lt;UnityEngine.Object&gt; and UnityEngine.Object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeElementAttribute : Attribute
    {

    }
}
