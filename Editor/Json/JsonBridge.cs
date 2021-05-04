using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace OnionCollections.DataEditor.Editor
{
    internal static class JsonBridge
    {
        public static void BuildNode(JsonNode root, UnityEngine.Object target, Action<JsonNode> bind)
        {
            JSONObject jsonDataRoot = JSONObject.Create((target as TextAsset).text);

            ImportFromJsonObject(root, jsonDataRoot, bind);
        }

        public static string GetJsonText(JsonNode exportJsonNode)
        {
            JSONObject exportJsonObject = ExportToJsonObject(exportJsonNode);

            return exportJsonObject.Print(true);
        }

        static void ImportFromJsonObject(JsonNode jsonNode, JSONObject jsonObject, Action<JsonNode> onBind)
        {
            //Object
            if (jsonObject.IsObject)
            {
                BuildObject();
            }

            //Array
            if (jsonObject.IsArray)
            {
                BuildArray();
            }

            //Field - String
            if (jsonObject.IsString)
            {
                BuildString();
            }

            //Field - Float
            if (jsonObject.IsNumber)
            {
                BuildFloat();
            }

            //Field - Bool
            if (jsonObject.IsBool)
            {
                BuildBool();
            }
            var n = jsonNode;

            onBind(n);


            void BuildArray()
            {
                jsonNode.JsonType = JsonNode.JsonNodeType.Array;
                for (int i = 0; i < jsonObject.Count; i++)
                {
                    var j = jsonObject[i];
                    var ch = new JsonNode();
                    jsonNode.AddItem(ch);

                    ImportFromJsonObject(ch, j, onBind);
                }

                jsonNode.UpdateItemIndexDisplayName();
            }

            void BuildObject()
            {
                jsonNode.JsonType = JsonNode.JsonNodeType.Object;
                foreach (string key in jsonObject.keys)
                {
                    var j = jsonObject[key];

                    var ch = new JsonNode()
                    {
                        displayName = key
                    };
                    jsonNode.AddProperty(key, ch);

                    ImportFromJsonObject(ch, j, onBind);
                }
            }


            void BuildString()
            {
                jsonNode.Set(jsonObject.str);
            }

            void BuildFloat()
            {
                jsonNode.Set(jsonObject.n);
            }

            void BuildBool()
            {
                jsonNode.Set(jsonObject.b);
            }


        }

        static JSONObject ExportToJsonObject(JsonNode jsonNode)
        {
            switch (jsonNode.JsonType)
            {
                case JsonNode.JsonNodeType.Object:

                    var objData = new JSONObject
                    {
                        type = JSONObject.Type.OBJECT,
                        keys = new List<string>(),
                        list = new List<JSONObject>(),
                    };
                    if (jsonNode.Keys.Count > 0)
                    {
                        var ch = jsonNode.GetChildren().ToArray();
                        for (int i = 0; i < jsonNode.Keys.Count; i++)
                        {
                            string key = jsonNode.Keys[i];
                            JSONObject value = ExportToJsonObject(ch[i] as JsonNode);
                            objData.AddField(key, value);
                        }
                    }
                    return objData;


                case JsonNode.JsonNodeType.Array:

                    var arrayData = new JSONObject
                    {
                        type = JSONObject.Type.ARRAY,
                        list = new List<JSONObject>(),
                    };
                    if (jsonNode.ChildCount > 0)
                    {
                        var ch = jsonNode.GetChildren().ToArray();
                        for (int i = 0; i < jsonNode.ChildCount; i++)
                        {
                            JSONObject value = ExportToJsonObject(ch[i] as JsonNode);
                            arrayData.Add(value);
                        }
                    }
                    return arrayData;


                case JsonNode.JsonNodeType.String:
                    return JSONObject.StringObject(jsonNode.str);

                case JsonNode.JsonNodeType.Number:
                    return new JSONObject(jsonNode.f);

                case JsonNode.JsonNodeType.Bool:
                    return new JSONObject(jsonNode.b);

                case JsonNode.JsonNodeType.Null:
                    return JSONObject.nullJO;


                default:
                    throw new Exception($"Unknown json data type in {jsonNode.displayName}");
            }
        }

    }
}