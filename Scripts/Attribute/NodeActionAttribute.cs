using System;

namespace OnionCollections.DataEditor
{
    /// <summary>在OnionDataEditor中作為選擇資料時可執行的快捷動作按鈕，只能掛於void的Method上。</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeActionAttribute : Attribute
    {
        public string actionName = null;

        public string[] userTags = new string[0];

        public NodeActionAttribute() { }
        public NodeActionAttribute(string actionName)
        {
            this.actionName = actionName;
        }
    }
}
