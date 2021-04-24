﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor.Editor
{
    internal class DefaultNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            return node;
        }
    }
}