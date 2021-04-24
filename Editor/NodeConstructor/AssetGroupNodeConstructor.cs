using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    [CustomConstructorOf(typeof(AssetFilterGroup))]
    internal class AssetGroupNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            return node;
        }
    }
}