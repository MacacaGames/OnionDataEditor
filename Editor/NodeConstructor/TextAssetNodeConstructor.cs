using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace OnionCollections.DataEditor.Editor
{
    [CustomNodeConstructorOf(typeof(TextAsset))]
    internal class TextAssetNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            if(AssetDatabase.IsMainAsset(target) && AssetDatabase.GetAssetPath(target).EndsWith(".json"))
            {
                var n = new JsonNodeConstructor().Construct(node, target);

                return n;
            }



            node.OnInspectorAction = new OnionAction(OnInspectorGUI);

            node.NodeActions = new List<OnionAction>
            {
                new OnionAction(Save, "Save"),
            };

            Init(target);

            //node.GetElementTree();

            return node;
        }



        TextAsset textAsset;
        void Init(Object target)
        {
            textAsset = target as TextAsset;
            editingContent = textAsset.text;
        }

        GUIStyle textFieldGUIStyle;
        string editingContent;
        void OnInspectorGUI()
        {
            textFieldGUIStyle = new GUIStyle(EditorStyles.textArea)
            {
                padding = new RectOffset(15, 15, 15, 15),
                margin = new RectOffset(0, 0, 0, 0),
            };

            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                var n = GUILayout.TextArea(editingContent, textFieldGUIStyle);
                if (ch.changed)
                {
                    editingContent = n;
                }
            }
        }

        void Save()
        {
            string assetPath = AssetDatabase.GetAssetPath(textAsset).Substring("Asset/".Length);

            string projectPath = Application.dataPath;

            string path = $"{projectPath}{assetPath}";

            try
            {
                File.WriteAllText(path, editingContent);

                EditorUtility.DisplayDialog("Onion Data Editor", "Saved Success!", "OK");

                EditorUtility.SetDirty(textAsset);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }


    }
}