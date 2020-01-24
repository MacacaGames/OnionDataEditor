using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>在OnionDataEditor中定義此節點底下任意自訂節點，只能掛於IEnumerable&lt;TreeNode&gt;、TreeNode上。</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NodeCustomElementAttribute : Attribute
    {

    }
}