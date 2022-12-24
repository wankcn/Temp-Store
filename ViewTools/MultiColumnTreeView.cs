using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 多列树状表格
    /// <para>注意: 当前表格数据并非树形，但用了树形表格的样式。</para>
    /// </summary>
    internal class MultiColumnTreeView : TreeTable<RowElement>
    {
        /// <summary>
        /// 行高
        /// </summary>
        const float kRowHeights = 20f;

        /// <summary>
        /// 选择框宽度
        /// </summary>
        const float kToggleWidth = 18f;

        /// <summary>
        /// 是否编辑模式
        /// </summary>
        public bool isEditMode;

        /// <summary>
        /// 列表表头
        /// </summary>
        public static List<ColumnField> tableColumnHeaders;

        /// <summary>
        /// 单元格行图标
        /// <para>用于无法显示或修改的特殊数据类型。</para>
        /// <para>参考: https://github.com/halak/unity-editor-icons</para>
        /// </summary>
        /// <value></value>
        static readonly Texture2D[] s_TestIcons =
        {
            EditorGUIUtility.FindTexture("Prefab Icon"),
            EditorGUIUtility.FindTexture("GameObject Icon"),
            EditorGUIUtility.FindTexture("Folder Icon"),
            EditorGUIUtility.FindTexture("Camera Icon"),
            EditorGUIUtility.FindTexture("AudioSource Icon")
        };

        public MultiColumnTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<RowElement> model) : base(state, multicolumnHeader, model)
        {
            // Enum.GetValues(typeof(TableColumns))
            Assert.AreEqual(tableColumnHeaders?.Count(), multiColumnHeader.state.columns?.Count(), "列表字段必须与列匹配!");
            // 更新自定义设置
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            // 居中设置（详见RowGUI）
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;
            // RowClicked += OnRowClicked;

            Reload();
        }

        #region 列表绘制

        /// <summary>
        /// 多选支持（暂时没用到，可以设置）
        /// </summary>
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        /// <summary>
        /// 构建表格行数据
        /// </summary>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        /// <summary>
        /// 绘制行
        ///<para>注意：当你发现这一行代码出现BUG的时候，不要惊恐。</para>
        ///<para>这是UnityEditor.IMGUI的BUG，截至2020.3.25版本尚未修复。</para>
        ///<para>链接与解决方案: https://giters.com/pardeike/Harmony/issues/399</para>
        ///<para>因为只影响绘制，还有因为try-catch导致的执行效率问题，其它情况下是OK的所以就先让它去了~</para>
        /// </summary>
        protected override void RowGUI(RowGUIArgs args)
        {
            try
            {
                // 行控制一直可见(暂无需要)
                // ControlGUI(item, ref args);
                if (tableColumnHeaders == null) return;

                // 显示的列不一定是数据列则另计数列索引
                // var columnCount = args.GetNumVisibleColumns();
                for (int dataIndex = 0, cellIndex = 0; dataIndex < tableColumnHeaders.Count(); dataIndex++)
                {
                    // 只绘制可见项。
                    if (tableColumnHeaders[dataIndex].isVisible)
                    {
                        var item = args.item as TreeViewItem<RowElement>;
                        CellGUI(cellIndex, item.Data.Content[dataIndex], item, ref args);
                        cellIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                // 绘制不可见项报错并不要紧，不要卡住流程
                Debug.LogError(ex);
            }
        }

        #endregion

        #region 字段处理

        /// <summary>
        /// 创建表头
        /// </summary>
        /// <param name="treeViewWidth"></param>
        /// <returns></returns>
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            // 表头设置
            var columns = tableColumnHeaders.Select(item => item.type switch
            {
                FieldType.PRIMARY => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, EditorGUIUtility.FindTexture("FilterByLabel"), FieldType.PRIMARY.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Right,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    width = 100,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                FieldType.BOOL => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, FieldType.BOOL.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    width = 50,
                    minWidth = 30,
                    maxWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                FieldType.STRING => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, FieldType.STRING.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Right,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    width = 100,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                FieldType.NUMBER => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, FieldType.NUMBER.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Right,
                    sortingArrowAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    width = 100,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                FieldType.LIST => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, EditorGUIUtility.FindTexture("GameObject Icon"), FieldType.LIST.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Center,
                    sortingArrowAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    width = 120,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                _ => new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(item.header, EditorGUIUtility.FindTexture("GameObject Icon"), FieldType.UNDEFINED.ToString()),
                    contextMenuText = $"{item.name}<{item.header}>",
                    headerTextAlignment = TextAlignment.Center,
                    sortingArrowAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    width = 120,
                    minWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            }).ToArray();

            Assert.AreEqual(columns.Length, tableColumnHeaders.Count, "列表列字段数与表头不匹配！");
            return new MultiColumnHeaderState(columns);
        }

        /// <summary>
        /// 绘制单元格
        /// </summary>
        /// <param name="cellRect">单元格UI布局</param>
        /// <param name="row">行数据</param>
        /// <param name="cell">格数据</param>
        /// <param name="args">用于绘制表格的参数(ref)</param>
        void CellGUI(int index, TreeElementCell cell, TreeViewItem<RowElement> row, ref RowGUIArgs args)
        {
            Rect cellRect = args.GetCellRect(index);
            CenterRectUsingSingleLineHeight(ref cellRect);

            // 根据单元格数据类型显示 TODO: 非string型数据使用更高级的方式显示。
            if (index == 0) cell.type = FieldType.PRIMARY;

            // 注册右键菜单
            if (cellRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick)
            {
                OnCellClicked(row.Data, cell);
            }

            switch (cell.type)
            {
                case FieldType.PRIMARY: // 主键不允许编辑！
                    {
                        // 单元格显示的内容
                        string value = cell.rawContent;
                        DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                    }
                    break;
                case FieldType.NUMBER: // 数字类型
                    {
                        if (isEditMode)
                        {
                            cellRect.xMin += 5f; // 编辑模式增加一些宽度
                            // row.Data.Value = EditorGUI.Slider(cellRect, GUIContent.none, item.data.floatValue1, 0f, 1f);
                            string record = cell.rawContent;
                            cell.rawContent = GUI.TextField(cellRect, cell.rawContent);
                            if (cell.rawContent != record) row.Data.isDirty = cell.isDirty = true;
                        }
                        else
                        {
                            // 单元格显示的内容
                            string value = cell.rawContent;
                            DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                        }
                    }
                    break;
                case FieldType.STRING: // 字符串类型
                    {
                        if (isEditMode)
                        {
                            cellRect.xMin += 5f; // 编辑模式增加一些宽度
                            string record = cell.rawContent;
                            cell.rawContent = GUI.TextField(cellRect, cell.rawContent);
                            if (cell.rawContent != record) row.Data.isDirty = cell.isDirty = true;
                        }
                        else
                        {
                            // 单元格显示的内容
                            string value = cell.rawContent;
                            DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                        }
                    }
                    break;
                case FieldType.BOOL: // 布尔类型
                    {
                        if (isEditMode)
                        {
                            cellRect.xMin += 5f; // 编辑模式增加一些宽度
                            bool record = bool.Parse(cell.rawContent);
                            cell.rawContent = GUI.TextField(cellRect, cell.rawContent);
                            if (bool.Parse(cell.rawContent) != record) row.Data.isDirty = cell.isDirty = true;
                        }
                        else
                        {
                            // 单元格显示的内容
                            string value = cell.rawContent;
                            DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                        }
                    }
                    break;
                case FieldType.LIST: // 列表类型
                    {
                        if (isEditMode)
                        {
                            cellRect.xMin += 5f; // 编辑模式增加一些宽度
                            string record = cell.rawContent;
                            cell.rawContent = GUI.TextField(cellRect, cell.rawContent);
                            if (cell.rawContent != record) row.Data.isDirty = cell.isDirty = true;
                        }
                        else
                        {
                            // 单元格显示的内容
                            string value = cell.rawContent;
                            DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                        }
                    }
                    break;
                case FieldType.OBJECT: // 对象型
                    {
                        GUI.DrawTexture(cellRect, s_TestIcons[1], ScaleMode.ScaleToFit);
                    }
                    break;
                default: // 其它或未定义 - 不可修改
                    {
                        GUI.DrawTexture(cellRect, s_TestIcons[0], ScaleMode.ScaleToFit);
                    }
                    break;
            }
        }

        #endregion

        #region 排序控制

        // 点击排序后操作
        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (tableColumnHeaders != null && tableColumnHeaders.Count > 0) SortIfNeeded(rootItem, GetRows());
        }

        // 点击表格元素
        void OnCellClicked(RowElement rowData, TreeElementCell cellData)
        {
            Event.current.Use();
            var rowCtxMenu = new GenericMenu();
            if (cellData.rawContent?.ToString() is string cellContent && !string.IsNullOrEmpty(cellContent))
            {
                rowCtxMenu.AddDisabledItem(new GUIContent(SlashFormat(cellContent)), false);
                rowCtxMenu.AddSeparator(string.Empty);
                rowCtxMenu.AddItem(new GUIContent("复制内容"), false, CopyCellData, cellData);
            }
            rowCtxMenu.AddItem(new GUIContent("复制主键"), false, CopyIndex, rowData);
            rowCtxMenu.AddItem(new GUIContent("复制列名"), false, CopyColData, cellData);
            rowCtxMenu.AddItem(new GUIContent("复制整行"), false, CopyRowData, rowData);
            rowCtxMenu.ShowAsContext();
        }

        // 复制主键
        void CopyIndex(object argument)
        {
            RowElement rowData = (RowElement)argument;
            EditorGUIUtility.systemCopyBuffer = string.Join(" | ", rowData?.Content[0]?.ToString());
        }

        // 复制格数据
        void CopyCellData(object argument)
        {
            TreeElementCell cellData = (TreeElementCell)argument;
            EditorGUIUtility.systemCopyBuffer = cellData.rawContent?.ToString();
        }

        // 复制列名
        void CopyColData(object argument)
        {
            TreeElementCell cellData = (TreeElementCell)argument;
            if (cellData != null
                && (int)cellData.coord.y is int columnIndex
                && tableColumnHeaders != null
                && columnIndex < tableColumnHeaders.Count()
            ) EditorGUIUtility.systemCopyBuffer = tableColumnHeaders[columnIndex].header;
        }

        // 复制行数据
        void CopyRowData(object argument)
        {
            RowElement rowData = (RowElement)argument;
            EditorGUIUtility.systemCopyBuffer = string.Join(" | ", rowData?.Content.Select(item => item.ToString()));
        }

        // 排序
        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1) return;
            if (multiColumnHeader.sortedColumnIndex == -1) return; // 无排序采用默认规则
            // 当前树根元素排序
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        // 多字段排序
        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0) return;

            var myTypes = rootItem.children.Cast<TreeViewItem<RowElement>>();
            // 记录历史，并初始化
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 0; i < sortedColumns.Length; i++)
            {
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);
                // 按先后顺序一路全部排下来(单次排序的长度不足补位保持数字按数值顺序排)
                int maxlen = orderedQuery.Max(l => CheckIfHasContent(l.Data.Content[i]) ? l.Data.Content[i].rawContent.Length : 0);
                orderedQuery = orderedQuery.ThenBy(l => CheckIfHasContent(l.Data.Content[i]) ? l.Data.Content[i].rawContent.PadLeft(maxlen, ' ') : null, ascending);
            }
            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        // 初始化选中列的顺序
        IOrderedEnumerable<TreeViewItem<RowElement>> InitialOrder(IEnumerable<TreeViewItem<RowElement>> myTypes, int[] history)
        {
            var sortOption = tableColumnHeaders[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            // int maxlen = myTypes.Max(l => CheckIfHasContent(l.Data.Content[history[0]]) ? l.Data.Content[history[0]].rawContent.Length : 0);
            return myTypes.Order(l => l.Data.Content[history[0]]?.rawContent, ascending);
        }

        /// <summary>
        /// 将"/"转译以便Unity菜单识别。
        /// <para>WARN: 只有Context型菜单需要转换，因为这个转义的符号并不是原符号(兼容OSX)。</para>
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string SlashFormat(string rawString, params object[] intrpls)
        {
            return string.Format(rawString.Replace(@"/", "\u2215"), intrpls);
        }

        /// <summary>
        /// 保存表格数据
        /// </summary>
        /// <param name="voName">当前加载的VO类名</param>
        public bool SaveChange(string voName)
        {
            // 获得当前table的行数据后保存
            return ViewParser.UpdateViewByRowData(voName, TreeModel.GetData());
        }

        #endregion

        #region 其它处理

        /// <summary>
        /// 检查列字段是否为空
        /// </summary>
        public static bool CheckIfHasContent(TreeElementCell element) => element.rawContent is string content && content != null;

        /// <summary>
        /// 树形结构转列表
        /// </summary>
        /// <param name="root"></param>
        /// <param name="result"></param>
        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null) throw new NullReferenceException("根元素不能为空！");
            if (result == null) throw new NullReferenceException("列表元素不能为空！");

            result.Clear();

            if (root.children == null) return;

            // 利用栈结构将多维数据扁平化（让子列紧随父列）
            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
            {
                stack.Push(root.children[i]);
            }

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 表格功能拓展
    /// </summary>
    internal static class TableExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}