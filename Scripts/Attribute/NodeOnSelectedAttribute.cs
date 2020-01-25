using System;

namespace OnionCollections.DataEditor.Editor
{
    /// <summary>在OnionDataEditor中選擇後，被選擇的物件所要執行的動作，一個class中只能有一個。</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeOnSelectedAttribute : Attribute
    {
        public string[] userTags = new string[0];
    }
}
