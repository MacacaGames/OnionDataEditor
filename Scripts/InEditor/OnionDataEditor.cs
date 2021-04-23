#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor;
using OnionCollections.DataEditor.Editor;
using System.Reflection;
using System.Linq;

public static class OnionDataEditor
{
    const string AssetsPath = "Assets/OnionDataEditor";
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
    internal static OnionSetting Setting
    {
        get
        {
            const string assetName = "Setting";

            if (_setting == null)
                _setting = AutoCreateLoad<OnionSetting>(assetName);

            return _setting;
        }
    }

    internal static bool IsSetting(TreeNode node)
    {
        return node.IsPseudo == false && node.Target == Setting;
    }


    internal static TreeNode Bookmarks => Setting.BookmarksNode;

    internal static bool IsBookmark(TreeNode node)
    {
        return node.tags.Contains("Bookmarks");
    }




    internal static Texture2D GetIconTexture(string iconName)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>($"{path}/Editor/Icons/{iconName}.png");
    }


    static T AutoCreateLoad<T>(string assetName) where T : ScriptableObject
    {
        var result = Resources.Load<T>($"{ResourcePath}/{assetName}");

        if (result == null)
        {
            DirectoryVisitor directoryVisitor = new DirectoryVisitor("Assets/")
                .CreateFolderIfNotExist("Resources")
                .Enter("Resources")
                .CreateFolderIfNotExist(ResourcePath)
                .Enter(ResourcePath);

            var assetIns = ScriptableObject.CreateInstance<T>();
            Debug.Log($"Auto create asset : {directoryVisitor}{assetName}.asset");

            AssetDatabase.CreateAsset(assetIns, $"{directoryVisitor}/{assetName}.asset");
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



    static readonly Dictionary<Type, bool?> openWithDataEditorQuery = new Dictionary<Type, bool?>();

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        UnityEngine.Object target = EditorUtility.InstanceIDToObject(instanceID);

        var t = target.GetType();

        if (openWithDataEditorQuery.TryGetValue(t, out bool? queryResult) == false)
        {
            var openAttr = t.GetCustomAttribute<OpenWithOnionDataEditor>(true);
            queryResult = openAttr?.openWithDataEditor;
            openWithDataEditorQuery.Add(t, queryResult);
        }

        bool openResult;
        if (queryResult.HasValue)
        {
            openResult = openWithDataEditorQuery[t].Value;
        }
        else
        {
            openResult = target is IQueryableData;
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