using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using OnionCollections;
using OnionCollections.DataEditor;

public static class OnionDataEditor
{

    [MenuItem("Assets/Open with Onion Data Editor")]
    public static void OpenWithOnion()
    {
        ScriptableObject selectObj = Selection.activeObject as ScriptableObject;
        if (selectObj != null)
        {
            var window = OnionDataEditorWindow.ShowWindow();
            window.target = selectObj;
        }
    }

}
