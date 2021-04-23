
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if(UNITY_EDITOR)
using UnityEditor;
#endif

namespace OnionCollections.DataEditor
{
    public interface IOnionDataEditorGameObjectAgent
    {
        IEnumerable<TreeNode> GetNodes(GameObject go);

        void OnInspectorGUI(TreeNode rootNode);
    }


    /// <summary>
    /// Because game object can not attach custom node attribute, use OnionDataEditorGameObjectAgent to display children node in node editor.
    /// </summary>
    public abstract class OnionDataEditorGameObjectAgent : MonoBehaviour, IOnionDataEditorGameObjectAgent
    {
        /// <summary>
        /// Define your custom method to get children nodes of game object.
        /// </summary>
        /// <param name="go">The target game object.</param>
        /// <returns></returns>
        public abstract IEnumerable<TreeNode> GetNodes(GameObject go);

        public abstract void OnInspectorGUI(TreeNode rootNode);
    }


    internal class DefaultOnionDataEditorGameObjectAgent : IOnionDataEditorGameObjectAgent
    {
        public IEnumerable<TreeNode> GetNodes(GameObject go)
        {
#if(UNITY_EDITOR)
            var nodes = go.GetComponents<Component>()
                .Where(c => c is Transform == false)
                .Select(c => new TreeNode(c)
                {
                    displayName = (c is IQueryableData q) ? q.GetID() : c.GetType().Name
                });

            return nodes;
#else
            return Enumerable.Empty<TreeNode>();
#endif
        }

        const float iconSize = 24F;
        const float bigIconSize = 38F;

        GameObject go = null;

        public void OnInspectorGUI(TreeNode rootNode)
        {

#if (UNITY_EDITOR)

            if (go == null)
                go = (rootNode.Target as GameObject);


            GUIStyle iconStyle = new GUIStyle
            {
                fixedHeight = iconSize,
                fixedWidth = iconSize
            };

            GUIStyle bigIconStyle = new GUIStyle
            {
                fixedHeight = bigIconSize,
                fixedWidth = bigIconSize
            };

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            GUI.enabled = false;


            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image, bigIconStyle);

                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("", GUILayout.Width(2F));
                        GUILayout.Toggle(go.activeSelf, "", GUILayout.Width(20));
                        GUILayout.TextField(rootNode.Target.name);
                    }

                    GUILayout.Label("", GUILayout.Height(0F));

                    using (new EditorGUILayout.HorizontalScope())
                    {

                        GUILayout.Label("Tag", GUILayout.Width(30));
                        EditorGUILayout.TagField(go.tag);

                        GUILayout.Label("", GUILayout.Width(10));

                        GUILayout.Label("Layer", GUILayout.Width(34));
                        EditorGUILayout.LayerField(go.layer);


                    }

                    GUI.enabled = true;
                }
            }

            GUILayout.Label("", GUILayout.Height(10F));

            EditorGUI.DrawRect(new Rect(0, 48, 10000, 1), new Color(0, 0, 0, 0.15F));

            GUILayout.Label("", GUILayout.Height(5F));

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var n in rootNode.GetChildren())
                {
                    if (n.IsPseudo == false && n.IsNull == false)
                    {
                        GUILayout.Label("", GUILayout.Width(1F));
                        GUILayout.Label(EditorGUIUtility.ObjectContent(null, n.Target.GetType()).image, iconStyle);
                    }
                }
            }

#endif
        }
    }

}