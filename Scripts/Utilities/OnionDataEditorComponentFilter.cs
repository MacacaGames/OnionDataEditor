
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnionCollections.DataEditor
{
    public abstract class OnionDataEditorComponentFilter : MonoBehaviour
    {
        public abstract IEnumerable<Component> GetComponents(GameObject go);
    }
}