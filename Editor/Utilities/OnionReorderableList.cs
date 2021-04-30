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
        SerializedProperty elements;


        public string title = "No Title";
        public Texture2D titleIcon = null;

        public Action<Rect, SerializedProperty, int> customGUI = null;
        public Action<Rect> customHeadGUI = null;
        public Action<Rect> customListEmptyGUI = null;


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
            this.elements = elements;

            rList = new ReorderableList(serializedObject, elements, true, true, true, true)
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement,
                drawNoneElementCallback = DrawListEmpty,
            };

            rList.elementHeight = EditorGUIUtility.singleLineHeight * 1.2F;
        }

        void DrawListHeader(Rect rect)
        {
            var aRect = rect;
            aRect.height = EditorGUIUtility.singleLineHeight;

            if (customHeadGUI == null)
            {
                if (titleIcon != null)
                {
                    EditorGUI.LabelField(new Rect(rect.x +0, rect.y +1, 16, 16), new GUIContent("", titleIcon));
                    aRect.x += 19;
                }
                EditorGUI.LabelField(aRect, title);
            }
            else
            {
                customHeadGUI.Invoke(aRect);
            }

            //DrawListSize();

            void DrawListSize()
            {
                var rRect = rect;
                const float rectWidth = 40;

                rRect.x = rect.width - rectWidth;
                rRect.width = rectWidth;
                rRect.y += 1;

                using (var ch = new EditorGUI.ChangeCheckScope())
                {
                    var n = EditorGUI.DelayedIntField(rRect, elements.arraySize, EditorStyles.miniTextField);
                    if (ch.changed)
                        elements.arraySize = n;
                }
            }
        }


        void DrawListElement(Rect rect, int currentIndex, bool isActive, bool isFocused)
        {

            if (rList.serializedProperty.arraySize <= currentIndex)
                return;

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

        void DrawListEmpty(Rect rect)
        {
            var arect = rect;
            arect.height = EditorGUIUtility.singleLineHeight;

            if (customListEmptyGUI == null)
            {
                GUI.color = new Color(1, 1, 1, 0.5F);
                EditorGUI.LabelField(arect, "List is empty");
                GUI.color = Color.white;
            }
            else
            {
                customListEmptyGUI.Invoke(arect);
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