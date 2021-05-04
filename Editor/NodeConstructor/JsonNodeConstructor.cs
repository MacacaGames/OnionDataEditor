using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using LitJson;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OnionCollections.DataEditor.Editor
{
#if (true || ONIONDATAEDITOR_USE_JSON)
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

            var saveAction = new OnionAction(() => JsonBridge.Save(root, target), "Save");

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
                    jsonNode.OnInspectorAction = new OnionAction(() => DrawCollectionGUI(jsonNode, false));
                    jsonNode.NodeActions = new List<OnionAction> 
                    {
                        saveAction,
                        GetRemoveNodeAction(jsonNode),
                        //new OnionAction(() =>
                        //{
                        //    CommonTextInputWindow.Open("Add property", str => { Debug.Log($"Add {str}"); });
                        //}, "Add"),

                    };

                    return;
                }

                //Array
                if (jsonNode.JsonType == JsonNode.JsonNodeType.Array)
                {
                    jsonNode.OnInspectorAction = new OnionAction(() => DrawCollectionGUI(jsonNode, true));
                    jsonNode.NodeActions = new List<OnionAction> 
                    {
                        saveAction,
                        GetRemoveNodeAction(jsonNode),
                        new OnionAction(() =>
                        {
                            if(jsonNode.ChildCount == 0)
                            {
                                var menu = new GenericMenu();
                                foreach( JsonNode.JsonNodeType enumType in Enum.GetValues(typeof(JsonNode.JsonNodeType)))
                                {
                                    if(enumType == JsonNode.JsonNodeType.None)
                                        continue;

                                    menu.AddItem(new GUIContent(enumType.ToString()), false, ()=>
                                    {
                                        AddNewArrayItem(jsonNode, enumType);
                                    });
                                }
                                menu.ShowAsContext();
                            }
                            else
                            {
                                JsonNode.JsonNodeType arrayItemType = (jsonNode.GetChildren().First() as JsonNode).JsonType;
                                AddNewArrayItem(jsonNode, arrayItemType);
                            }
                        },
                        "Add",
                        OnionDataEditor.GetIconTexture("Add")),
                    };

                    return;
                }

                //Field
                jsonNode.OnInspectorAction = new OnionAction(() => DrawFieldGUI(jsonNode));
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



        }


    }

#endif
}