#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace OnionCollections.DataEditor.Editor
{
    internal class DataObjTreeView : TreeView
    {
        internal Dictionary<int, TreeNode> treeQuery;
        TreeNode tree;
        TreeViewItem rootViewItem;
        List<TreeViewItem> rowList;

        internal DataObjTreeView(TreeNode tree, TreeViewState state) : base(state)
        {
            rowHeight = 22F;
            isSelectChange = false;

            SetData(tree);
        }
        internal void SetData(TreeNode tree)
        {
            this.tree = tree;

            BuildRoot();

            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            treeQuery = new Dictionary<int, TreeNode>();
            id = 0;

            rootViewItem = TreeNodeToTreeViewItem(id, -1, tree);
            id++;

            BuildTree();

            return rootViewItem;
        }

        void BuildTree()
        {
            rowList = new List<TreeViewItem>();

            Build(tree, rootViewItem);
            //Build(0, tree, result);


            SetupDepthsFromParentsAndChildren(rootViewItem);

            //SetupParentsAndChildrenFromDepths(root, result);
        }

        enum RowState { Normal, Pseudo, Error };
        GUIStyle GetStateGUIStyle(RowState state, TreeViewItem item)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 3)
            };
            switch (state)
            {
                //一般
                case RowState.Normal:
                    break;
                //偽
                case RowState.Pseudo:
                    style.normal.textColor = new Color(0.5F, 0.5F, 0.5F, 0.9F);
                    break;
                //警告
                case RowState.Error:
                    style.normal.textColor = new Color(0.85F, 0.18F, 0.18F);
                    break;
            }
            return style;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            TreeNode node = treeQuery[args.item.id];

            RowState state;
            if (node.IsNull)
                state = RowState.Error;
            else if (node.IsPseudo)
                state = RowState.Pseudo;
            else
                state = RowState.Normal;

            GUIStyle style = GetStateGUIStyle(state, args.item);


            bool hasIcon = node.icon != null;
            //icon
            if (hasIcon)
            {
                int padding = 1;
                float iconHeight = args.rowRect.height - padding * 2;

                Rect iconRect = args.rowRect;
                iconRect.x += GetContentIndent(args.item);
                iconRect.y += padding;
                iconRect.width = iconHeight + 10;
                iconRect.height = iconHeight;

                GUI.Label(iconRect, node.icon);
            }


            //text
            Rect labelRect = args.rowRect;
            labelRect.x += GetContentIndent(args.item) + (hasIcon ? (args.rowRect.height + 4F) : 2F);
            labelRect.width = args.rowRect.width;
            GUI.Label(labelRect, node.displayName, style);


            //child count
            bool isHideElementNodes = node.flag.HasFlag(TreeNode.NodeFlag.HideElementNodes);
            if (treeQuery[args.item.id].ChildCount > 0 || isHideElementNodes)
            {
                const float tagWidth = 150F;
                Rect rightRect = new Rect(args.rowRect.width - tagWidth, args.rowRect.y, tagWidth, args.rowRect.height);
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = new Color(0.5F, 0.5F, 0.5F, 0.5F);
                labelStyle.alignment = TextAnchor.MiddleRight;
                labelStyle.fontSize = 10;
                labelStyle.padding = new RectOffset(0, 8, 0, 3);

                string displayTag;
                if (isHideElementNodes)
                    displayTag = "H";
                else
                    displayTag = treeQuery[args.item.id].ChildCount.ToString();

                GUI.Label(rightRect, $"[ {displayTag} ]", labelStyle);
            }

            if (node.tagColor.a > 0F)
            {
                const float colorTagWidth = 3F;
                Rect colorTagRect = new Rect(args.rowRect.width - colorTagWidth, args.rowRect.y, colorTagWidth, args.rowRect.height - 1);
                EditorGUI.DrawRect(colorTagRect, node.tagColor);
            }

            //Line
            Rect lineRect = args.rowRect;
            lineRect.x += GetContentIndent(args.item);
            lineRect.y += lineRect.height - 1;
            lineRect.height = 1;
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.05F));      //線
        }
        public override IList<TreeViewItem> GetRows()
        {
            return rowList;
        }

        int id = 0;
        TreeViewItem Build(TreeNode node, TreeViewItem parentViewItem)
        {
            TreeViewItem nodeViewItem = TreeNodeToTreeViewItem(id, node);
            rowList.Add(nodeViewItem);
            treeQuery.Add(id, node);

            parentViewItem.AddChild(nodeViewItem);

            id++;

            foreach (var item in node.GetChildren())
            {
                Build(item, nodeViewItem);
            }

            return nodeViewItem;
        }
        TreeViewItem TreeNodeToTreeViewItem(int _id, int _depth, TreeNode node)
        {
            return new TreeViewItem
            {
                id = _id,
                depth = _depth,
                displayName = node.displayName,
            };
        }
        TreeViewItem TreeNodeToTreeViewItem(int _id, TreeNode node)
        {
            return new TreeViewItem
            {
                id = _id,
                displayName = node.displayName,
            };
        }

        //Events
        bool isSelectChange = false;
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            if (isSelectChange)
            {
                isSelectChange = false;
                return;
            }

            EditorWindow.GetWindow<OnionDataEditorWindow>().OnTriggerItem(treeQuery[id]);
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            EditorWindow.GetWindow<OnionDataEditorWindow>().OnDoubleClickItem(treeQuery[id]);
        }
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count > 0)
            {
                isSelectChange = true;
                EditorWindow.GetWindow<OnionDataEditorWindow>().OnTriggerItem(treeQuery[selectedIds[0]]);
            }
        }
    }
}
#endif