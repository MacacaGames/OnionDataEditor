#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace OnionCollections.DataEditor.Editor
{
    internal class OnionSetting : QueryableData, IEnumerable<TreeNode>
    {
        const string nodeName = "Settings";

        [NodeTitle]
        string title => nodeName;

        [NodeDescription]
        string description => "Onion data editor settings.";

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Settings");


        public override string GetID() => nodeName;

        public IEnumerator<TreeNode> GetEnumerator()
        {
            yield return userTagsNode;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return userTagsNode;
        }

        public string[] userTags = new string[0];

        OnionReorderableList userTagsList;
        TreeNode _userTagsNode;
        [NodeCustomElement]
        TreeNode userTagsNode
        {
            get
            {
                if (_userTagsNode == null)
                {
                    const string propertyTitle = "User Tags";
                    _userTagsNode = new TreeNode(TreeNode.NodeFlag.Pseudo)
                    {
                        displayName = propertyTitle,
                        onInspectorAction = new OnionAction(() =>
                        {
                            if (userTagsList == null)
                            {
                                userTagsList = new OnionReorderableList(
                                    new SerializedObject(this).FindProperty("userTags"),
                                    propertyTitle);
                            }

                            userTagsList.OnInspectorGUI();
                        })
                    };
                }
                return _userTagsNode;
            }
        }

        

    }

    [CustomEditor(typeof(OnionSetting))]
    public class OnionSettingEditor: UnityEditor.Editor
    {
        List<TreeNode> nodes;
        private void OnEnable()
        {
            nodes = (target as OnionSetting).ToList();
        }


        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();

            foreach(var node in nodes)
            {
                ve.Add(new IMGUIContainer(node.onInspectorAction.action));
            }

            return ve;
        }

    }
}

#endif
