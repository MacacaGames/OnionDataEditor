
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor
{
    /// <summary>
    /// Because game object can not attach custom node attribute, use OnionDataEditorGameObjectAgent to display children node in node editor.
    /// </summary>
    public abstract class OnionDataEditorGameObjectAgent : MonoBehaviour
    {
        /// <summary>
        /// Define your custom method to get children nodes of game object.
        /// </summary>
        /// <param name="go">The target game object.</param>
        /// <returns></returns>
        public abstract IEnumerable<TreeNode> GetNodes(GameObject go);
    }
}