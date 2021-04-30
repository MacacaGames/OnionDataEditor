using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Asset default open with data editor if set true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OpenWithOnionDataEditorAttribute : Attribute
    {
        public bool openWithDataEditor { get; private set; }
        public OpenWithOnionDataEditorAttribute(bool openWithDataEditor)
        {
            this.openWithDataEditor = openWithDataEditor;
        }
    }
}
