using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using OnionCollections.DataEditor.Editor;

public static class OnionDataEditor
{

    [MenuItem("Assets/Open with Onion Data Editor")]
    public static void OpenWithOnion()
    {
        ScriptableObject selectObj = Selection.activeObject as ScriptableObject;
        if (selectObj != null && selectObj is IQueryableData queryableData)
        {
            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();
            window.SetTarget(queryableData);
        }
    }

}
