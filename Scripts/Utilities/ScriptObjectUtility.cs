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

        public static T CreateAndAddData<T>(QueryableData parentData, string dataPropertyName, string assetName, string assetPath = null) where T : ScriptableObject, IQueryableData
        {
            if (assetPath == null)
            {
                string assetPathAndName = AssetDatabase.GetAssetPath(parentData);
                assetPath = assetPathAndName.Substring(0, assetPathAndName.LastIndexOf('/'));
            }

            assetName = string.IsNullOrEmpty(assetName) ? $"Data {parentData}" : assetName;

            T data = CreateScriptObject<T>(assetPath, assetName);

            AddData(parentData, dataPropertyName, data);

            return data;
        }


        public static T CreateScriptObject<T>(string path, string name) where T : ScriptableObject
        {
            T scriptableObject = ScriptableObject.CreateInstance<T>();
            scriptableObject.name = name;

            string assetNamePath = $"{path}/{name}.asset";
            AssetDatabase.CreateAsset(scriptableObject, assetNamePath);

            return AssetDatabase.LoadAssetAtPath<T>(assetNamePath);
        }


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