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

        string editingContent;
        void OnInspectorGUI()
        {
            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                var n = GUILayout.TextArea(editingContent);
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