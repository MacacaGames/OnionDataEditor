using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    [CustomNodeConstructorOf(typeof(AssetFilterGroup))]
    internal class AssetGroupNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            return node;
        }
    }
}