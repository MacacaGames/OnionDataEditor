
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor
{
    public abstract class OnionDataEditorGameObjectAgent : MonoBehaviour
    {
        public abstract IEnumerable<TreeNode> GetNodes(GameObject go);
    }
}