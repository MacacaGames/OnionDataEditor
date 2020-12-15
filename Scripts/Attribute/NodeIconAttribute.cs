using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Define this node's icon in data editor.
    /// Only can attach on UnityEngine.Texture and UnityEngine.Sprite.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeIconAttribute : Attribute
    {

    }
}
