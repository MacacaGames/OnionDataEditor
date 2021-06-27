using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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


            Init(target);


            //node.OnInspectorGUI = OnInspectorGUI;
            node.OnInspectorVisualElementRoot = CreateInspectorRoot();

            node.NodeActions = new List<OnionAction>
            {
                new OnionAction(Save, "Save", OnionDataEditor.GetIconTexture("Save")),
            };

            return node;
        }



        TextAsset textAsset;
        void Init(Object target)
        {
            textAsset = target as TextAsset;
            editingContent = textAsset.text;
        }


        void Save()
        {
            string assetPath = AssetDatabase.GetAssetPath(textAsset).Substring("Asset/".Length);

            string projectPath = Application.dataPath;

            string path = $"{projectPath}{assetPath}";

            try
            {
                File.WriteAllText(path, editingContent, Encoding.UTF8);

                EditorUtility.SetDirty(textAsset);
                AssetDatabase.Refresh();

                OnionDataEditorWindow.ShowNotification(new GUIContent("Saved Success!"), 0.5F);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }


        VisualElement CreateInspectorRoot()
        {
            VisualElement root = new VisualElement();

            TextField textField = new TextField().AddTo(root);
            textField.multiline = true;
            textField.value = editingContent;

            textField
                .SetFlexGrow(1F)
                .SetBorderWidth(1)
                .SetBorderColor(new Color(0,0,0,0.3F))
                .SetBorderRadius(6);

            textField.style.fontSize = new StyleLength(13);

            textField.SelectRange(0, 0);
            textField.RegisterValueChangedCallback(n => editingContent = n.newValue);

            VisualElement textInput = textField.Q("unity-text-input");
            textInput
                .SetPadding(12F)
                .SetBorderWidth(0);


            return root;
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



    }
}