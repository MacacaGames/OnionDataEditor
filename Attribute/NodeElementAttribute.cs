using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中會被視為一個節點。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeElementAttribute : Attribute
    {

    }
}
