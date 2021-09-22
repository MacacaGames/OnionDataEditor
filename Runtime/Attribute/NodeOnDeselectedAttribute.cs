using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>
    /// If the node has been deselected, this function will execute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeOnDeselectedAttribute : Attribute
    {
        public string[] userTags = new string[0];
    }
}
