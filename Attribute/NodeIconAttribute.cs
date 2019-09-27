using System;

namespace Onion
{
    /// <summary>在OnionDataEditor中標記為此節點的圖示，一個class中只能有一個。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeIconAttribute : Attribute
    {

    }
}
