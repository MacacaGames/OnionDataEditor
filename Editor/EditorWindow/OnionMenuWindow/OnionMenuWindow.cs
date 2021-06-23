using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;


namespace OnionCollections.DataEditor.Editor
{
    public class OnionMenuWindow: EditorWindow
    {

        public void AddRoot(VisualElement ve)
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(ve);
        }

        float openTime;

        void OnEnable()
        {
            openTime = Time.realtimeSinceStartup;
        }

        void OnLostFocus()
        {
            float aliveTime = Time.realtimeSinceStartup - openTime;

            if (aliveTime > 0.1F)
            {
                Close();
            }
            else
            {
                Focus();
            }
        }

    }

    internal class OnionMenu
    {
        VisualElement root;
        Vector2 size;

        OnionMenuWindow window;

        public OnionMenu()
        {
            float padding = 4F;

            root = new VisualElement()
                .SetBorderWidth(1)
                .SetBorderColor(new Color(0, 0, 0, 0.45F));

            root.style.backgroundColor = new StyleColor(new Color(0.2F, 0.2F, 0.2F));
            root.style.paddingBottom = new StyleLength(padding);
            root.style.paddingTop = new StyleLength(padding);

            size = new Vector2(170, 2 + padding * 2);


            window = EditorWindow.CreateInstance<OnionMenuWindow>();
        }


        public void AddItem(string name, Action action, Texture icon = null)
        {

            float buttonHeight = 30;
            size += new Vector2(0, buttonHeight + 1);

            VisualElement b = new VisualElement()
                .SetPadding(0)
                .SetMargin(0)
                .SetBorderWidth(0)
                .SetBorderRadius(0)
                .SetBorderColor(new Color(0.1F, 0.1F, 0.1F, 0F))
                .SetFlexDirection(FlexDirection.Row)
                .SetHeight(buttonHeight);

            b.style.borderBottomWidth = new StyleFloat(1F);

            b.style.backgroundColor = new StyleColor(new Color(0.2F, 0.2F, 0.2F));

            b.RegisterCallback<MouseOverEvent>(n => b.style.backgroundColor = new StyleColor(new Color(0.25F, 0.25F, 0.25F)));
            b.RegisterCallback<MouseOutEvent>(n => b.style.backgroundColor = new StyleColor(new Color(0.2F, 0.2F, 0.2F)));

            b.RegisterCallback<MouseUpEvent>(n =>
            {
                if (n.button == 0)
                {
                    action?.Invoke();
                }
                window.Close();
            });

            float iconSize = 16;
            Image iconVe = new Image()
                .AddTo(b)
                .SetMargin((buttonHeight - iconSize)/2)
                .SetWidth(iconSize)
                .SetHeight(iconSize);

            iconVe.image = icon;

            Label lableVe = new Label(name)
                .AddTo(b)
                .SetHeight(buttonHeight);

            lableVe.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);


            root.Add(b);

        }

        public void AddSplitLine()
        {
            float margin = 6F;

            size += new Vector2(0, 1);

            VisualElement b = new VisualElement()
                .SetPadding(0)
                .SetMargin(0)
                .SetBorderWidth(0)
                .SetBorderRadius(0)
                .SetBorderColor(new Color(0.1F, 0.1F, 0.1F, 0.2F))
                .SetFlexDirection(FlexDirection.Row)
                .SetHeight(0)
                .AddTo(root);

            b.style.borderBottomWidth = new StyleFloat(1);
            b.style.marginLeft = new StyleLength(margin);
            b.style.marginRight = new StyleLength(margin);
        }

        public void Show()
        {
            window.AddRoot(root);

            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            //window.ShowAsDropDown(new Rect(pos, Vector2.zero), size);

            window.position = new Rect(pos, size);
            window.ShowPopup();
        }
    }
}