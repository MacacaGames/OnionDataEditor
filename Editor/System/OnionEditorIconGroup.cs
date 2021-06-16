using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    [OpenWithOnionDataEditor(true)]
    internal class OnionEditorIconGroup : ScriptableObject
    {

        [NodeIcon]
        Texture2D Icon => GetIcon("Node_Icon");


        [System.Serializable]
        public class IconInfo : IQueryableData
        {
            public string id;
            public Texture2D defaultIcon;
            public Texture2D darkModeIcon;

            public string GetID() => id;
        }


        [SerializeField]
        IconInfo[] data = new IconInfo[0];

        [NodeCustomElement]
        IEnumerable<TreeNode> DataNodes
        {
            get => data.Select(n => new TreeNode()
            {
                displayName = n.id,
                icon = n.defaultIcon,
            });
        }


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