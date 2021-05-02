using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    [CustomPropertyDrawer(typeof(LockableAttribute))]
    public class LockablePropertyDrawer : PropertyDrawer
    {
        bool isLock = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float space = 5;
            float iconSize = EditorGUIUtility.singleLineHeight;

            GUI.enabled = !isLock;
            EditorGUI.PropertyField(position.ExtendRight(-iconSize - space), property, label);
            GUI.enabled = true;

            GUI.contentColor = isLock ? new Color(0.9F, 0.9F, 0.9F) : new Color(1F, 0.8F, 0.1F);
            bool isClick = GUI.Button(
                position.ExtendLeft(-(position.width - iconSize)).SetHeight(iconSize),
                new GUIContent(OnionDataEditor.GetIconTexture(isLock ? "Lock" : "Unlock")),
                new GUIStyle(EditorStyles.label) { padding = new RectOffset(2, 2, 2, 2) });
            GUI.contentColor = Color.white;

            if (isClick)
            {
                isLock = !isLock;
            }

        }

        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    VisualElement prop = base.CreatePropertyGUI(property);
        //    VisualElement icon = new Image() { image = OnionDataEditor.GetIconTexture("Lock") };

        //    VisualElement root = new VisualElement();
        //    root.Add(prop);
        //    root.Add(icon);

        //    return root;
        //}

    }
}