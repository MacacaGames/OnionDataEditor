using UnityEngine;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
{
    [CustomNodeConstructorOf(typeof(DataGroup))]
    internal class DataGroupNodeConstructor : NodeConstructorBase
    {
        public override TreeNode Construct(TreeNode node, Object target)
        {
            node.GetElementTree();

            node.OnInspectorAction = new OnionAction(OnInspectorGUI);

            Init(target);

            return node;
        }



        DataGroup targetDataGroup;

        GUIStyle backgrounStyle;
        Texture2D backgroundTex;

        GUIStyle iconStyle;

        UnityEditor.Editor dataGroupEditor;

        void Init(Object target)
        {
            targetDataGroup = target as DataGroup;

            backgroundTex = MakeTex(new Color(0.3F, 0.1F, 0.1F, 0.5F));

            dataGroupEditor = UnityEditor.Editor.CreateEditor(target);

            Texture2D MakeTex(Color col)
            {
                Texture2D result = new Texture2D(1, 1);
                result.SetPixels(new[] { col });
                result.Apply();
                return result;
            }
        }

        void OnInspectorGUI()
        {
            if (backgrounStyle == null)
            {
                backgrounStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(0, 0, 0, 0)
                };
                backgrounStyle.normal.background = backgroundTex;
            }

            if (iconStyle == null)
            {
                iconStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(5, 5, 5, 5)
                };
                iconStyle.normal.background = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
            }

            if (targetDataGroup.IsDataHaveNull == true)
            {
                ErrorInfo("Data have null!");
            }
            else if (targetDataGroup.IsDataHaveRepeatId == true)
            {
                ErrorInfo("Data have repeat id!");
            }


            dataGroupEditor.OnInspectorGUI();


            void ErrorInfo(string text)
            {

                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope(backgrounStyle, GUILayout.Height(32 + 10)))
                {
                    GUILayout.Label("", iconStyle, GUILayout.Width(32), GUILayout.Height(32));

                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUI.color = new Color(1F, 0.3F, 0.3F);
                        GUILayout.Label(text);
                        GUILayout.FlexibleSpace();
                    }
                }
                GUILayout.Space(10);

                GUI.backgroundColor = new Color(1F, 0.5F, 0.5F);
                GUI.color = Color.white;

            }
        }
    }
}

