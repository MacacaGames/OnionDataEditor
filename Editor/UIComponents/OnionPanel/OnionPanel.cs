using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{

    class OnionPanel : VisualElement
    {

        public OnionPanel(string title, Texture2D icon, VisualElement content)
        {
            string ussPath = $"{OnionDataEditor.Path}/Editor/UIComponents/OnionPanel/OnionPanel.uss";
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            this.styleSheets.Add(styleSheet);

            VisualElement root = this
                .AddClass("onion-panel");

            VisualElement header = new VisualElement()
                .AddTo(root)
                .AddClass("header");

            new IMGUIContainer(() => DrawHeader(header.layout))
                .AddTo(header);

            content
                .AddTo(root)
                .AddClass("content");



            void DrawHeader(Rect rect)
            {
                var aRect = rect;
                aRect.height = EditorGUIUtility.singleLineHeight;

                if (icon != null)
                {
                    EditorGUI.LabelField(new Rect(rect.x + 0, rect.y + 1, 16, 16), new GUIContent("", icon));
                    aRect.x += 19;
                }
                EditorGUI.LabelField(aRect, title);
            }
        }
    }
}