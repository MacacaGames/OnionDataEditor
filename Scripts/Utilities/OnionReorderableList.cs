#if(UNITY_EDITOR)

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionReorderableList : ReorderableList
    {
        ReorderableList rList;

        SerializedObject serializedObject;
        SerializedProperty elements;
        string title;

        public OnionReorderableList(SerializedProperty elements, string title) : base(elements.serializedObject, elements)
        {
            Init(elements.serializedObject, elements, title);
        }
        public OnionReorderableList(SerializedObject serializedObject, SerializedProperty elements, string title) : base(serializedObject, elements)
        {
            Init(serializedObject, elements, title);
        }

        void Init(SerializedObject serializedObject, SerializedProperty elements, string title)
        {
            this.serializedObject = serializedObject;
            this.elements = elements;
            this.title = title;

            rList = new ReorderableList(serializedObject, elements, true, true, true, true)
            {
                drawHeaderCallback = DrawListHeader,
                drawElementCallback = DrawListElement
            };
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

            EditorGUI.PropertyField(arect, serElem, GUIContent.none);
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