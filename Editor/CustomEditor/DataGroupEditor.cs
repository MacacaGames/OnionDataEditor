using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataGroup))]
class DataGroupEditor : Editor
{
    DataGroup targetDataGroup;

    GUIStyle backgrounStyle;
    Texture2D backgroundTex;

    GUIStyle iconStyle;

    private void OnEnable()
    {
        targetDataGroup = target as DataGroup;


        backgroundTex = MakeTex(new Color(0.3F, 0.1F, 0.1F, 0.5F));

        Texture2D MakeTex(Color col)
        {
            Texture2D result = new Texture2D(1, 1);
            result.SetPixels(new[] { col });
            result.Apply();
            return result;
        }
    }

    public override void OnInspectorGUI()
    {
        if (backgrounStyle == null)
        {
            backgrounStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };
            backgrounStyle.normal.background = backgroundTex;
        }

        if (iconStyle == null)
        {
            iconStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(5, 5, 5, 5)
            };
            iconStyle.normal.background = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
        }

        if (targetDataGroup.IsDataHaveNull == true)
        {
            ErrorInfo("Data have null!");
        }
        else if (targetDataGroup.IsDataHaveRepeatId == true)
        {
            ErrorInfo("Data have repeat id!");
        }


        base.OnInspectorGUI();


        void ErrorInfo(string text)
        {

            GUILayout.Space(10);
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
}

