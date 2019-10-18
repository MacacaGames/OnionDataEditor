#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using System.Text;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionDocumentWindow : EditorWindow
    {
        const string path = "Assets/OnionDataEditor";

        static OnionDocument.DocumentObject data;
        public static OnionDocumentWindow ShowWindow(OnionDocument.DocumentObject _data)
        {
            data = _data;
            var window = GetWindow<OnionDocumentWindow>();
            window.Display(data);

            return window;
        }

        public void OnEnable()
        {
            Init();
        }

        void Init()
        {
            var root = this.rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "/Editor/OnionDocument.uxml");
            var cloneTree = visualTree.CloneTree();
            cloneTree.style.flexGrow = 1;

            root.Add(cloneTree);

            Display(data);
        }

        void Display(OnionDocument.DocumentObject doc)
        {
            var root = this.rootVisualElement;

            root.Q<Label>("title").text = doc.title;

            VisualElement content = root.Q("content");
            content.Clear();

            foreach (var d in doc.data)
            {
                VisualElement element = new VisualElement();
                element.AddToClassList("element");

                Label title = new Label(d.name);
                title.AddToClassList("title");

                Label description = new Label(d.description);
                description.AddToClassList("description");

                element.Add(title);
                element.Add(description);
                content.Add(element);
            }
        }

    }
}

#endif