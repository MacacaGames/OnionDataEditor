#if(UNITY_EDITOR)

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionReorderableList
    {
        ReorderableList rList;

        SerializedObject serializedObject;
        string title;

        Action<Rect, SerializedProperty, int> customGUI = null;

        bool _editable = true;
        public bool Editable
        {
            set
            {
                rList.displayAdd = value;
                rList.displayRemove = value;
                rList.draggable = value;
                _editable = value;
            }
            get => _editable;
        }

        internal OnionReorderableList(SerializedProperty elements, string title)
        {
            Init(elements.serializedObject, elements, title, null);
        }

        internal OnionReorderableList(SerializedProperty elements, string title, Action<Rect, SerializedProperty, int> customGUI)
        {
            Init(elements.serializedObject, elements, title, customGUI);
        }

        void Init(SerializedObject serializedObject, SerializedProperty elements, string title, Action<Rect, SerializedProperty, int> customGUI)
        {
            this.serializedObject = serializedObject;
            this.title = title;
            this.customGUI = customGUI;

            rList = new ReorderableList(serializedObject, elements, true, true, true, true)
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement
            };

            rList.elementHeight = EditorGUIUtility.singleLineHeight * 1.2F;
        }

        void DrawListHeader(Rect rect)
        {
            var arect = rect;
            arect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(arect, title);
        }


        void DrawListElement(Rect rect, int currentIndex, bool isActive, bool isFocused)
        {
            var arect = rect;
            var serElem = rList.serializedProperty.GetArrayElementAtIndex(currentIndex);
            arect.height = EditorGUIUtility.singleLineHeight;
            arect.y += EditorGUIUtility.singleLineHeight * 0.1F;

            if (customGUI == null)
            {
                EditorGUI.PropertyField(arect, serElem, GUIContent.none);
            }
            else
            {
                customGUI.Invoke(arect, serElem, currentIndex);
            }
        }

        public void OnInspectorGUI()
        {
            serializedObject.Update();

            rList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif