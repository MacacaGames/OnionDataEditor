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
        const float leftPadding = 14;
        const float space = 3;

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
                var rects = r.ExtendLeft(-leftPadding).HorizontalSplit(3, space);

                EditorGUI.LabelField(rects[0], new GUIContent("Key"));

                EditorGUI.LabelField(rects[1], new GUIContent("Default"));

                EditorGUI.LabelField(rects[2], new GUIContent("DarkMode"));
            }
        };

        void OnItemInspector(Rect r, SerializedProperty sp, int i)
        {
            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                var rects = r.HorizontalSplit(3, space);

                EditorGUI.PropertyField(rects[0], sp.FindPropertyRelative("key"), GUIContent.none);

                EditorGUI.PropertyField(rects[1], sp.FindPropertyRelative("defaultIcon"), GUIContent.none);

                EditorGUI.PropertyField(rects[2], sp.FindPropertyRelative("darkModeIcon"), GUIContent.none);

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
