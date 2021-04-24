using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Display button that could execute custom function in data editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeActionAttribute : Attribute
    {
        public string actionName = null;

        /// <summary>
        /// NodeActionAttribute enable function for users by user tags.
        /// </summary>
        public string[] userTags = new string[0];

        public NodeActionAttribute() { }
        public NodeActionAttribute(string actionName)
        {
            this.actionName = actionName;
        }
    }
}
