using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>
    /// If node be double clicked, will execute this function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeOnDoubleClickAttribute : Attribute
    {
        /// <summary>
        /// NodeOnDoubleClickAttribute enable function for users by user tags.
        /// </summary>
        public string[] userTags = new string[0];
    }
}
