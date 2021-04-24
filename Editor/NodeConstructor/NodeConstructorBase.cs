using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    public abstract class NodeConstructorBase
    {
        public abstract TreeNode Construct(TreeNode node, Object target);
    }
}