using System;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Asset default open with data editor if set true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OpenWithOnionDataEditor : Attribute
    {
        public bool openWithDataEditor { get; private set; }
        public OpenWithOnionDataEditor(bool openWithDataEditor)
        {
            this.openWithDataEditor = openWithDataEditor;
        }
    }
}
