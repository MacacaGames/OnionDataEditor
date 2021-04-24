using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Display custom color tag in data editor.
    /// Default is transparent.
    /// Only can attach on Color.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeColorTagAttribute : Attribute
    {

    }
}
