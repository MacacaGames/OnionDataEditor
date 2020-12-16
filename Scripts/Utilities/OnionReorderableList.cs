#if(UNITY_EDITOR)

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionReorderableList
    {
        ReorderableList rList;

        SerializedObject serializedObject;
        string title;

        internal OnionReorderableList(SerializedProperty elements, string title)
        {
            Init(elements.serializedObject, elements, title);
        }

        void Init(SerializedObject serializedObject, SerializedProperty elements, string title)
        {
            this.serializedObject = serializedObject;
            this.title = title;

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