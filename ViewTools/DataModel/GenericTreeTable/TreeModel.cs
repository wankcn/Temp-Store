using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 树形表格模型 - 基于IMGUI.Treeview开发
    /// <para>该类用于处理树状列表元素的深度与顺序。</para>
    /// <para>调用的列表数据被Unity序列化处理。</para>
    /// <para>列表数据使用工具TreeElementUtility.ListToTree方法构建。</para>
    /// <para>根节点深度必须为-1，其它节点>=0。</para>
    /// </summary>
    internal class TreeModel<T> where T : TreeElement
    {
        IList<T> m_Data;
        T m_Root;
        int m_MaxID;

        public T Root { get { return m_Root; } set { m_Root = value; } }
        public event Action ModelChanged;
        public int NumberOfDataElements
        {
            get { return m_Data.Count; }
        }

        public TreeModel(IList<T> data)
        {
            SetData(data);
        }

        public T Find(int id)
        {
            return m_Data.FirstOrDefault(element => element.Id == id);
        }

        public IList<T> GetData()
        {
            return m_Data;
        }

        public void SetData(IList<T> data)
        {
            m_Data = data ?? throw new ArgumentNullException("data", "表格数据不能为空！");

            if (m_Data.Count > 0) m_Root = TreeElementUtility.ListToTree(data);

            m_MaxID = m_Data.Max(e => e.Id);
        }

        public int GenerateUniqueID()
        {
            return ++m_MaxID;
        }

        public IList<int> GetAncestors(int id)
        {
            var parents = new List<int>();
            TreeElement T = Find(id);
            if (T != null)
            {
                while (T.Parent != null)
                {
                    parents.Add(T.Parent.Id);
                    T = T.Parent;
                }
            }
            return parents;
        }

        public IList<int> GetDescendantsThatHaveChildren(int id)
        {
            T searchFromThis = Find(id);
            if (searchFromThis != null)
            {
                return GetParentsBelowStackBased(searchFromThis);
            }
            return new List<int>();
        }

        IList<int> GetParentsBelowStackBased(TreeElement searchFromThis)
        {
            Stack<TreeElement> stack = new Stack<TreeElement>();
            stack.Push(searchFromThis);

            var parentsBelow = new List<int>();
            while (stack.Count > 0)
            {
                TreeElement current = stack.Pop();
                if (current.HasChildren)
                {
                    parentsBelow.Add(current.Id);
                    foreach (var T in current.Children)
                    {
                        stack.Push(T);
                    }
                }
            }

            return parentsBelow;
        }

        public void RemoveElements(IList<int> elementIDs)
        {
            IList<T> elements = m_Data.Where(element => elementIDs.Contains(element.Id)).ToArray();
            RemoveElements(elements);
        }

        public void RemoveElements(IList<T> elements)
        {
            foreach (var element in elements)
                if (element == m_Root)
                    throw new ArgumentException("根元素不允许移除。");

            var commonAncestors = TreeElementUtility.FindCommonAncestorsWithinList(elements);

            foreach (var element in commonAncestors)
            {
                element.Parent.Children.Remove(element);
                element.Parent = null;
            }

            TreeElementUtility.TreeToList(m_Root, m_Data);

            Changed();
        }

        public void AddElements(IList<T> elements, TreeElement parent, int insertPosition)
        {
            if (elements == null) throw new ArgumentNullException("elements", "添加元素不能为空！");
            if (elements.Count == 0) throw new ArgumentNullException("elements", "没有添加任何元素！");
            if (parent == null) throw new ArgumentNullException("parent", "找不到父元素！");

            if (parent.Children == null) parent.Children = new List<TreeElement>();
            parent.Children.InsertRange(insertPosition, elements.Cast<TreeElement>());

            foreach (var element in elements)
            {
                element.Parent = parent;
                element.Depth = parent.Depth + 1;
                TreeElementUtility.UpdateDepthValues(element);
            }

            TreeElementUtility.TreeToList(m_Root, m_Data);

            Changed();
        }

        public void AddRoot(T root)
        {
            if (root == null) throw new ArgumentNullException("root", "根元素为空！");
            if (m_Data == null) throw new InvalidOperationException("数据列表为空！");
            if (m_Data.Count != 0) throw new InvalidOperationException("空列表不允许添加元素！");

            root.Id = GenerateUniqueID();
            root.Depth = -1;
            m_Data.Add(root);
        }

        public void AddElement(T element, TreeElement parent, int insertPosition)
        {
            if (element == null) throw new ArgumentNullException("element", "元素不能为空！");
            if (parent == null) throw new ArgumentNullException("parent", "父元素不能为空！");

            if (parent.Children == null) parent.Children = new List<TreeElement>();
            parent.Children.Insert(insertPosition, element);
            element.Parent = parent;

            TreeElementUtility.UpdateDepthValues(parent);
            TreeElementUtility.TreeToList(m_Root, m_Data);

            Changed();
        }

        public void MoveElements(TreeElement parentElement, int insertionIndex, List<TreeElement> elements)
        {
            if (insertionIndex < 0) throw new ArgumentException("输入错误: 插入索引为-1");

            // 没有移动到其它元素下
            if (parentElement == null) return;

            // 确保所有元素在插入前已经移除，之后调整元素索引
            if (insertionIndex > 0) insertionIndex -= parentElement.Children.GetRange(0, insertionIndex).Count(elements.Contains);

            // 拖拽移动，脱离父元素
            foreach (var draggedItem in elements)
            {
                draggedItem.Parent.Children.Remove(draggedItem);
                draggedItem.Parent = parentElement; // 设置新的父元素
            }

            if (parentElement.Children == null)
                parentElement.Children = new List<TreeElement>();

            // 将元素插入新的父元素下
            parentElement.Children.InsertRange(insertionIndex, elements);

            TreeElementUtility.UpdateDepthValues(Root);
            TreeElementUtility.TreeToList(m_Root, m_Data);

            Changed();
        }

        void Changed()
        {
            ModelChanged?.Invoke();
        }
    }

}
