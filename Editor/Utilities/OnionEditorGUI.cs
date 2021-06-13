using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    internal static class OnionEditorGUI
    {
        public static bool Toggle(Rect rect, bool value)
        {
            Texture texture = value ?
                OnionDataEditor.GetIconTexture("Toggle_Left") :
                OnionDataEditor.GetIconTexture("Toggle_Right");

            bool buttonClicked = Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition);

            EditorGUI.LabelField(rect, new GUIContent(texture));

            if (buttonClicked)
            {
                value = !value;
                GUI.changed = true;
            }

            return value;
        }

        public static GUIStyle BackgroundOutline(this GUIStyle guiStyle)
        {
            GUIStyle result = new GUIStyle(guiStyle);


            Texture2D texture = OnionDataEditor.GetIconTexture("Outline");

            result.normal.background = texture;
            result.border = new RectOffset(8, 8, 8, 8);

            return result;
        }




    }
}