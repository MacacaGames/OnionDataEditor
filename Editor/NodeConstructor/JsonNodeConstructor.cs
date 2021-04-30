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

        GUIStyle objectOrArrayConatiner;
        GUIStyle objectOrArrayTitle;
        GUIStyle objectOrArrayTitleChip;

        public override TreeNode Construct(TreeNode node, UnityEngine.Object target)
        {
            InitStyles();

            node.ClearChildren();

            JsonNode root = new JsonNode()
            {
                displayName = "root",
                description = "Object",
            };

            node.AddSingleChild(root);

            JsonData jsonDataRoot = JsonMapper.ToObject((target as TextAsset).text);

            root.SetFromJsonData(jsonDataRoot, Bind);

            return node;
            
            //

            void Bind(JsonNode jsonNode)
            {
                //Object
                if (jsonNode.JsonType == JsonNode.JsonNodeType.Object)
                {
                    jsonNode.OnInspectorAction = new OnionAction(() => DrawCollectionGUI(jsonNode, false));
                    return;
                }

                //Array
                if (jsonNode.JsonType == JsonNode.JsonNodeType.Array)
                {
                    jsonNode.OnInspectorAction = new OnionAction(() => DrawCollectionGUI(jsonNode, true));
                    return;
                }

                //Field
                jsonNode.OnInspectorAction = new OnionAction(() => DrawFieldGUI(jsonNode));

            }

            void DrawCollectionGUI(JsonNode jsonNode, bool isArray)
            {
                string lb = isArray ? "[" : "{";
                string rb = isArray ? "]" : "}";

                using (new GUILayout.HorizontalScope())
                {
                    jsonNode.isExpend = GUILayout.Toggle(jsonNode.isExpend,
                        new GUIContent($"{jsonNode.displayName}"),
                        objectOrArrayTitle,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    GUI.color = new Color(1, 1, 1, 0.7F);

                    GUILayout.Label(
                        $"  {lb}",
                        objectOrArrayTitleChip,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (jsonNode.isExpend == false)
                    {
                        GUILayout.Space(7);

                        GUILayout.Label($"{jsonNode.ChildCount}", new GUIStyle(EditorStyles.helpBox) { stretchWidth = false });

                        GUILayout.Space(7);

                        GUILayout.Label(
                            $"{rb}",
                            objectOrArrayTitleChip,
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }

                    GUI.color = Color.white;
                }

                if (jsonNode.isExpend)
                {
                    using (new GUILayout.VerticalScope(objectOrArrayConatiner))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);
                            GUI.color = new Color(1, 1, 1, 0.1F);
                            GUILayout.Label(
                                "", 
                                new GUIStyle() { 
                                    normal = { background = OnionDataEditor.GetIconTexture("Gray") },
                                    stretchHeight = true,
                                },
                                GUILayout.Width(1));
                            GUI.color = Color.white;
                            GUILayout.Space(5);

                            using (new GUILayout.VerticalScope())
                            {
                                if (jsonNode.ChildCount > 0)
                                {
                                    foreach (var nodeCh in jsonNode.GetChildren())
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
                        }
                    }

                    if (true)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUI.color = new Color(1, 1, 1, 0.7F);
                            GUILayout.Space(5);
                            GUILayout.Label(
                                $"{rb}",
                                objectOrArrayTitleChip,
                                GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            GUI.color = Color.white;
                        }
                    }
                }
            }

            void DrawFieldGUI(JsonNode jsonNode)
            {
                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
                    {
                        GUILayout.Space(14);
                        GUILayout.Label(
                            new GUIContent(jsonNode.displayName),
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        GUILayout.FlexibleSpace();
                    }

                    jsonNode.DrawFieldGUI();
                }
            }

            //

            void InitStyles()
            {
                objectOrArrayConatiner = new GUIStyle
                {
                    padding = new RectOffset(0, 0, 0, 0),
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
            }

        }












    }
}