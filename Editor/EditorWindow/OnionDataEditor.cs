
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    public static class OnionDataEditor
    {
        const string ResourcePath = "OnionDataEditorResource";

        public static string Path
        {
            get
            {
                string rootPath = GetRootPath();

                return rootPath ?? "Packages/com.macacagames.oniondataeditor";
            }
        }

        public static string GetRootPath()
        {
            const string AsmdefQuery = "t:ASMDEF OnionDataEditor.Editor";

            string asmdefGUID = AssetDatabase.FindAssets(AsmdefQuery)[0];
            string asmdefPath = AssetDatabase.GUIDToAssetPath(asmdefGUID);

            DirectoryVisitor directoryVisitor = new DirectoryVisitor(asmdefPath);
            directoryVisitor.Back();

            string rootPath = directoryVisitor.GetPathWithoutSplitChar();

            if (rootPath.StartsWith("Assets/"))
            {
                return rootPath;
            }

            return null;
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

        #region Icon

        static OnionEditorIconGroup iconGroup = null;
        internal static Texture2D GetIconTexture(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey))
                return null;

            if(iconGroup == null)
            {
                iconGroup = AssetDatabase.LoadAssetAtPath<OnionEditorIconGroup>($"{Path}/Editor/IconGroup.asset");
            }

            Texture2D result = iconGroup.GetIcon(iconKey);

            return result;
        }

        public static Texture2D SmallErrorIcon => EditorGUIUtility.FindTexture("console.erroricon.sml");
        public static Texture2D ErrorIcon => EditorGUIUtility.FindTexture("console.erroricon");
        public static Texture2D SmallInfoIcon => EditorGUIUtility.FindTexture("console.infoicon.sml");
        public static Texture2D InfoIcon => EditorGUIUtility.FindTexture("console.infoicon");
        public static Texture2D SmallWarningIcon => EditorGUIUtility.FindTexture("console.warnicon.sml");
        public static Texture2D WarningIcon => EditorGUIUtility.FindTexture("console.warnicon");


        #endregion

        static T AutoCreateLoad<T>(string assetName) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var result = AssetDatabase.LoadAssetAtPath<T>(path);

                return result;
            }
            else
            {
                DirectoryVisitor directoryVisitor = new DirectoryVisitor("Assets/")
                    .CreateFolderIfNotExist(ResourcePath)
                    .Enter(ResourcePath);

                var assetIns = ScriptableObject.CreateInstance<T>();

                string path = $"{directoryVisitor}{assetName}.asset";
                Debug.Log($"Auto create asset : {path}");

                AssetDatabase.CreateAsset(assetIns, path);
                var result = AssetDatabase.LoadAssetAtPath<T>(path);
                
                return result;
            }

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
                var openAttr = t.GetCustomAttribute<OpenWithOnionDataEditorAttribute>(true);
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
}