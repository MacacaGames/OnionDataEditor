using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{

    class OnionPanelVisualElement : VisualElement
    {

        public OnionPanelVisualElement(string title, Texture2D icon, VisualElement content)
        {
            VisualElement root = this.SetFlexGrow(1);

            VisualElement header = new VisualElement().AddTo(root);

            new IMGUIContainer(() => DrawHeader(header.layout)).AddTo(header);

            content.AddTo(root);

            header.SetBorderColor(new Color(0.14F, 0.14F, 0.14F));
            header.SetBorderWidth(1F);
            content.SetBorderColor(new Color(0.14F, 0.14F, 0.14F));
            content.SetBorderWidth(1F);
            content.style.borderTopWidth = new StyleFloat(0F);

            header.style.borderTopLeftRadius = new StyleLength(3F);
            header.style.borderTopRightRadius = new StyleLength(3F);
            content.style.borderBottomLeftRadius = new StyleLength(3F);
            content.style.borderBottomRightRadius = new StyleLength(3F);

            content.style.paddingLeft = new StyleLength(3);
            content.style.paddingRight = new StyleLength(3);
            content.style.paddingTop = new StyleLength(5);
            content.style.paddingBottom = new StyleLength(5);

            header.style.backgroundColor = new StyleColor(new Color(0.2F, 0.2F, 0.2F));
            content.style.backgroundColor = new StyleColor(new Color(0.255F, 0.255F, 0.255F));

            header.SetHeight(20);
            header.style.paddingLeft = new StyleLength(5);

            root.style.marginBottom = new StyleLength(10);


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