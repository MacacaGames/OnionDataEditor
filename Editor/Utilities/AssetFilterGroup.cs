using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnionCollections.DataEditor;
using System.Linq;
using OnionCollections.DataEditor.Editor;
using UnityEditor;

[OpenWithOnionDataEditor(true)]
[CreateAssetMenu(menuName = "Onion Data Editor/Asset Group", fileName = "AssetGroup")]
public class AssetFilterGroup : ScriptableObject
{

    [SerializeField]
    public FilterItem[] filters = new FilterItem[0];

    [SerializeField]
    string[] searchFolders = new string[0];

    public enum FilterType
    {
        NameIncluding = 0,
        TypeIs = 1,
        LableHas = 2,
    }

    [System.Serializable]
    public struct FilterItem
    {
        public FilterType type;
        public string value;
    }


    [NodeCustomElement]
    IEnumerable<TreeNode> Nodes
    {
        get
        {
            const int maxResultAmount = 1000;


            IEnumerable<string> names = filters.Where(n => n.type == FilterType.NameIncluding).Select(n => n.value).Where(n => !string.IsNullOrEmpty(n));
            IEnumerable<string> labels = filters.Where(n => n.type == FilterType.LableHas).Select(n => n.value).Where(n => !string.IsNullOrEmpty(n));
            IEnumerable<string> types = filters.Where(n => n.type == FilterType.TypeIs).Select(n => n.value).Where(n => !string.IsNullOrEmpty(n));

            IEnumerable<string> folders = searchFolders.Where(n => !string.IsNullOrEmpty(n));

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


            if (findResult.Length == 0)
            {
                return new[]
                {
                    new TreeNode(TreeNode.NodeFlag.Pseudo)
                    {
                        displayName = "Can not find any match asset."
                    }
                };
            }

            bool isTooMuchResult = findResult.Length > maxResultAmount;

            var result = findResult
                .Take(maxResultAmount)
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

            if (isTooMuchResult == true)
                return AppendTooMuchNode(result);

            return result;

            IEnumerable<TreeNode> AppendTooMuchNode(IEnumerable<TreeNode> results)
            {
                foreach (var n in results)
                    yield return n;

                var node = new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = $"Too many nodes to show..."
                };

                yield return node;

            }

        }
    }

    [NodeIcon]
    Texture Icon => EditorGUIUtility.IconContent("d_Folder Icon").image;

}
