using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace OnionCollections.DataEditor.Editor
{
    internal class JsonNodeConstructor : NodeConstructorBase
    {

        static GUIStyle objectOrArrayConatiner;
        static GUIStyle objectOrArrayTitle;
        static GUIStyle objectOrArrayTitleChip;

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

            var saveAction = new OnionAction(() =>
            {
                string jsonText = JsonBridge.GetJsonText(root);
                Save(jsonText);
            }, "Save", OnionDataEditor.GetIconTexture("Save"));

            node.NodeActions = new List<OnionAction> { saveAction };

            JsonBridge.BuildNode(root, target, Bind);

            return node;

            //

            void Bind(JsonNode jsonNode)
            {
                jsonNode.description = jsonNode.JsonType.ToString();

                //Object
                if (jsonNode.JsonType == JsonNode.JsonNodeType.Object)
                {
                    jsonNode.OnInspectorGUI = () => DrawCollectionGUI(jsonNode, false);
                    jsonNode.NodeActions = new List<OnionAction> 
                    {
                        saveAction,
                        GetRemoveNodeAction(jsonNode),
                        new OnionAction(() =>
                        {
                            CommonTypeNameInputWindow.Open(
                                "Add Property",
                                JsonNode.JsonNodeType.Object,
                                "",
                                (propertyType, propertyName) =>
                                {
                                    if(string.IsNullOrEmpty(propertyName) == false)
                                    {
                                        AddNewObjectProperty(jsonNode, propertyName, (JsonNode.JsonNodeType)propertyType);
                                    }
                                });

                        }, 
                        "Add Property",
                        OnionDataEditor.GetIconTexture("Add")),

                    };

                    return;
                }

                //Array
                if (jsonNode.JsonType == JsonNode.JsonNodeType.Array)
                {
                    jsonNode.OnInspectorGUI = () => DrawCollectionGUI(jsonNode, true);
                    jsonNode.NodeActions = new List<OnionAction> 
                    {
                        saveAction,
                        GetRemoveNodeAction(jsonNode),
                        new OnionAction(() =>
                        {
                            if(jsonNode.ChildCount == 0)
                            {
                                CommonTypeNameInputWindow.Open(
                                    "Add Item",
                                    JsonNode.JsonNodeType.Object,
                                    jsonType =>
                                    {
                                        AddNewArrayItem(jsonNode, (JsonNode.JsonNodeType)jsonType);
                                    });
                            }
                            else
                            {
                                JsonNode.JsonNodeType arrayItemType = (jsonNode.GetChildren().First() as JsonNode).JsonType;
                                AddNewArrayItem(jsonNode, arrayItemType);
                            }
                        },
                        "Add Item",
                        OnionDataEditor.GetIconTexture("Add")),
                    };

                    return;
                }

                //Field
                jsonNode.OnInspectorGUI = () => DrawFieldGUI(jsonNode);
                jsonNode.OnRowGUI = (rect) =>
                {
                    float h = EditorGUIUtility.singleLineHeight;
                    rect = rect.MoveDown((rect.height - h) / 2 - 1).SetHeight(h);

                    jsonNode.DrawFieldGUI(rect);
                };
                jsonNode.NodeActions = new List<OnionAction> 
                { 
                    saveAction,
                    GetRemoveNodeAction(jsonNode),
                };

            }

            OnionAction GetRemoveNodeAction(JsonNode targetNode)
            {
                return new OnionAction(() =>
                {
                    JsonNode parent = (targetNode.Parent as JsonNode);
                    if (parent != null)
                    {
                        parent.Remove(targetNode);
                        parent.UpdateItemIndexDisplayName();
                        OnionDataEditorWindow.UpdateTreeView();
                        OnionDataEditorWindow.SetSelectionAt(parent);
                    }
                },
                "Remove",
                OnionDataEditor.GetIconTexture("Trash"));
            }

            void AddNewArrayItem(JsonNode arrayNode, JsonNode.JsonNodeType jsonNodeType)
            {
                var newArrayNode = new JsonNode()
                {
                    JsonType = jsonNodeType,
                };
                Bind(newArrayNode);
                arrayNode.AddItem(newArrayNode);
                arrayNode.UpdateItemIndexDisplayName();
                OnionDataEditorWindow.UpdateTreeView();
            }

            void AddNewObjectProperty(JsonNode objectNode, string propertyName, JsonNode.JsonNodeType jsonNodeType)
            {
                var newObjectNode = new JsonNode()
                {
                    JsonType = jsonNodeType,
                };
                Bind(newObjectNode);
                objectNode.AddProperty(propertyName, newObjectNode);
                objectNode.UpdatePropertyDisplayName();
                OnionDataEditorWindow.UpdateTreeView();
            }

            //

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
                                new GUIStyle()
                                {
                                    normal = { background = OnionDataEditor.GetIconTexture("Gray") },
                                    stretchHeight = true,
                                },
                                GUILayout.Width(1));
                            GUI.color = Color.white;
                            GUILayout.Space(10);

                            using (new GUILayout.VerticalScope())
                            {
                                if (jsonNode.ChildCount > 0)
                                {
                                    foreach (var nodeCh in jsonNode.GetChildren())
                                    {
                                        nodeCh.OnInspectorGUI?.Invoke();
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
                        GUI.color = new Color(1, 1, 1, 0.5F);
                        GUILayout.Label(OnionDataEditor.GetIconTexture("Dash"),
                            new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 4, 1) },
                            GUILayout.Width(14), GUILayout.Height(14));
                        GUI.color = Color.white;

                        GUILayout.Label(
                            new GUIContent(jsonNode.displayName),
                            new GUIStyle(EditorStyles.label) { margin = new RectOffset(0, 0, 1, 1) },
                            GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        GUILayout.FlexibleSpace();
                    }

                    Rect rect = GUILayoutUtility.GetRect(new GUIContent(), new GUIStyle() { stretchWidth = true, stretchHeight = true });

                    float h = EditorGUIUtility.singleLineHeight;
                    rect = rect.MoveDown((rect.height - h) / 2 - 1).SetHeight(h);

                    jsonNode.DrawFieldGUI(rect);

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
                    margin = new RectOffset(0, 0, 4, 4),
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

            void Save(string jsonText)
            {
                string assetPath = AssetDatabase.GetAssetPath(target).Substring("Asset/".Length);

                string projectPath = Application.dataPath;

                string path = $"{projectPath}{assetPath}";

                try
                {
                    File.WriteAllText(path, jsonText, Encoding.UTF8);
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

        }


    }

}