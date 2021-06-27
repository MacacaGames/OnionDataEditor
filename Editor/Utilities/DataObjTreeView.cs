
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

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
            rowHeight = 25F;

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

            SetupDepthsFromParentsAndChildren(rootViewItem);
        }

        enum RowState { Normal, Pseudo, Error };
        GUIStyle GetStateGUIStyle(RowState state, TreeViewItem item)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 3),
                fontSize = 12,
            };

            switch (state)
            {
                case RowState.Normal:
                    break;

                case RowState.Pseudo:
                    style.normal.textColor = new Color(0.5F, 0.5F, 0.5F, 1F);
                    break;

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

            DrawNodeRowGUI();

            DrawIcon();

            DrawTitle();

            DrawRightSideInfo();

            DrawTagColor();

            //Line
            Rect lineRect = args.rowRect
                .ExtendLeft(-GetContentIndent(args.item))
                .ExtendUpToFit(1);

            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.05F));      //線


            void DrawNodeRowGUI()
            {
                const float leftSpace = 200F;
                const float rightSpace = 40F;

                if (Event.current.type == EventType.Repaint &&
                    args.rowRect.width < leftSpace + rightSpace)
                {
                    return;
                }

                Rect rect = args.rowRect
                    .ExtendLeft(-leftSpace)
                    .ExtendRight(-rightSpace);

                //EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.3F)); //For Debug

                node.OnRowGUI?.Invoke(rect);

            }

            void DrawIcon()
            {
                const float padding = 1F;
                if (hasIcon)
                {
                    float iconHeight = args.rowRect.height - padding * 2;

                    Rect iconRect = new Rect(args.rowRect)
                        .ExtendLeft(-GetContentIndent(args.item))
                        .SetSize(iconHeight, iconHeight);


                    GUI.Label(iconRect, node.icon);
                }
            }

            void DrawTitle()
            {
                Rect labelRect = new Rect(args.rowRect)
                   .ExtendLeft(-GetContentIndent(args.item))
                   .ExtendLeft(-(hasIcon ? (args.rowRect.height + 4F) : 2F));

                GUI.Label(labelRect, node.displayName, style);
            }

            void DrawRightSideInfo()
            {
                bool isHideElementNodes = node.Flag.HasFlag(TreeNode.NodeFlag.HideElementNodes);
                if (treeQuery[args.item.id].ChildCount > 0 || isHideElementNodes)
                {
                    const float tagWidth = 40F;
                    Rect rightRect = new Rect(args.rowRect)
                        .ExtendLeft(-args.rowRect.width)
                        .ExtendLeft(tagWidth);

                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        normal = {
                            textColor = new Color(0.5F, 0.5F, 0.5F, 0.5F),
                        },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 10,
                        padding = new RectOffset(0, 0, 0, 0),
                        border = new RectOffset(16, 16, 16, 16),
                    };

                    if (isHideElementNodes)
                    {
                        var icon = OnionDataEditor.GetIconTexture("HideElement");
                        GUI.color = new Color(1, 1, 1, 0.5F);
                        GUI.Label(
                            rightRect.ExtendLeft(-rightRect.width + rightRect.height).MoveLeft(8),
                            new GUIContent(icon, "Hide Elements"));
                        GUI.color = Color.white;
                    }
                    else
                    {
                        string displayTag = treeQuery[args.item.id].ChildCount.ToString();
                        GUI.Label(rightRect.ExtendHorizontal(-5).ExtendVertical(-2), $"[ {displayTag} ]", labelStyle);
                    }
                }
            }

            void DrawTagColor()
            {
                if (node.tagColor.a > 0F)
                {
                    const float colorTagWidth = 3F;
                    Rect colorTagRect = new Rect(args.rowRect)
                        .ExtendLeft(-args.rowRect.width)
                        .ExtendLeft(colorTagWidth)
                        .ExtendDown(-1);

                    EditorGUI.DrawRect(colorTagRect, node.tagColor);
                }

            }

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

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);

            if (Event.current == null || Event.current.type != EventType.ContextClick)
            {
                return;
            }

            if (Event.current.button == 1)
            {
                OnRightClickMenu(treeQuery[id]);
            }

            if (isSelectChange)
            {
                isSelectChange = false;
                return;
            }

            EditorWindow.GetWindow<OnionDataEditorWindow>().OnTriggerItem(treeQuery[id]);


            void OnRightClickMenu(TreeNode node)
            {
                if (node.NodeActions == null || node.NodeActions.Count() == 0)
                    return;


                var menu = new OnionMenu();

                foreach (var action in node.NodeActions)
                {
                    menu.AddItem(action);
                }

                menu.Show();

                //var menu = new GenericMenu
                //{
                //    allowDuplicateNames = true,
                //};

                //foreach(var action in node.NodeActions)
                //{
                //    menu.AddItem(new GUIContent(action.actionName), false, ()=> { action.action(); });
                //}

                //menu.ShowAsContext();
            }
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

        public TreeNode GetSelectedNode()
        {
            return treeQuery[GetSelection()[0]];
        }

        public void SelectAt(TreeNode node)
        {
            var queryResult = treeQuery.Where(n => n.Value == node);
            if (queryResult.Any())
            {
                int id = queryResult.First().Key;
                ContextClickedItem(id);
                SelectionChanged(new List<int> { id });

                SetSelection(new List<int> { id });
            }
        }

        public void SetState(TreeViewState state)
        {
            if (state == null)
            {
                return;
            }

            SetSelection(state.selectedIDs);

            CollapseAll();
            SetExpanded(state.expandedIDs);
        }
    }
}