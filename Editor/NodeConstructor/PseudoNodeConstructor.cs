using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    //Note: 由於 Pseudo Node Constructor 是用 static cache 因此不能在 class 內儲存任何狀態，所有變動只能在 Construct function 內完成
    internal class PseudoNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            return node;
        }
    }
}