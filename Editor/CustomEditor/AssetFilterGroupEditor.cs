﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;


[CustomEditor(typeof(AssetFilterGroup))]
public class AssetFilterGroupEditor : Editor
{
    AssetFilterGroup assetFilterGroup;

    private void OnEnable()
    {
        assetFilterGroup = target as AssetFilterGroup;

        filterList = new OnionReorderableList(new SerializedObject(assetFilterGroup).FindProperty("filters"))
        {
            title = "Filters",
            emptyListHint = "No filter",
            customGUI = itemGUI,
            titleIcon = OnionDataEditor.GetIconTexture("Filter"),
        };

        searchFolderList = new OnionReorderableList(new SerializedObject(assetFilterGroup).FindProperty("searchFolders"))
        {
            title = "Search Folders",
            emptyListHint = "Search all folders",
            titleIcon = OnionDataEditor.GetIconTexture("Folder"),
        };


        void itemGUI(Rect r, SerializedProperty sp, int inx)
        {
            if (string.IsNullOrEmpty(assetFilterGroup.filters[inx].value))
            {
                //IS NULL
                GUI.color = new Color(1F, 0.8F, 0.8F);
            }

            const float objWidth = 150;
            const float spaceWidth = 10;

            Rect objRect = r;
            objRect.width = objWidth;

            Rect pathRect = r;
            pathRect.x += objWidth + spaceWidth;
            pathRect.width -= objWidth + spaceWidth;

            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                var o = EditorGUI.EnumPopup(objRect, assetFilterGroup.filters[inx].type);
                if (ch.changed)
                {
                    assetFilterGroup.filters[inx].type = (AssetFilterGroup.FilterType)o;
                }
            }

            using (var ch = new EditorGUI.ChangeCheckScope())
            {
                var p = GUI.TextField(pathRect, assetFilterGroup.filters[inx].value);
                if (ch.changed)
                {
                    assetFilterGroup.filters[inx].value = p;
                }
            }


            GUI.color = Color.white;

        }
    }

    OnionReorderableList filterList;
    OnionReorderableList searchFolderList;

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);

        searchFolderList.OnInspectorGUI();

        GUILayout.Space(10);
        
        filterList.OnInspectorGUI();

    }



}