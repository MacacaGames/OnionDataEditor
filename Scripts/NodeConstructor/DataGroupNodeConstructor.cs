using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    [CustomConstructorOf(typeof(DataGroup))]
    internal class DataGroupNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            return node;
        }
    }
}

