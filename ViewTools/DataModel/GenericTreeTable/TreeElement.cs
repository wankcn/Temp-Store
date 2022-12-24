using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 单元格
    /// <para>只读建议使用结构体(速度快), 涉及到编辑功能则必须用类</para>
    /// </summary>
    [Serializable]
    internal class TreeElementCell
    {
        /// <summary>
        /// 是否被修改
        /// </summary>
        [NonSerialized]
        public bool isDirty;

        /// <summary>
        /// 字段名
        /// </summary>
        public string field;

        /// <summary>
        /// 字段列类型
        /// </summary>
        public FieldType type;

        /// <summary>
        /// 当前格字段的内容(字节)
        /// </summary>
        public string rawContent;

        /// <summary>
        /// 当前单元格在列表中的坐标
        /// </summary>
        public Vector2 coord;

        // 重写ToString
        public override string ToString()
        {
            return type == FieldType.LIST ? $"[{rawContent}]" : rawContent;
        }
    }

    /// <summary>
    /// 树状列表行元素
    /// <para>该类为数据的抽象，不关心表格行状态</para>
    /// </summary>
    [Serializable]
    internal abstract class TreeElement
    {
        /// <summary>
        /// 元素行ID
        /// </summary>
        /// <value></value>
        [SerializeField] int m_ID;

        /// <summary>
        /// 元素行名称
        /// </summary>
        /// <value></value>
        [SerializeField] string m_Name;

        /// <summary>
        /// 元素行内容
        /// <para>设置[NonSerialized]则可以在窗口刷新后重置</para>
        /// </summary>
        [SerializeField] List<TreeElementCell> m_Content;

        /// <summary>
        /// 当前行深度
        /// </summary>
        /// <value></value>
        [SerializeField] int m_Depth;

        /// <summary>
        /// 父行
        /// </summary>
        /// <value></value>
        [NonSerialized] TreeElement m_Parent;

        /// <summary>
        /// 子行列表
        /// </summary>
        /// <value></value>
        [NonSerialized] List<TreeElement> m_Children;

        public int Depth
        {
            get { return m_Depth; }
            set { m_Depth = value; }
        }

        public TreeElement Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public List<TreeElement> Children
        {
            get { return m_Children; }
            set { m_Children = value; }
        }

        public List<TreeElementCell> Content
        {
            get { return m_Content; }
            set { m_Content = value; }
        }

        public bool HasChildren
        {
            get { return Children != null && Children.Count > 0; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int Id
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public TreeElement()
        {
        }

        public TreeElement(string name, int depth, int id)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
        }
    }

}
