using System;

namespace Onion
{
    /// <summary>在OnionDataEditor中雙擊後，被選擇的物件所要執行的動作，一個class中只能有一個。</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NodeOnDoubleClickAttribute : Attribute
    {

    }
}
