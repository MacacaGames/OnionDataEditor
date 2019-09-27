using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Onion Data/Bookmark", fileName = "OnionBookmark")]
public class OnionBookmark : QueryableData
{
    [Onion.NodeTitle]
    string bookmarkName => target.name;

    public ScriptableObject target;

    [Onion.NodeOnSelected]
    void OnSelected()
    {
        var onionWindow = EditorWindow.GetWindow<OnionDataEditorWindow>();

        onionWindow.target = target;

    }

    public override string GetID()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<QueryableData> GetData()
    {
        throw new System.NotImplementedException();
    }
}
