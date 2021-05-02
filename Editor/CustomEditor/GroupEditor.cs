using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor;
using OnionCollections.DataEditor.Editor;

[CustomEditor(typeof(Group))]
class GroupEditor : Editor
{
    Group targetDataGroup;

    static GUIStyle backgrounStyle;

    static GUIStyle iconStyle;

    private void OnEnable()
    {
        targetDataGroup = target as Group;
    }

    public override void OnInspectorGUI()
    {
        if (targetDataGroup.IsDataHaveNull == true)
        {
            ErrorInfo("Data have null!");
        }

        base.OnInspectorGUI();
    }


    public static void ErrorInfo(string text)
    {
        if (backgrounStyle == null)
        {
            backgrounStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        if (iconStyle == null)
        {
            iconStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(5, 5, 5, 5)
            };
            iconStyle.normal.background = OnionDataEditor.ErrorIcon;
        }

        GUILayout.Space(10);
        GUI.backgroundColor = new Color(1F, 0.4F, 0.4F);
        using (new GUILayout.HorizontalScope(backgrounStyle, GUILayout.Height(32 + 10)))
        {
            GUILayout.Label("", iconStyle, GUILayout.Width(32), GUILayout.Height(32));

            using (new GUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.color = new Color(1F, 0.3F, 0.3F);
                GUILayout.Label(text);
                GUILayout.FlexibleSpace();
            }
        }
        GUILayout.Space(10);

        GUI.backgroundColor = new Color(1F, 0.5F, 0.5F);
        GUI.color = Color.white;

    }
}

