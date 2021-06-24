using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;


namespace OnionCollections.DataEditor.Editor
{
    internal class OnionMenuWindow: EditorWindow
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

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{OnionDataEditor.Path}/Editor/EditorWindow/OnionMenuWindow/OnionMenu.uss");

            rootVisualElement.styleSheets.Add(styleSheet);
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

            size = new Vector2(180, 2 + padding * 2);


            window = EditorWindow.CreateInstance<OnionMenuWindow>();
        }


        public void AddItems(IEnumerable<OnionAction> onionActions)
        {
            foreach(var onionAction in onionActions)
            {
                AddItem(onionAction);
            }
        }

        public void AddItem(OnionAction onionAction)
        {
            AddItem(onionAction.actionName, onionAction.action, onionAction.actionIcon);
        }

        public void AddItem(string name, Action action, Texture icon = null)
        {
            size += new Vector2(0, 30);

            VisualElement b = new VisualElement()
                .AddTo(root)
                .AddClass("item-btn");

            b.RegisterCallback<MouseUpEvent>(n =>
            {
                if (n.button == 0)
                {
                    action?.Invoke();
                }
                window.Close();
            });

            new Image() { image = icon }
                .AddTo(b)
                .AddClass("item-icon");


            new Label(name)
                .AddTo(b)
                .AddClass("item-text");


        }

        public void AddLabel(string text)
        {
            size += new Vector2(0, 24);

            new Label(text)
                .AddTo(root)
                .AddClass("item-label");
        }

        public void AddSplitLine()
        {
            size += new Vector2(0, 1);

            new VisualElement()
                .AddTo(root)
                .AddClass("item-splitline");
        }

        public void AddCustomItem(VisualElement ve, float height)
        {
            size += new Vector2(0, height);

            ve
                .AddTo(root)
                .AddClass("item-custom");
        }

        public void Show()
        {
            window.AddRoot(root);

            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            window.ShowAsDropDown(new Rect(pos, Vector2.zero), size);

            //window.ShowPopup();
        }
    }
}