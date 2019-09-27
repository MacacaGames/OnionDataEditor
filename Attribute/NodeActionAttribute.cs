using System;

namespace Onion
{
    /// <summary>在OnionDataEditor中作為選擇資料時可執行的快捷動作按鈕。</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeActionAttribute : Attribute
    {
        public string actionName = null;
        public NodeActionAttribute() { }
        public NodeActionAttribute(string actionName)
        {
            this.actionName = actionName;
        }
    }
}
