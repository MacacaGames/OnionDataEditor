#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor;
using OnionCollections.DataEditor.Editor;
using System.Reflection;

public static class OnionDataEditor
{
    public const string path = "Assets/OnionDataEditor";

    static OnionSetting _setting;
    internal static OnionSetting setting
    {
        get
        {
            string _path = $"{path}/Setting.asset";

            if (_setting == null)
                _setting = AutoCreateLoad<OnionSetting>(_path);

            return _setting;
        }
    }

    static OnionBookmarkGroup _bookmarkGroup;
    internal static OnionBookmarkGroup bookmarkGroup
    {
        get
        {
            string _path = $"{path}/BookmarkGroup.asset";

            if (_bookmarkGroup == null)
                _bookmarkGroup = AutoCreateLoad<OnionBookmarkGroup>(_path);

            return _bookmarkGroup;
        }
    }


    static T AutoCreateLoad<T>(string assetPath) where T : ScriptableObject
    {
        var result = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (result == null)
        {
            var bookmarkGroupIns = ScriptableObject.CreateInstance<T>();
            Debug.Log($"Auto create asset : {assetPath}");
            AssetDatabase.CreateAsset(bookmarkGroupIns, assetPath);
            result = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return result;
    }



    [MenuItem("Assets/Open with Onion Data Editor")]
    public static void OpenWithOnionDataEditor()
    {
        UnityEngine.Object selectObj = Selection.activeObject;
        if (selectObj != null)
        {
            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();
            window.SetTarget(selectObj);
        }
    }

    static Dictionary<Type, bool?> openWithDataEditorQuery = new Dictionary<Type, bool?>();

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        UnityEngine.Object target = EditorUtility.InstanceIDToObject(instanceID);

        bool openResult = target is IQueryableData;

        var t = target.GetType();

        if (openWithDataEditorQuery.TryGetValue(t, out bool? result) == false)
        {
            var openAttr = t.GetCustomAttribute<OpenWithOnionDataEditor>(true);
            if (openAttr != null)
            {
                openWithDataEditorQuery.Add(t, openAttr.openWithDataEditor);
            }
            else
            {
                openWithDataEditorQuery.Add(t, null);
            }
        }


        if (openWithDataEditorQuery[t].HasValue)
        {
            openResult = openWithDataEditorQuery[t].Value;
        }


        if (openResult == true)
        {
            OpenWithOnionDataEditor();
            return true;
        }
        else
        {
            return false;
        }
    }

}

#endif