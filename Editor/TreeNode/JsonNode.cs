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
            Null = 0,
            Object = 1,
            Array = 2,
            String = 3,
            Number = 4,
            Bool = 5,
        }

        public JsonNodeType JsonType { get; set; } = JsonNodeType.Null;

        public int Count => ChildCount;

        public List<string> Keys { get; private set; } = new List<string>();


        public bool isExpend = true;

        public string str { get; private set; }
        public float f { get; private set; }
        public float n => f;
        public bool b { get; private set; }

        public void Set(string str)
        {
            JsonType = JsonNodeType.String;
            this.str = str;
        }


        public void Set(float f)
        {
            JsonType = JsonNodeType.Number;
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

            for (int i = 0; i < ChildCount; i++)
            {
                children[i].displayName = $"[ {i} ]";
            }
        }
        public void UpdatePropertyDisplayName()
        {
            if (JsonType != JsonNodeType.Object)
                return;

            for (int i = 0; i < ChildCount; i++)
            {
                children[i].displayName = Keys[i];
            }
        }

        //



        public void DrawFieldGUI(Rect rect)
        {
            switch (JsonType)
            {
                case JsonNodeType.Bool:
                    bool b = EditorGUI.Toggle(rect, this.b);
                    Set(b);
                    break;


                case JsonNodeType.Number:
                    float f = EditorGUI.FloatField(rect, this.f);
                    Set(f);
                    break;

                case JsonNodeType.String:
                    string str = EditorGUI.TextField(rect, this.str);
                    Set(str);
                    break;

                case JsonNodeType.Null:
                    EditorGUI.LabelField(rect, $"null", new GUIStyle(EditorStyles.helpBox) { stretchWidth = false });
                    break;


                default:
                    EditorGUI.LabelField(rect, displayName);
                    break;

            }
        }


    }
}