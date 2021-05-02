using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor;
using OnionCollections.DataEditor.Editor;

[CustomEditor(typeof(QueryableGroup))]
class QueryableGroupEditor : Editor
{
    QueryableGroup targetDataGroup;

    private void OnEnable()
    {
        targetDataGroup = target as QueryableGroup;
    }

    public override void OnInspectorGUI()
    {
        if (targetDataGroup.IsDataHaveNull == true)
        {
            GroupEditor.ErrorInfo("Data have null!");
        }
        else if (targetDataGroup.IsDataHaveRepeatId == true)
        {
            GroupEditor.ErrorInfo("Data have duplicate id!");
        }

        base.OnInspectorGUI();
    }
}

