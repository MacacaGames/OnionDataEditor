using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;

public static class OnionDataEditor
{

    [MenuItem("Assets/Open with Onion Data Editor")]
    public static void OpenWithOnionDataEditor()
    {
        //NOTE: 可接受非IQueryableData的ScriptableObject
        ScriptableObject selectObj = Selection.activeObject as ScriptableObject;
        if (selectObj != null)
        {
            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();
            window.SetScriptableObjectTarget(selectObj);
        }
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (Selection.activeObject is ScriptableObject && Selection.activeObject is IQueryableData)
        {
            OpenWithOnionDataEditor();
            return true; //catch open file
        }

        return false; // let unity open the file
    }

}
