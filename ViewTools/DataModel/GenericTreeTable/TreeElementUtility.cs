using System;
using System.Collections.Generic;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 树状列表工具类
    /// </summary>
    internal static class TreeElementUtility
    {
        /// <summary>
        /// 树状结构转列表
        /// </summary>
        public static void TreeToList<T>(T root, IList<T> result) where T : TreeElement
        {
            if (result == null) throw new NullReferenceException("输入列表不能为空！");
            result.Clear();

            Stack<T> stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                T current = stack.Pop();
                result.Add(current);

                if (current.Children != null && current.Children.Count > 0)
                {
                    for (int i = current.Children.Count - 1; i >= 0; i--)
                    {
                        stack.Push((T)current.Children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 列表转树状结构 
        /// <para>! 第一项为必须且深度必须为-1，其它元素的深度必须大于等于0</para>
        /// </summary>
        public static T ListToTree<T>(IList<T> list) where T : TreeElement
        {
            // 输入校验 - 非树状也需要保持结构
            ValidateDepthValues(list);

            // 初始化
            foreach (var element in list)
            {
                element.Parent = null;
                element.Children = null;
            }

            // 设置父-子节点关系
            for (int parentIndex = 0; parentIndex < list.Count; parentIndex++)
            {
                var parent = list[parentIndex];
                bool alreadyHasValidChildren = parent.Children != null;

                if (alreadyHasValidChildren) continue;

                int parentDepth = parent.Depth;
                int childCount = 0;

                // 检查子节点深度
                for (int i = parentIndex + 1; i < list.Count; i++)
                {
                    if (list[i].Depth == parentDepth + 1) childCount++;
                    if (list[i].Depth <= parentDepth) break;
                }

                // 填充子列表
                List<TreeElement> childList = null;
                if (childCount != 0)
                {
                    childList = new List<TreeElement>(childCount); // 分配数量
                    childCount = 0;
                    for (int i = parentIndex + 1; i < list.Count; i++)
                    {
                        if (list[i].Depth == parentDepth + 1)
                        {
                            list[i].Parent = parent;
                            childList.Add(list[i]);
                            childCount++;
                        }

                        if (list[i].Depth <= parentDepth) break;
                    }
                }

                parent.Children = childList;
            }

            return list[0];
        }

        /// <summary>
        /// 检查列表层级深度数据
        /// </summary>
        public static void ValidateDepthValues<T>(IList<T> list) where T : TreeElement
        {
            if (list.Count == 0) throw new ArgumentException("列表中没有元素，请检查列表", "list");

            if (list[0].Depth != -1) throw new ArgumentException($"最外层列表深度必须为-1！ 当前深度: {list[0].Depth}", "list");

            for (int i = 0; i < list.Count - 1; i++)
            {
                int depth = list[i].Depth;
                int nextDepth = list[i + 1].Depth;
                if (nextDepth > depth && nextDepth - depth > 1)
                {
                    throw new ArgumentException($"列表深度信息解析错误，增量不能大于1！ 第{i}元素深度为{depth}且第{i + 1}元素深度为{nextDepth}！");
                }
            }

            for (int i = 1; i < list.Count; ++i)
            {
                if (list[i].Depth < 0) throw new ArgumentException($"第{i}个元素深度错误，只有根元素深度可以为-1！");
            }

            if (list.Count > 1 && list[1].Depth != 0) throw new ArgumentException("第1级列表的深度必须为0", "list");
        }


        /// <summary>
        /// 更新重绘后列表的节点深度数据
        /// </summary>
        /// <param name="root"></param>
        /// <typeparam name="T"></typeparam>
        public static void UpdateDepthValues<T>(T root) where T : TreeElement
        {
            if (root == null) throw new ArgumentNullException("root", "根节点不能为空！");

            if (!root.HasChildren) return;

            Stack<TreeElement> stack = new Stack<TreeElement>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                TreeElement current = stack.Pop();
                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        child.Depth = current.Depth + 1;
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// 检查子节点是否存在父级
        /// </summary>
        static bool IsChildOf<T>(T child, IList<T> elements) where T : TreeElement
        {
            while (child != null)
            {
                child = (T)child.Parent;
                if (elements.Contains(child)) return true;
            }
            return false;
        }

        /// <summary>
        /// 找到共同的父节点的所有元素
        /// </summary>
        public static IList<T> FindCommonAncestorsWithinList<T>(IList<T> elements) where T : TreeElement
        {
            // 只有自己 = 返回包涵自己的集合
            if (elements.Count == 1) return new List<T>(elements);

            List<T> result = new List<T>(elements);
            result.RemoveAll(g => IsChildOf(g, elements));
            return result;
        }
    }

}
