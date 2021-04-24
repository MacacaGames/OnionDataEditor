using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;
using System.Linq;

#if (UNITY_EDITOR)
using OnionCollections.DataEditor.Editor;
using UnityEditor;
#endif

[OpenWithOnionDataEditor(true)]
[CreateAssetMenu(menuName = "Onion Data Editor/Asset Group", fileName = "AssetGroup")]
public class AssetFilterGroup : ScriptableObject
{
    [SerializeField]
    string[] matchNames = new string[0];

    [SerializeField]
    string[] matchLabels = new string[0];

    [SerializeField]
    string[] matchTypes = new string[0];

    [SerializeField]
    string[] searchFolders = new string[0];



#if (UNITY_EDITOR)

    [NodeCustomElement]
    IEnumerable<TreeNode> Nodes
    {
        get
        {
            var names = matchNames.Where(n => !string.IsNullOrEmpty(n));
            var labels = matchLabels.Where(n => !string.IsNullOrEmpty(n));
            var types = matchTypes.Where(n => !string.IsNullOrEmpty(n));

            var folders = searchFolders.Where(n => !string.IsNullOrEmpty(n));
            


            if (!names.Any() &&
                !labels.Any() &&
                !types.Any())
            {
                return new[]
                {
                    new TreeNode(TreeNode.NodeFlag.Pseudo)
                    {
                        displayName = "You have to fill at least one match field."
                    }
                };
            }




            string filter = "";

            if (names.Any())
            {
                filter += string.Join(" ", names.Select(n => $"{n}"));
            }

            if (labels.Any())
            {
                filter += string.Join(" ", labels.Select(l => $"l:{l}"));
            }

            if (types.Any())
            {
                filter += string.Join(" ", types.Select(t => $"t:{t}"));
            }


            string[] findResult = null;

            if (folders.Any())
            {
                findResult = AssetDatabase.FindAssets(filter, folders.ToArray());
            }
            else
            {
                findResult = AssetDatabase.FindAssets(filter);
            }

            var result = findResult
                .Select(o =>
                {
                    string path = AssetDatabase.GUIDToAssetPath(o);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    return new
                    {
                        asset,
                        path,
                    };
                })
                .Where(n => n.asset != this)    //篩掉自己
                .Select(m => 
                {
                    var node = new TreeNode(m.asset, TreeNode.NodeFlag.HideElementNodes)
                    {
                        description = m.path,
                    };
                    return node;
                });


            return result;


        }
    }

    [NodeIcon]
    Texture Icon => EditorGUIUtility.IconContent("d_Folder Icon").image;

#endif
}
