using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace Com.Zorro.KomoriLife.Editor
{
    #region 数据加载

    /// <summary>
    /// 单元格基本数据
    /// </summary>
    internal enum FieldType
    {
        [Description("未定义")] UNDEFINED,

        [Description("主键")] PRIMARY,

        [Description("逻辑")] BOOL,

        [Description("数字")] NUMBER,

        [Description("字符串")] STRING,

        [Description("集合")] LIST,

        [Description("对象")] OBJECT
    }

    /// <summary>
    /// 表格列(字段)
    /// </summary>
    [Serializable]
    internal struct ColumnField
    {
        /// <summary>
        /// 全局唯一标识
        /// </summary>
        public int index;

        /// <summary>
        /// 视图
        /// </summary>
        public string view;

        /// <summary>
        /// 表头
        /// </summary>
        public string header;

        /// <summary>
        /// 当前字段中文名称
        /// </summary>
        public string name;

        /// <summary>
        /// 数据类型
        /// </summary>
        public FieldType type;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool isVisible;

        public ColumnField(int index, FieldType type, string header, string view = "", string desc = "")
        {
            this.index = index;
            this.header = header;
            this.type = type;
            this.view = view;
            isVisible = true;
            name = string.IsNullOrEmpty(desc) ? index.ToString() : desc;
        }
    }

    /// <summary>
    /// 表格行数据
    /// <para>该类为数据的实现 + 保存表格行状态</para>
    /// </summary>
    [Serializable]
    internal class RowElement : TreeElement
    {
        /// <summary>
        /// 数据是否被修改
        /// </summary>
        [NonSerialized] public bool isDirty;

        /// <summary>
        /// 当前行是否激活
        /// </summary>
        public bool enabled;

        /// <summary>
        /// 行号
        /// </summary>
        public int rowId;

        /// <summary>
        /// 原始数据行号
        /// </summary>
        public int rawIndex;

        /// <summary>
        /// 行层级
        /// </summary>
        public int level;

        /// <summary>
        /// 当前行名称(默认用ID填充)
        /// </summary>
        public string rowName;

        /// <summary>
        /// 表格行元素
        /// </summary>
        public RowElement(string rowName, int level, int rowId, int rawIndex) : base(rowName, level, rowId)
        {
            this.rowId = rowId;
            this.level = level;
            this.rowName = rowName;
            this.rawIndex = rawIndex;
            // 使用构造生成的数据默认允许操作
            enabled = true;
        }

    }

    #endregion

    #region 表格模型

    /// <summary>
    /// 表格行对应数据模型(单元格可能会有很多不同的类型)
    /// </summary>
    internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
    {
        public T Data { get; set; }

        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            Data = data;
        }
    }

    /// <summary>
    /// 拓展IMGUI的树型表格
    /// <para>泛型定义行类型以便未来可能会拓展不同类型的行</para>
    /// </summary>
    internal class TreeTable<T> : TreeView where T : TreeElement
    {
        TreeModel<T> m_TreeModel;

        /// <summary>
        /// 显示行 - 有最大数量限制
        /// </summary>
        /// <typeparam name="TreeViewItem"></typeparam>
        /// <returns></returns>
        readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>();

        /// <summary>
        /// 数据结构变化
        /// </summary>
        public event Action TreeChanged;

        /// <summary>
        /// 元素点击
        /// </summary>
        public event Action<int> RowClicked;

        public TreeModel<T> TreeModel { get { return m_TreeModel; } }

        // TODO: 拖拽支持
        public event Action<IList<TreeViewItem>> BeforeDroppingDraggedItems;

        public TreeTable(TreeViewState state, TreeModel<T> model) : base(state)
        {
            Init(model);
        }

        public TreeTable(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader)
        {
            Init(model);
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new TreeViewItem<T>(m_TreeModel.Root.Id, depthForHiddenRoot, m_TreeModel.Root.Name, m_TreeModel.Root);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (m_TreeModel.Root == null)
            {
                Debug.LogError("找不到根节点，请检查是否依旧执行SetData()。");
            }

            m_Rows.Clear();
            if (!string.IsNullOrEmpty(searchString))
            {
                Search(m_TreeModel.Root, searchString, m_Rows);
            }
            else
            {
                if (m_TreeModel.Root.HasChildren)
                {
                    AddChildrenRecursive(m_TreeModel.Root, 0, m_Rows);
                }
            }

            // 即使父节点为空也需要初始化，不然绘制布局会报错（如搜索）
            SetupParentsAndChildrenFromDepths(root, m_Rows);

            return m_Rows;
        }

        // 行右键点击
        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
            RowClicked?.Invoke(id);
        }

        void Init(TreeModel<T> model)
        {
            m_TreeModel = model;
            m_TreeModel.ModelChanged += ModelChanged;
        }

        void ModelChanged()
        {
            TreeChanged?.Invoke();
            Reload();
        }

        /// <summary>
        /// 递归设置子元素
        /// </summary>
        void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (T child in parent.Children.Cast<T>())
            {
                var item = new TreeViewItem<T>(child.Id, depth, child.Name, child);
                newRows.Add(item);

                if (child.HasChildren)
                {
                    if (IsExpanded(child.Id))
                    {
                        AddChildrenRecursive(child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        /// <summary>
        /// 表格内搜索功能
        /// </summary>
        void Search(T searchFromThis, string search, List<TreeViewItem> result)
        {
            if (string.IsNullOrEmpty(search)) throw new ArgumentException("搜索条件不能为空！", "search");

            const int kItemDepth = 0; // 搜索的时候降维

            Stack<T> stack = new Stack<T>();
            foreach (var element in searchFromThis.Children)
            {
                stack.Push((T)element);
            }

            while (stack.Count > 0)
            {
                T current = stack.Pop();

                // 搜索 TODO: 算法优化！
                foreach (var item in current.Content)
                {
                    if (item.rawContent.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.Add(new TreeViewItem<T>(current.Id, kItemDepth, current.Name, current));
                        break;
                    }
                }

                if (current.Children != null && current.Children.Count > 0)
                {
                    foreach (var element in current.Children)
                    {
                        stack.Push((T)element);
                    }
                }
            }

            SortSearchResult(result);
        }

        protected virtual void SortSearchResult(List<TreeViewItem> rows)
        {
            // 默认排序方式，可以被其它覆盖。
            rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName));
        }

        protected override IList<int> GetAncestors(int id)
        {
            return m_TreeModel.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return m_TreeModel.GetDescendantsThatHaveChildren(id);
        }

    }

    #endregion
}
