using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    public class CommonTextInputWindow : EditorWindow
    {
        Action<string> onConfirm = null;

        public static void Open(string title, Action<string> onConfirm)
        {

            Vector2 size = new Vector2(250, 50);

            CommonTextInputWindow window = CreateInstance<CommonTextInputWindow>();

            window.onConfirm = onConfirm;
            window.maxSize = size;
            window.minSize = size;
            window.titleContent = new GUIContent(title);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, size.x, size.y);

            window.ShowAuxWindow();
        }

        private void OnEnable()
        {
            string path = $"{OnionDataEditor.Path}/Editor/EditorWindow/CommonTextInputWindow/CommonTextInputWindow.uxml";

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            TemplateContainer cloneTree = visualTree.CloneTree();
            cloneTree.style.flexGrow = 1;
            rootVisualElement.Add(cloneTree);

            rootVisualElement.Q<Button>("Btn-Ok").clickable.clicked += OnClickOk;

        }

        void OnClickOk()
        {
            string result = rootVisualElement.Q<TextField>("TextField").text;

            onConfirm?.Invoke(result);

            Close();
        }


    }
}