using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    internal class JsonNode : TreeNode
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

        public JsonNodeType JsonType { get; set; } = JsonNodeType.None;

        public int Count => ChildCount;

        public List<string> Keys { get; private set; } = new List<string>();


        public bool isExpend = true;

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

        public void Remove(JsonNode node)
        {
            if (JsonType == JsonNodeType.Array)
            {
                children.Remove(node);
            }
            else if(JsonType == JsonNodeType.Object)
            {
                int index = children.IndexOf(node);
                if (index >= 0)
                {
                    children.RemoveAt(index);
                    Keys.RemoveAt(index);
                }
            }
        }

        public void UpdateItemIndexDisplayName()
        {
            if (JsonType != JsonNodeType.Array)
                return;

            for(int i = 0; i < ChildCount; i++)
            {
                children[i].displayName = $"[ {i} ]";
            }
        }

        //



        public void DrawFieldGUI()
        {
            switch (JsonType)
            {
                case JsonNodeType.Bool:
                    bool b = EditorGUILayout.Toggle(this.b);
                    Set(b);
                    break;

                case JsonNodeType.Int:
                    int i = EditorGUILayout.IntField(this.i);
                    Set(i);
                    break;

                case JsonNodeType.Float:
                    float f = EditorGUILayout.FloatField(this.f);
                    Set(f);
                    break;

                case JsonNodeType.String:
                    string str = EditorGUILayout.TextField(this.str);
                    Set(str);
                    break;

                default:
                    EditorGUILayout.LabelField(displayName);
                    break;

            }
        }


    }
}