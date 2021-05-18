using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;


[CustomEditor(typeof(PlayerPrefGroup))]
public class PlayerPrefGroupEditor : Editor
{
    PlayerPrefGroup playerPrefGroup;

    private void OnEnable()
    {
        playerPrefGroup = target as PlayerPrefGroup;

        filterList = new OnionReorderableList(new SerializedObject(playerPrefGroup).FindProperty("regexFilter"))
        {
            title = "Regex Filters",
            titleIcon = OnionDataEditor.GetIconTexture("Filter"),
        };

        customGUIList = new OnionReorderableList(new SerializedObject(playerPrefGroup).FindProperty("customGUI"))
        {
            title = "Custom GUI Type",
            titleIcon = OnionDataEditor.GetIconTexture("Filter"),
            customGUI = CustomItemGUI,
        };


        void CustomItemGUI(Rect r,SerializedProperty sp, int index)
        {
            var rs = r.HorizontalSplit(2, 4);

            EditorGUI.PropertyField(rs[0], sp.FindPropertyRelative("regexFilter"), GUIContent.none);
            EditorGUI.PropertyField(rs[1], sp.FindPropertyRelative("guiType"), GUIContent.none);

        }

    }

    OnionReorderableList filterList;
    OnionReorderableList customGUIList;

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);

        filterList.OnInspectorGUI();

        GUILayout.Space(10);

        customGUIList.OnInspectorGUI();

        playerPrefGroup.IsCustomGUIDirty = true;
    }



}