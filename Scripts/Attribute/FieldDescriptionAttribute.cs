using System;

namespace OnionCollections.DataEditor
{
    /// <summary>說明欄位功能。</summary>
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
