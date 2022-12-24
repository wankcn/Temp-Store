using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 定义VO规范的接口
    /// <para>TODO: 如果所有的VO能继承这个接口然后在此拓展，编码可以更加规范！</para>
    /// </summary>
    internal interface IViewObject
    {
        void LoadData();

        void GetVOList();
    }

    /// <summary>
    /// 配置表数据
    /// </summary>
    [Serializable]
    internal class TableView
    {
        /// <summary>
        /// bytes数据
        /// </summary>
        public TextAsset rawData;

        // bytes对应的excel表格名称（映射）
        public string voName;

        protected List<RowElement> m_TableRows = new List<RowElement>();
        public List<RowElement> TableRows
        {
            get { return m_TableRows; }
            set { m_TableRows = value; }
        }

        protected List<ColumnField> m_TableHeaders = new List<ColumnField>();
        public List<ColumnField> TableHeaders
        {
            get { return m_TableHeaders; }
            set { m_TableHeaders = value; }
        }

        // VO数据管理
        public TableView(TextAsset rawText, string rawVoName)
        {
            // byte文件能打开的前提: 1.存在该文件 2.存在该文件的VO.cs
            // 打不开则尝试直接匹配模板，使用外部数据初始化。
            if (rawText == null)
                voName = rawVoName;
            else
                voName = string.IsNullOrEmpty(rawVoName) ? rawText.name : null;

            if (!TryParseVOName(ref voName))
            {
                return;
            }

            // 文件存在的情况下解析数据
            rawData = rawText;

            if (rawData != null)
                Debug.Log($"[Editor]: 加载资源{rawData.name}，大小 -> {rawData.bytes.Length}B");
            else
                Debug.Log($"[Editor]: 使用数据分片加载资源{voName}...");

            if (TableRows.Count == 0)
                TableRows = ViewParser.AssembleDataFromBytes(
                    voName, out m_TableHeaders, rawData == null ? default : rawData.bytes);

            // 测试
            // if (_TreeElements.Count == 0) _TreeElements = RowElementGenerator.GenerateRandomTree(120);
        }

        /// <summary>
        /// 解析VO名称
        /// </summary>
        protected bool TryParseVOName(ref string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            // 套用正确的模板名称
            if (name.LastIndexOf("vo", StringComparison.OrdinalIgnoreCase) is int pos && pos >= 0)
            {
                name = $"{name.Substring(0, pos)}";
            }

            name = VOUtilities.GetResourceVOName($"{VOMapper.VO_ROOT}/{name}VO.cs");
            // 使用模板加载
            if (name == null)
            {
                Debug.LogError("[Editor]: 加载错误，找不到对应的解析模板。");
                return false;
            }

            return true;
        }

    }

    /// <summary>
    /// 关联表数据
    /// </summary>
    internal class JoinedTableView : TableView
    {
        // 关联表名 - 显示的时候以主表为准。
        public string voAltName;

        // 关联表初始化两张表
        public JoinedTableView(string rawVoName, string rawVoAltName, string condition = "") : base(default, rawVoName)
        {
            voAltName = rawVoAltName;
            if (!TryParseVOName(ref voAltName))
            {
                // 这里返回依旧能保留旧表的数据。
                return;
            }

            // 获得关联表的数据
            List<RowElement> alt_TreeElements = ViewParser
                .AssembleDataFromBytes(voAltName, out List<ColumnField> alt_TableHeaders);

            // 表头直接叠加
            TableHeaders.AddRange(alt_TableHeaders);

            // 获得当前字段游标
            int index = TableHeaders.FindIndex(col => col.header.Equals(condition, StringComparison.OrdinalIgnoreCase));
            if (index < 0) index = 0;

            // Join
            ViewParser.JoinTableData(TableRows, alt_TreeElements, index);

        }

    }
}
