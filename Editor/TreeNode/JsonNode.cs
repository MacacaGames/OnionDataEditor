using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    public class JsonNode : TreeNode
    {
        public enum JsonNodeType
        {
            None = 0,
            Object = 1,
            Array = 2,
            String = 3,
            Int = 4,
            Float = 5,
            Bool = 6,
        }

        public JsonNodeType JsonType { get; private set; } = JsonNodeType.None;

        public int Count => ChildCount;

        public List<string > Keys { get; private set; }




        public string str { get; private set; }
        public int i { get; private set; }
        public float f { get; private set; }
        public bool b { get; private set; }

        public void Set(string str)
        {
            JsonType = JsonNodeType.String;
            this.str = str;
        }

        public void Set(int i)
        {
            JsonType = JsonNodeType.Int;
            this.i = i;
        }

        public void Set(float f)
        {
            JsonType = JsonNodeType.Float;
            this.f = f;
        }

        public void Set(bool b)
        {
            JsonType = JsonNodeType.Bool;
            this.b = b;
        }

        public void AddProperty(string propName, JsonNode node)
        {
            JsonType = JsonNodeType.Object;
            Keys.Add(propName);
            AddSingleChild(node);
        }

        public void AddItem(JsonNode node)
        {
            JsonType = JsonNodeType.Array;
            AddSingleChild(node);
        }


    }
}