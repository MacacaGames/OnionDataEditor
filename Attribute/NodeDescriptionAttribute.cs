using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中標記為此節點的描述，一個class中只能有一個。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeDescriptionAttribute : Attribute
    {

    }
}
