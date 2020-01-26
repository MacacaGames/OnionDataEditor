#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    public class OnionSetting : QueryableData
    {
        [NodeTitle]
        string title => "Settings";

        [NodeIcon]
        Texture2D icon => OnionDataEditorWindow.GetIconTexture("Settings");


        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            throw new System.NotImplementedException();
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
}

#endif
