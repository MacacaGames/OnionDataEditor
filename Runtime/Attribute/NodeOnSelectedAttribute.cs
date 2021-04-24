using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>
    /// If node be selected, will execute this function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeOnSelectedAttribute : Attribute
    {
        public string[] userTags = new string[0];
    }
}
