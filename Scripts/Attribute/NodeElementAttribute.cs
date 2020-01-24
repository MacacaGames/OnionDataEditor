using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中會被視為一個節點，只能掛於IEnumerable&lt;UnityEngine.Object&gt;、UnityEngine.Object上。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeElementAttribute : Attribute
    {
    }
}
