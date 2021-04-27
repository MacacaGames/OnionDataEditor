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
        
        public string title = "No Title";
        public Texture2D titleIcon = null;

        public Action<Rect, SerializedProperty, int> customGUI = null;
        public Action<Rect> customHeadGUI = null;


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

        internal OnionReorderableList(SerializedProperty elements)
        {
            Init(elements.serializedObject, elements);
        }

        void Init(SerializedObject serializedObject, SerializedProperty elements)
        {
            this.serializedObject = serializedObject;

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

            if (customHeadGUI == null)
            {
                if (titleIcon != null)
                {
                    EditorGUI.LabelField(new Rect(rect.x + 2, rect.y + 2, 14, 14), new GUIContent("", titleIcon));
                    arect.x += 18;
                }
                EditorGUI.LabelField(arect, title);
            }
            else
            {
                customHeadGUI.Invoke(arect);
            }
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