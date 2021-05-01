using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;
using System;

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

        public void ImportFromJsonData(JsonData jsonDataPointer, Action<JsonNode> onBind)
        {
            ImportFromJsonDataExtension(this, jsonDataPointer, onBind);
        }

        static void ImportFromJsonDataExtension(JsonNode jsonNode, JsonData jsonData, Action<JsonNode> onBind)
        {
            //Object
            if (jsonData.IsObject)
            {
                BuildObject();
            }

            //Array
            if (jsonData.IsArray)
            {
                BuildArray();
            }

            //Field - String
            if (jsonData.IsString)
            {
                BuildString();
            }

            //Field - Int
            if (jsonData.IsLong || jsonData.IsInt)
            {
                BuildInt();
            }

            //Field - Float
            if (jsonData.IsDouble)
            {
                BuildFloat();
            }

            //Field - Bool
            if (jsonData.IsBoolean)
            {
                BuildBool();
            }
            var n = jsonNode;

            onBind(n);


            void BuildArray()
            {
                jsonNode.JsonType = JsonNodeType.Array;
                for (int i = 0; i < jsonData.Count; i++)
                {
                    var j = jsonData[i];
                    var ch = new JsonNode();
                    jsonNode.AddItem(ch);

                    ch.ImportFromJsonData(j, onBind);
                }

                jsonNode.UpdateItemIndexDisplayName();
            }

            void BuildObject()
            {
                jsonNode.JsonType = JsonNodeType.Object;
                foreach (string key in jsonData.Keys)
                {
                    var j = jsonData[key];

                    var ch = new JsonNode()
                    {
                        displayName = key
                    };
                    jsonNode.AddProperty(key, ch);

                    ch.ImportFromJsonData(j, onBind);
                }
            }

            void BuildInt()
            {
                jsonNode.Set((int)jsonData);
            }

            void BuildString()
            {
                jsonNode.Set((string)jsonData);
            }

            void BuildFloat()
            {
                jsonNode.Set((float)jsonData);
            }

            void BuildBool()
            {
                jsonNode.Set((bool)jsonData);
            }


        }

        public JsonData ExportToJsonData()
        {
            return ExportToJsonDataExtension(this);
        }

        static JsonData ExportToJsonDataExtension(JsonNode jsonNode)
        {
            switch (jsonNode.JsonType)
            {
                case JsonNodeType.Object:

                    var objData = new JsonData();
                    objData.SetJsonType(LitJson.JsonType.Object);
                    if (jsonNode.Keys.Count > 0)
                    {
                        for (int i = 0; i < jsonNode.Keys.Count; i++)
                        {
                            string key = jsonNode.Keys[i];
                            JsonData value = ExportToJsonDataExtension(jsonNode.children[i] as JsonNode);
                            objData[key] = value;
                        }
                    }
                    return objData;


                case JsonNodeType.Array:

                    var arrayData = new JsonData();
                    arrayData.SetJsonType(LitJson.JsonType.Array);
                    if (jsonNode.ChildCount > 0)
                    {
                        for (int i = 0; i < jsonNode.ChildCount; i++)
                        {
                            JsonData value = ExportToJsonDataExtension(jsonNode.children[i] as JsonNode);
                            arrayData.Add(value);
                        }
                    }
                    return arrayData;


                case JsonNodeType.String:
                    return new JsonData(jsonNode.str);


                case JsonNodeType.Int:
                    return new JsonData(jsonNode.i);

                case JsonNodeType.Float:
                    return new JsonData(jsonNode.f);

                case JsonNodeType.Bool:
                    return new JsonData(jsonNode.b);


                default:
                    throw new Exception($"Unknown json data type in {jsonNode.displayName}");
            }
        }



    }
}