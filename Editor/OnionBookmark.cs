using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Onion Data/Bookmark", fileName = "OnionBookmark")]
public class OnionBookmark : ScriptableObject
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
}
