using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;

[CustomEditor(typeof(OnionEditorIconGroup))]
public class OnionEditorIconGroupEditor : Editor
{

    OnionReorderableList iconList;

    private void OnEnable()
    {
        if (iconList == null)
        {
            iconList = new OnionReorderableList(new SerializedObject(target).FindProperty("data"))
            {
                title = "Icons",
                customGUI = OnItemInspector,
                customHeadGUI = OnHeadInspector,
            };
        }

        void OnHeadInspector(Rect r)
        {
            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                const float leftPadding = 14;
                r.width -= leftPadding;
                r.x += leftPadding;

                r.width = (r.width - 10) / 3;

                EditorGUI.LabelField(r, new GUIContent("Key"));

                r.x += r.width + 10 / (3 - 1);
                EditorGUI.LabelField(r, new GUIContent("Default"));

                r.x += r.width + 10 / (3 - 1);
                EditorGUI.LabelField(r, new GUIContent("DarkMode"));

            }
        };

        void OnItemInspector(Rect r, SerializedProperty sp, int i)
        {
            using (var ch = new EditorGUI.ChangeCheckScope())
            {

                r.width = (r.width - 10) / 3;

                EditorGUI.PropertyField(r, sp.FindPropertyRelative("key"), new GUIContent(""));

                r.x += r.width + 10 / (3 - 1);
                EditorGUI.PropertyField(r, sp.FindPropertyRelative("defaultIcon"), new GUIContent(""));

                r.x += r.width + 10 / (3 - 1);
                EditorGUI.PropertyField(r, sp.FindPropertyRelative("darkModeIcon"), new GUIContent(""));

                if (ch.changed)
                {
                    sp.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
        };
    }


    public override void OnInspectorGUI()
    {
        iconList?.OnInspectorGUI();
    }
}
