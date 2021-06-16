
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace OnionCollections.DataEditor.Editor
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
            var nodes = go.GetComponents<Component>()
                .Select(c =>
                {
                    UnityEditor.Editor customEditor = null;
                    var node = new TreeNode(c)
                    {
                        displayName = (c is IQueryableData q) ? q.GetID() : c.GetType().Name,
                        OnInspectorGUI = () => OnComponentInspectorGUI(c, customEditor),
                    };

                    return node;
                });

            return nodes;
        }

        const float iconSize = 24F;
        const float bigIconSize = 38F;

        GameObject go = null;

        Dictionary<Component, bool> compFoldState = new Dictionary<Component, bool>();

        public void OnInspectorGUI(TreeNode rootNode)
        {

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

                        GUILayout.Label("Layer", GUILayout.Width(38));
                        EditorGUILayout.LayerField(go.layer);
                    }

                    GUI.enabled = true;
                }
            }

            GUILayout.Label("", GUILayout.Height(10F));

            foreach (var n in rootNode.GetChildren())
            {
                if (n.IsPseudo == false && n.IsNull == false)
                {
                    n.OnInspectorGUI();
                }
            }

        }

        void OnComponentInspectorGUI(Component comp, UnityEditor.Editor customEditor)
        {
            if (customEditor == null)
            {
                customEditor = UnityEditor.Editor.CreateEditor(comp);
            }

            if( compFoldState.TryGetValue(comp, out bool isFoldOut) == false)
            {
                isFoldOut = true;
                compFoldState.Add(comp, isFoldOut);
            }

            compFoldState[comp] = EditorGUILayout.InspectorTitlebar(isFoldOut, comp);
            if (isFoldOut)
            {
                GUILayout.Space(3);
                customEditor.OnInspectorGUI();
                GUILayout.Space(10);
            }
        }
    }

}