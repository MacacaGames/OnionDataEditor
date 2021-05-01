using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace OnionCollections.DataEditor.Editor
{
    [OpenWithOnionDataEditor(true)]
    internal class OnionEditorIconGroup : ScriptableObject
    {

        [NodeIcon]
        Texture2D Icon => GetIcon("Dot");


        [System.Serializable]
        public class IconInfo : IQueryableData
        {
            public string key;
            public Texture2D defaultIcon;
            public Texture2D darkModeIcon;

            public string GetID() => key;
        }


        [SerializeField]
        IconInfo[] data = new IconInfo[0];



        public Texture2D GetIcon(string key)
        {
            var targetItem = data.QueryByID<IconInfo>(key);

            if(targetItem==null)
            {
                Debug.LogError($"{key}");
            }

            if (EditorGUIUtility.isProSkin == false)
                return targetItem.defaultIcon;

            if (targetItem.darkModeIcon == null)
                return targetItem.defaultIcon;

            return targetItem.darkModeIcon;
        }


    }
}