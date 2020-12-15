using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Display custom field description in data editor.
    /// </summary>
    [Obsolete]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FieldDescriptionAttribute : Attribute
    {
        public string description { get; private set; }
        public FieldDescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
}
