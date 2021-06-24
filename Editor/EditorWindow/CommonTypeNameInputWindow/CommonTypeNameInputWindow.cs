using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    public class CommonTypeNameInputWindow : EditorWindow
    {

        public static void Open(string title, Enum enumValue, string text, Action<Enum, string> onConfirm)
        {
            Vector2 size = new Vector2(250, 130);
            CommonTypeNameInputWindow window = CreateInstance<CommonTypeNameInputWindow>();

            window.maxSize = size;
            window.minSize = size;
            window.titleContent = new GUIContent(title);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, size.x, size.y);

            window.enumField.Init(enumValue);
            window.textField.value = text;
            window.onConfirm = onConfirm;

            window.ShowModal();
        }

        public static void Open(string title, Enum enumValue, Action<Enum> onConfirm)
        {
            Vector2 size = new Vector2(250, 100);
            CommonTypeNameInputWindow window = CreateInstance<CommonTypeNameInputWindow>();

            window.maxSize = size;
            window.minSize = size;
            window.titleContent = new GUIContent(title);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, size.x, size.y);

            window.enumField.Init(enumValue);
            window.textFieldRoot.Hide();
            window.onConfirm = (e, _) => onConfirm(e);

            window.ShowAuxWindow();
            window.textField.Focus();
        }

        private void OnEnable()
        {
            rootVisualElement.Add(GetRootVisualElement());
            
            textField.Focus();
        }

        Action<Enum, string> onConfirm = null;

        VisualElement enumFieldRoot;
        VisualElement textFieldRoot;

        EnumField enumField;
        TextField textField;

        VisualElement GetRootVisualElement()
        {

            VisualElement root = new VisualElement()
                .SetPadding(20F)
                .SetFlexGrow(1F);

            enumFieldRoot = new VisualElement()
                .SetFlexGrow(1F)
                .SetFlexDirection(FlexDirection.Row)
                .AddTo(root);

            new Label("Type")
                .SetFlexGrow(1F)
                .AddTo(enumFieldRoot);

            enumField = new EnumField()
                .SetWidth(150)
                .AddTo(enumFieldRoot);

            textFieldRoot = new VisualElement()
            {
                style =
                    {
                        marginTop = 5,
                    }
            }
            .SetFlexGrow(1F)
            .SetFlexDirection(FlexDirection.Row)
            .AddTo(root);

            new Label("Name")
                .SetFlexGrow(1F)
                .AddTo(textFieldRoot);

            textField = new TextField()
                .SetWidth(150)
                .AddTo(textFieldRoot);

            textField.RegisterCallback<KeyDownEvent>(n =>
            {
                switch (n.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        OnClickOk();
                        break;

                    case KeyCode.Escape:
                        Close();
                        break;
                }
            });


            VisualElement btnRoot = new VisualElement()
                .SetFlexGrow(1F)
                .AddTo(root);

            Button btnOk = new Button(OnClickOk)
            {
                text = "Ok",
                style =
                {
                    marginTop = 20,
                    marginRight = 30,
                    marginLeft = 30,
                }
            }
            .SetHeight(30)
            .AddTo(btnRoot);





            return root;




            void OnClickOk()
            {
                Enum enumValue = enumField.value;
                string text = textField.text;

                onConfirm?.Invoke(enumValue, text);

                Close();
            }
        }




    }
}