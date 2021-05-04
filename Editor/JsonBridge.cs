#define USE_LITJSON

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if(USE_LITJSON)
using LitJson;
#endif


namespace OnionCollections.DataEditor.Editor
{

    internal static class JsonBridge
    {


#if (USE_LITJSON)
        public static void BuildNode(JsonNode root, UnityEngine.Object target, Action<JsonNode> bind)
        {
            JsonData jsonDataRoot = JsonMapper.ToObject((target as TextAsset).text);

            ImportFromJsonData(root, jsonDataRoot, bind);
        }

        public static void Save(JsonNode exportJsonNode, UnityEngine.Object target)
        {
            var n = ExportToJsonData(exportJsonNode);

            var sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb)
            {
                PrettyPrint = true,
            };

            JsonMapper.ToJson(n, writer);

            string resultJsonText = sb.ToString();

            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            resultJsonText = reg.Replace(resultJsonText, (Match m) => { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });


            string assetPath = AssetDatabase.GetAssetPath(target).Substring("Asset/".Length);

            string projectPath = Application.dataPath;

            string path = $"{projectPath}{assetPath}";

            try
            {
                File.WriteAllText(path, resultJsonText, Encoding.UTF8);
                EditorUtility.SetDirty(target);
                AssetDatabase.Refresh();

                //Debug.Log(resultJsonText);
                Debug.Log("Save success");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

        }

        static void ImportFromJsonData(JsonNode jsonNode, JsonData jsonData, Action<JsonNode> onBind)
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
                jsonNode.JsonType = JsonNode.JsonNodeType.Array;
                for (int i = 0; i < jsonData.Count; i++)
                {
                    var j = jsonData[i];
                    var ch = new JsonNode();
                    jsonNode.AddItem(ch);

                    ImportFromJsonData(ch, j, onBind);
                }

                jsonNode.UpdateItemIndexDisplayName();
            }

            void BuildObject()
            {
                jsonNode.JsonType = JsonNode.JsonNodeType.Object;
                foreach (string key in jsonData.Keys)
                {
                    var j = jsonData[key];

                    var ch = new JsonNode()
                    {
                        displayName = key
                    };
                    jsonNode.AddProperty(key, ch);

                    ImportFromJsonData(ch, j, onBind);
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

        static JsonData ExportToJsonData(JsonNode jsonNode)
        {
            switch (jsonNode.JsonType)
            {
                case JsonNode.JsonNodeType.Object:

                    var objData = new JsonData();
                    objData.SetJsonType(LitJson.JsonType.Object);
                    if (jsonNode.Keys.Count > 0)
                    {
                        var ch = jsonNode.GetChildren().ToArray();
                        for (int i = 0; i < jsonNode.Keys.Count; i++)
                        {
                            string key = jsonNode.Keys[i];
                            JsonData value = ExportToJsonData(ch[i] as JsonNode);
                            objData[key] = value;
                        }
                    }
                    return objData;


                case JsonNode.JsonNodeType.Array:

                    var arrayData = new JsonData();
                    arrayData.SetJsonType(LitJson.JsonType.Array);
                    if (jsonNode.ChildCount > 0)
                    {
                        var ch = jsonNode.GetChildren().ToArray();
                        for (int i = 0; i < jsonNode.ChildCount; i++)
                        {
                            JsonData value = ExportToJsonData(ch[i] as JsonNode);
                            arrayData.Add(value);
                        }
                    }
                    return arrayData;


                case JsonNode.JsonNodeType.String:
                    return new JsonData(jsonNode.str);


                case JsonNode.JsonNodeType.Int:
                    return new JsonData(jsonNode.i);

                case JsonNode.JsonNodeType.Float:
                    return new JsonData(jsonNode.f);

                case JsonNode.JsonNodeType.Bool:
                    return new JsonData(jsonNode.b);


                default:
                    throw new Exception($"Unknown json data type in {jsonNode.displayName}");
            }
        }

#else

        public static void BuildNode(JsonNode root, UnityEngine.Object target, Action<JsonNode> bind)
        {
            //
        }

        public static void Save(JsonNode exportJsonNode, UnityEngine.Object target)
        {
            //
        }
#endif

    }
}