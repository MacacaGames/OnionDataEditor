#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{

    public static class ScriptObjectUtility
    {
        /// <summary>
        /// Create script object via asset data base.
        /// And add data in scriptable object target property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentData"></param>
        /// <param name="dataPropertyName"></param>
        /// <param name="assetName"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static T CreateAndAddData<T>(QueryableData parentData, string dataPropertyName, string assetName, string assetPath = null) where T : ScriptableObject, IQueryableData
        {
            if (assetPath == null)
            {
                string assetPathAndName = AssetDatabase.GetAssetPath(parentData);
                assetPath = assetPathAndName.Substring(0, assetPathAndName.LastIndexOf('/'));
            }

            assetName = string.IsNullOrEmpty(assetName) ? $"Data {parentData}" : assetName;

            T data = CreateScriptableObject<T>(assetPath, assetName);

            AddData(parentData, dataPropertyName, data);

            return data;
        }

        /// <summary>
        /// Create script object via asset data base.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="name">Scriptable object's name.</param>
        /// <returns></returns>
        public static T CreateScriptableObject<T>(string path, string name) where T : ScriptableObject
        {
            T scriptableObject = ScriptableObject.CreateInstance<T>();
            scriptableObject.name = name;

            string assetNamePath = $"{path}/{name}.asset";
            AssetDatabase.CreateAsset(scriptableObject, assetNamePath);

            return AssetDatabase.LoadAssetAtPath<T>(assetNamePath);
        }

        /// <summary>
        /// Add data in scriptable object target property.
        /// </summary>
        /// <param name="parentData"></param>
        /// <param name="dataPropertyName"></param>
        /// <param name="data"></param>
        public static void AddData(ScriptableObject parentData, string dataPropertyName, ScriptableObject data)
        {
            SerializedObject serializedObject = new SerializedObject(parentData);
            SerializedProperty serializedProperty = serializedObject.FindProperty(dataPropertyName);

            int index = serializedProperty == null ? 0 : serializedProperty.arraySize;
            serializedProperty.InsertArrayElementAtIndex(index);
            serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = data;

            serializedObject.ApplyModifiedProperties();
        }



    }
}
#endif