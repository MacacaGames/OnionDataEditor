#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionSetting : QueryableData
    {
        const string nodeName = "Settings";

        [NodeTitle]
        string title => nodeName;

        [NodeDescription]
        string description => "Onion data editor settings.";

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Settings");


        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            return new List<TreeNode>
            {
                userTagsNode,
            };
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
            nodes = new List<TreeNode>((target as QueryableData).GetData<TreeNode>());
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
