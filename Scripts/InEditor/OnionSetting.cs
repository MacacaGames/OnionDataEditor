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
        string title => "Setting";


        public string[] userTags = new string[0];


        public override string GetID()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IQueryableData> GetData()
        {
            throw new System.NotImplementedException();
        }

        /*
        [NodeCustomElement]
        TreeNode userTagsNode
        {
            get
            {
                return new TreeNode(TreeNode.NodeFlag.Pseudo)
                {
                    displayName = "User Tags",
                    onInspectorAction = new OnionAction(() =>
                    {
                        SerializedObject so = new SerializedObject(this);
                        SerializedProperty sp = so.FindProperty("userTags");

                        so.Update();
                        EditorGUILayout.PropertyField(sp);
                        so.ApplyModifiedProperties();

                    })
                };
            }
        }
        */



    }
}

#endif
