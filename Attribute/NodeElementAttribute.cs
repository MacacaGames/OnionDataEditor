using System;

namespace Onion
{
    /// <summary>在OnionDataEditor中會被視為一個節點，只能掛在Array或List上，一個class中只能有一個。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeElementAttribute : Attribute
    {

    }
}
