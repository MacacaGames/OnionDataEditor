#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;

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
        //NOTE: 可接受非IQueryableData的ScriptableObject
        Object selectObj = Selection.activeObject;
        if (selectObj != null)
        {
            var window = EditorWindow.GetWindow<OnionDataEditorWindow>();
            window.SetTarget(selectObj);
        }
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Object target = EditorUtility.InstanceIDToObject(instanceID);
        if (target is IQueryableData ||
            target is DataGroup)
        {
            OpenWithOnionDataEditor();
            return true;
        }

        return false;
    }

}

#endif