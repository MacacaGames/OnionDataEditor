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

    }

    OnionReorderableList filterList;

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);

        filterList.OnInspectorGUI();
    }



}