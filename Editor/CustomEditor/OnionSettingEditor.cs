using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OnionCollections.DataEditor.Editor;
using UnityEngine.UIElements;
using System.Linq;


[CustomEditor(typeof(OnionSetting))]
public class OnionSettingEditor : UnityEditor.Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        List<TreeNode> nodes = (target as OnionSetting).ToList();
        foreach (var node in nodes)
        {
            root.Add(new IMGUIContainer(node.OnInspectorAction.action));

            var space = new VisualElement();
            space.style.height = new StyleLength(10);
            root.Add(space);
        }

        return root;
    }

}