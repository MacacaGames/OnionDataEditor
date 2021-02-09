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
    const string AssetsPath = "Assets/OnionDataEditor";
    const string ResourceRootPath = "Assets/Resources";
    const string ResourcePath = "OnionDataEditor";

    //改成 UPM 後要針對不同的匯入方式處理路徑
    public static string path
    {
        get
        {
            // 暫時只有確認 OSX 環境下這個路徑檢查沒問題，Win 有問題的話再來修
            if (System.IO.Directory.Exists(Application.dataPath + AssetsPath.Replace("Assets", "")))
            {
                return AssetsPath;
            }
            return "Packages/com.macacagames.oniondataeditor/";
        }
    }

    static OnionSetting _setting;
    internal static OnionSetting setting
    {
        get
        {
            string assetName = "Setting";

            if (_setting == null)
                _setting = AutoCreateLoad<OnionSetting>(assetName);

            return _setting;
        }
    }

    static OnionBookmarkGroup _bookmarkGroup;
    internal static OnionBookmarkGroup bookmarkGroup
    {
        get
        {
            string assetName = "BookmarkGroup";

            if (_bookmarkGroup == null)
                _bookmarkGroup = AutoCreateLoad<OnionBookmarkGroup>(assetName);

            return _bookmarkGroup;
        }
    }


    static T AutoCreateLoad<T>(string assetName) where T : ScriptableObject
    {

        string _path = $"{ResourceRootPath}/{ResourcePath}/{assetName}.asset";

        var result = Resources.Load<T>($"{ResourcePath}/{assetName}");

        if (result == null)
        {
            DirectoryVisitor n = new DirectoryVisitor("Assets/")
                .CreateFolderIfNotExist("Resources")
                .Enter("Resources")
                .CreateFolderIfNotExist("OnionDataEditor")
                .Enter("OnionDataEditor");

            var bookmarkGroupIns = ScriptableObject.CreateInstance<T>();
            Debug.Log($"Auto create asset : {ResourceRootPath}/{ResourcePath}/{assetName}.asset");

            AssetDatabase.CreateAsset(bookmarkGroupIns, $"{ResourceRootPath}/{ResourcePath}/{assetName}.asset");
            result = Resources.Load<T>($"{ResourcePath}/{assetName}");
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