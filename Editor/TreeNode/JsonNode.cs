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

        public JsonNodeType JsonType { get; private set; } = JsonNodeType.None;

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
                    var ch = new JsonNode()
                    {
                        displayName = $"[ {i} ]",
                    };
                    jsonNode.AddItem(ch);

                    ch.ImportFromJsonData(j, onBind);
                }

            }

            void BuildObject()
            {
                jsonNode.JsonType = JsonNodeType.Object;
                foreach (string key in jsonData.Keys)
                {
                    var j = jsonData[key];

                    var ch = new JsonNode()
                    {
                        displayName = key,
                        description = j.GetJsonType().ToString()
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
                    if (jsonNode.Keys.Count > 0)
                    {
                        for (int i = 0; i < jsonNode.Keys.Count; i++)
                        {
                            string key = jsonNode.Keys[i];
                            JsonData value = ExportToJsonDataExtension(jsonNode.children[i] as JsonNode);
                            objData[key] = value;
                        }
                    }
                    else
                    {
                        objData.SetJsonType(LitJson.JsonType.Object);
                    }
                    return objData;


                case JsonNodeType.Array:

                    var arrayData = new JsonData();
                    if (jsonNode.ChildCount > 0)
                    {
                        for (int i = 0; i < jsonNode.ChildCount; i++)
                        {
                            JsonData value = ExportToJsonDataExtension(jsonNode.children[i] as JsonNode);
                            arrayData[i] = value;
                        }
                    }
                    else
                    {
                        arrayData.SetJsonType(LitJson.JsonType.Array);
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

                    return new JsonData();
            }
        }



    }
}