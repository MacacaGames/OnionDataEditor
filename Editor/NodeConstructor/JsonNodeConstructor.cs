using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;
using System;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    internal class JsonNodeConstructor : NodeConstructorBase
    {
        readonly Dictionary<TreeNode, JsonData> query = new Dictionary<TreeNode, JsonData>();


        GUIStyle objectOrArrayConatiner;
        GUIStyle objectOrArrayTitle;
        GUIStyle objectOrArrayTitleChip;

        public override TreeNode Construct(TreeNode node, UnityEngine.Object target)
        {
            node.ClearChildren();

            var n = GetJsonData();

            objectOrArrayConatiner = new GUIStyle
            {
                padding = new RectOffset(20, 0, 0, 0),
            };

            objectOrArrayTitle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                margin = new RectOffset(0, 0, 3, 3),
                stretchWidth = false,
                fontStyle = FontStyle.Normal,
            };

            objectOrArrayTitleChip = new GUIStyle(EditorStyles.label)
            {
                stretchWidth = false,
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 2, 2),
            };

            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            query.Clear();
            Map(node, n);

            return node;
            


            JsonData GetJsonData()
            {
                var reader = new JsonReader((target as TextAsset).text);

                var result = JsonMapper.ToObject(reader);
                
                //TODO: 要把JSON結構Map到JsonNode
                //TODO: 用JsonNode取代TreeNode


                return result;
            }


            void Map(TreeNode rootNode, JsonData root)
            {
                bool v = true;

                query.Add(rootNode, root);
                if (root.IsObject)
                {
                    rootNode.OnInspectorAction = new OnionAction(() =>
                    {
                        ShowContainer(rootNode, false);
                    });

                    foreach (string key in root.Keys)
                    {
                        var j = root[key];

                        var ch = new TreeNode()
                        {
                            displayName = key,
                            description = j.GetJsonType().ToString()
                        };
                        rootNode.AddSingleChild(ch);

                        Map(ch, j);
                    }
                }
                else if (root.IsArray)
                {
                    rootNode.OnInspectorAction = new OnionAction(() =>
                    {
                        ShowContainer(rootNode, true);
                    });

                    for (int i = 0; i < root.Count; i++)
                    {
                        var j = root[i];
                        var ch = new TreeNode()
                        {
                            displayName = $"[ {i} ]",
                        };
                        rootNode.AddSingleChild(ch);

                        Map(ch, j);
                    }
                }
                else
                {



                    rootNode.OnInspectorAction = new OnionAction(() =>
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
                            {
                                GUILayout.Space(14);
                                GUILayout.Label(
                                    new GUIContent(rootNode.displayName),
                                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                GUILayout.FlexibleSpace();
                            }

                            DrawFieldGUI(rootNode);
                        }
                    });

                    rootNode.NodeActions = new List<OnionAction>()
                    {
                        new OnionAction(() =>
                        {
                            query[rootNode] = new JsonData(60F);
                        }, "to double"),
                    };
                }


                void ShowContainer(TreeNode cNode, bool isArray)
                {
                    string lb = isArray ? "[" : "{";
                    string rb = isArray ? "]" : "}";

                    using (new GUILayout.HorizontalScope())
                    {
                        v = GUILayout.Toggle(v,
                            new GUIContent($"{cNode.displayName}"),
                            objectOrArrayTitle,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        GUI.color = new Color(1, 1, 1, 0.7F);

                        GUILayout.Label(
                            $"  {lb}",
                            objectOrArrayTitleChip,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        if (v == false)
                        {
                            GUILayout.Space(7);

                            GUILayout.Label($"{cNode.ChildCount}", new GUIStyle(EditorStyles.helpBox) { stretchWidth = false });

                            GUILayout.Space(7);

                            GUILayout.Label(
                                $"{rb}",
                                objectOrArrayTitleChip,
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        }

                        GUI.color = Color.white;
                    }

                    if (v)
                    {
                        using (new GUILayout.VerticalScope(objectOrArrayConatiner))
                        {
                            if (cNode.ChildCount > 0)
                            {
                                foreach (var nodeCh in cNode.GetChildren())
                                {
                                    nodeCh.OnInspectorAction?.action?.Invoke();
                                }
                            }
                            else
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(14);
                                    GUI.color = new Color(1, 1, 1, 0.5F);
                                    GUILayout.Label(
                                        isArray ? "Array is empty" : "Object is empty",
                                        new GUIStyle(EditorStyles.helpBox) { stretchWidth = false });
                                    GUI.color = Color.white;
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUI.color = new Color(1, 1, 1, 0.7F);
                            GUILayout.Space(14);
                            GUILayout.Label(
                                $"{rb}",
                                objectOrArrayTitleChip,
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            GUI.color = Color.white;
                        }
                    }
                }

                string stringCache;
                bool boolCache;
                int intCache;
                float floatCache;
                void DrawFieldGUI(TreeNode cNode)
                {
                    JsonData cJson = query[cNode];


                    if (cJson.IsBoolean)
                        boolCache = EditorGUILayout.Toggle((bool)cJson);

                    else if (cJson.IsInt)
                        intCache = EditorGUILayout.IntField((int)cJson);

                    else if (cJson.IsDouble)
                        floatCache = EditorGUILayout.FloatField((float)(double)cJson);

                    else if (cJson.IsString)
                        stringCache = EditorGUILayout.TextField((string)cJson);

                    else
                         EditorGUILayout.LabelField((string)cJson);
                }

            }



        }












    }
}