using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
// using System.Reflection.Emit;
using UnityEngine;
using UnityEditor;

using Random = UnityEngine.Random;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// VO数据处理
    /// </summary>
    internal static class ViewParser
    {
        /// <summary>
        /// 数据节点根名称（不要随便修改，搜索也要用）
        /// </summary>
        public const string ROOT_NODE = "Root";

        #region 常量区域

        /// <summary>
        /// 数据的显示分隔符（尽量不要在数据行内出现）
        /// </summary>
        const char SEPARATOR = ',';// '§'

        /// <summary>
        /// VO文件的命名空间
        /// </summary>
        // static readonly string VO_NAMESPACE = typeof(ItemVO).Namespace;

        /// <summary>
        /// VO获取方法
        /// </summary>
        const string METHOD_GET = "GetVO";

        /// <summary>
        /// VO查询方法
        /// </summary>
        const string METHOD_HAS = "HasVO";

        /// <summary>
        /// VO列表加载方法名
        /// </summary>
        const string METHOD_GET_LIST = "GetVOList";

        /// <summary>
        /// 存储VO的列表名
        /// </summary>
        const string FIELD_LIST = "list_vo";

        /// <summary>
        /// 存储VO的字典名
        /// </summary>
        const string FIELD_DICT = "dic_vo";

        #endregion

        #region 随机数据参数

        /// <summary>
        /// 最小子节点数量
        /// </summary>
        [Obsolete("测试专用")]
        const int minNumChildren = 5;

        /// <summary>
        /// 最大子节点数量
        /// </summary>
        [Obsolete("测试专用")]
        const int maxNumChildren = 10;

        /// <summary>
        /// 当前假数据变成叶子的几率
        /// </summary>
        [Obsolete("测试专用")]
        const float probabilityOfBeingLeaf = 0.5f;

        #endregion

        /// <summary>
        /// 自增行号
        /// </summary>
        static int rowID;

        /// <summary>
        /// 列类型
        /// </summary>
        /// <value></value>
        static readonly Dictionary<string, FieldType> fieldTypeDef = new Dictionary<string, FieldType>
        {
            {"Int32", FieldType.NUMBER},
            {"String", FieldType.STRING},
            {"Boolean", FieldType.BOOL},
            {"Collection", FieldType.LIST},
            {"Object", FieldType.OBJECT}
        };

        /// <summary>
        /// 创造表格根元素
        /// </summary>
        /// <returns></returns>
        public static RowElement CreateTableRoot()
        {
            rowID = 0;
            return new RowElement(ROOT_NODE, -1, rowID, -1);
        }

        /// <summary>
        /// 使用VO模板对VO进行解析并组装成单元格
        /// </summary>
        /// <param name="child"></param>
        /// <param name="voTemplate"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        static RowElement GetRowFromProperties(RowElement child, object voTemplate, List<ColumnField> headers)
        {
            var propInfos = voTemplate.GetType().GetProperties();
            // 解析数据 - 从前向后(数据保持顺序)确认表头的名称与属性
            for (int i = 0, cursor = 0; i < propInfos.Length; i++)
            {
                PropertyInfo attr = propInfos[i];
                if (cursor > headers.Count - 1) break;
                if (string.Compare(headers[cursor].header, attr.Name, true) != 0)
                {
                    continue;
                }
                child.Content.Add(GetCellFromProperties(attr, voTemplate, cursor));
                cursor++;
            }
            return child;
        }

        /// <summary>
        /// 从对象中解析单元格数据
        /// <para>读取Bytes数组用系统加载的方法，因此已经转换过类型，此处相当于某种反向操作。</para>
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        static TreeElementCell GetCellFromProperties(PropertyInfo attr, object element, int index)
        {
            var GenericTypeName = attr.PropertyType.FullName;

            // 集合类数据的处理
            if (GenericTypeName.Contains("Collections"))
            {
                return new TreeElementCell()
                {
                    field = attr.Name,
                    coord = new Vector2(0, index),
                    type = FieldType.LIST, // 显示在表格里的样式类型
                    rawContent = attr.GetValue(element) is IEnumerable list
                        ? string.Join(SEPARATOR.ToString(), list?.Cast<object>().ToArray())
                        : string.Empty
                };
            }

            return new TreeElementCell()
            {
                field = attr.Name,
                coord = new Vector2(0, index),
                type = FieldType.STRING, // 显示在表格里的样式类型
                rawContent = attr.GetValue(element)?.ToString()
            };
        }

        /// <summary>
        /// 从Bytes中读取数据
        /// </summary>
        /// <param name="className">待反射的类名</param>
        /// <param name="bytes">待解析字节</param>
        /// <param name="tableHeaders">表头（用于TreeTable的columns）</param>
        /// <returns></returns>
        public static List<RowElement> AssembleDataFromBytes(
            string className, out List<ColumnField> tableHeaders, byte[] bytes = default)
        {
            tableHeaders = null;
            // Type viewClass = null;
            if (className == null) return null;
            List<RowElement> treeElements = new List<RowElement>();

            // 在Editor模式下预加载VO
            var vo = VOUtilities.InstantiateInEditor<object>(className);
            Type viewClass = vo.GetType();

            // 用反射获取字段生成表头
            tableHeaders = GetHeaderFromViewObject(className, viewClass.GetProperties());

            try
            {
                AXTimerHelper.Mark("VO数据解析");
                // 读取数据列表加载列表元素
                MethodInfo listMethod = viewClass.GetMethod(METHOD_GET_LIST, BindingFlags.Static | BindingFlags.Public);
                IEnumerable<object> list = listMethod.Invoke(vo, null) as IEnumerable<object>;
                treeElements = GetDataFromViewObject(list, tableHeaders);

                AXTimerHelper.Record("VO数据解析");
            }
            catch (Exception ex)
            {
                // 解析错误不要卡住内容
                Debug.LogError("[Editor]: 数据解析错误! " + ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return treeElements;
        }

        /// <summary>
        /// 进度审计(弹出当前数据进度与预期时间)
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public static bool AuditProgress(int current, int total, DateTime startTime, string title = "数据加载中...")
        {
            // 审计
            double minutesRemaining = 0;
            if (current > 0)
            {
                var seconds = (DateTime.Now - startTime).TotalSeconds;
                var multiplier = (float)(total - current) / current;
                minutesRemaining = seconds * multiplier;
            }

            var cancel = EditorUtility.DisplayCancelableProgressBar(
                title,
                $"解析 {current} / {total} 行数据，剩余 {minutesRemaining:0.0} 秒后完成",
                (float)current / total
            );

            if (cancel)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log($"数据加载取消！耗时 {(DateTime.Now - startTime).TotalMinutes:0.0} 秒");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 反射获取VO数据对应的列表行(类似GetVO之类的操作)
        /// </summary>
        /// <param name="viewClass"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RowElement TryGetVOElement(int id, string className, List<ColumnField> headers)
        {
            Type viewClass = Type.GetType($"{className}, Assembly-CSharp");
            MethodInfo hasMethod = viewClass.GetMethod(METHOD_HAS, BindingFlags.Static | BindingFlags.Public);
            MethodInfo getMethod = viewClass.GetMethod(METHOD_GET, BindingFlags.Static | BindingFlags.Public);
            var vo = Activator.CreateInstance(viewClass);
            if (!(bool)hasMethod.Invoke(vo, new object[] { id })) return null;
            object voFields = getMethod.Invoke(vo, new object[] { id });
            getMethod.Invoke(vo, new object[] { id });
            // 离散的数据没有rawIndex和自增行号
            RowElement child = new RowElement($"-", 0, 0, 0)
            {
                Content = new List<TreeElementCell>()
            };
            return GetRowFromProperties(child, voFields, headers);
        }

        /// <summary>
        /// 从VO中提取数据
        /// </summary>
        /// <returns></returns>
        static List<RowElement> GetDataFromViewObject(IEnumerable<object> rows, List<ColumnField> headers)
        {
            var treeElements = new List<RowElement>();
            // 即使是普通表格，根元素按树形表格要求一定要添加，列表加载的时候会处理。
            var root = CreateTableRoot();
            treeElements.Add(root);
            int rawIndex = 0;
            // 统计
            var startTime = DateTime.Now;
            int total = rows.Count();

            foreach (object voFields in rows)
            {
                if (voFields == null) continue;

                // 解析
                rowID += 1;
                RowElement child = new RowElement($"- {rowID}", 0, rowID, rawIndex++)
                {
                    Content = new List<TreeElementCell>()
                };
                treeElements.Add(GetRowFromProperties(child, voFields, headers));

                // 审计
                if (!AuditProgress(rawIndex, total, startTime))
                {
                    return treeElements;
                }
            }

            return treeElements;
        }

        /// <summary>
        /// 从VO中提取表格头
        /// </summary>
        /// <returns></returns>
        static List<ColumnField> GetHeaderFromViewObject(string className, PropertyInfo[] propertyInfos)
        {
            List<ColumnField> _fields = new List<ColumnField>();
            List<(string, string, string)> _headers = new List<(string, string, string)>();

            // 从对象属性中获取列名与字段类型
            foreach (var prop in propertyInfos)
            {
                if (prop.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false) is object[] attrs
                    && attrs.Length > 0
                    && attrs[0] is DescriptionAttribute descAttr)
                {
                    var desc = descAttr.Description;
                    _headers.Add((prop.PropertyType.Name, prop.Name, desc));
                }
            }

            // 检查并生成表头
            if (_headers.Count < 1) return null;

            // 将第一列默认为主键
            _fields.Add(new ColumnField(0, FieldType.PRIMARY, _headers[0].Item2, className, _headers[0].Item3));
            // 剩下数据按类型添加
            for (int i = 1; i < _headers.Count; i++)
            {
                _fields.Add(new ColumnField(i, GetFieldType(_headers[i].Item1), _headers[i].Item2, className, _headers[i].Item3));
            }

            return _fields;
        }

        /// <summary>
        /// 表数据关联(更新两张表的引用数据)
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public static void JoinTableData(List<RowElement> t1, List<RowElement> t2, int cursor = 0)
        {
            var startTime = DateTime.Now;
            // TODO: 目前是Simple Nested-Loop，以后为了性能因切换使用Index Nested-Loop算法
            try
            {
                // TreeTable第一行是根节点，跳过。
                for (int i = 1; i < t1.Count; i++)
                {
                    RowElement leftTableRow = t1[i];

                    // 尝试找到需要Join的字段的值。
                    string jointValue = t1[i].Content[cursor].rawContent;

                    bool hasJoined = false;
                    for (int j = 1; j < t2.Count; j++)
                    {
                        // 仅比较表2主键，表1不填默认表1主键。
                        if (jointValue == t2[j].Content[0].rawContent)
                        {
                            // 主键相同则全体数据加入上张表。
                            leftTableRow.Content.AddRange(t2[j].Content);
                            hasJoined = true;
                            break;
                        }
                    }

                    // 填充空数据
                    if (!hasJoined) leftTableRow.Content.AddRange(Enumerable.Repeat(new TreeElementCell(), t2[1].Content.Count).ToList());

                    AuditProgress(i, t1.Count, startTime, "关联查询中...");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Editor]: 表关联加载失败! 错误:" + ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// 修改运行时VO数据
        /// <param name="className">VO类名</param>
        /// <param name="treeModel">数据行(引用)</param>
        /// <returns>是否修改成功</returns>
        public static bool UpdateViewByRowData(string className, IList<RowElement> rows)
        {

            // 接受修改数据(数据行号，单元格), 因为key允许重复所以不适用字典
            List<KeyValuePair<int, TreeElementCell>> updateRecords = new List<KeyValuePair<int, TreeElementCell>>();

            // 获取修改过的行的记录
            foreach (var rowData in rows)
            {
                if (!rowData.isDirty) continue;
                foreach (var cellData in rowData.Content)
                {
                    if (!cellData.isDirty) continue;
                    // Debug.Log($"[Editor]: 准备修改记录{rowData.rawIndex}的数据{cellData.field} => {cellData.rawContent}");
                    updateRecords.Add(new KeyValuePair<int, TreeElementCell>(rowData.rawIndex, cellData));
                }
            }

            if (updateRecords.Count < 1) return false;

            // 获得运行时的数据
            var viewClass = Type.GetType($"{className}, Assembly-CSharp");
            FieldInfo listVoFieldInfo = viewClass.GetField(FIELD_LIST, BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo dicVoFieldInfo = viewClass.GetField(FIELD_DICT, BindingFlags.NonPublic | BindingFlags.Static);

            // 字典与列表里引用对应的是同一个对象。
            var voList = listVoFieldInfo.GetValue(null) as IEnumerable;
            // var voDicList = (dicVoFieldInfo.GetValue(null) as IDictionary).Values as IEnumerable;

            // 记录
            int cursor = 0;
            HashSet<int> records = new HashSet<int>();

            // 按行号修改列表里的引用
            foreach (object element in voList)
            {
                var itemList = updateRecords.FindAll(item => item.Key == cursor);
                // 未索引成功 = 空集 = 不修改。
                foreach (var item in itemList)
                {
                    var field = viewClass.GetRuntimeProperties().FirstOrDefault(property => property.Name == $"{item.Value.field}");
                    if (item.Value.type == FieldType.LIST)
                    {
                        field.SetValue(element, item.Value.rawContent?.Split(SEPARATOR));
                    }
                    else
                    {
                        field.SetValue(element, item.Value.rawContent);
                    }
                    records.Add(cursor);
                }
                cursor++;
            }

            Debug.Log($"[Editor]: 成功修改记录[{string.Join(SEPARATOR.ToString(), records.ToArray())}]的数据。");
            return true;
        }

        /// <summary>
        /// 获得当前列的类型
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        static FieldType GetFieldType(string keyName)
        {
            if (keyName.IndexOf("Collection", StringComparison.OrdinalIgnoreCase) >= 0) return FieldType.LIST;
            return fieldTypeDef.ContainsKey(keyName) ? fieldTypeDef[keyName] : FieldType.UNDEFINED;
        }

        #region 开发时测试用

        /// <summary>
        /// 生成随机树
        /// </summary>
        [Obsolete("开发时测试用，平时别用！")]
        public static List<RowElement> GenerateRandomTree(int numTotalElements)
        {
            int numRootChildren = numTotalElements / 4;
            rowID = 0;
            var treeElements = new List<RowElement>(numTotalElements);

            var root = new RowElement(ROOT_NODE, -1, rowID, -1);
            treeElements.Add(root);
            for (int i = 0; i < numRootChildren; ++i)
            {
                int allowedDepth = 0;
                AddChildrenRecursive(root, Random.Range(minNumChildren, maxNumChildren), true, numTotalElements, ref allowedDepth, treeElements);
            }

            return treeElements;
        }

        /// <summary>
        /// 递归添加子节点
        /// <para>测试用，目前仅需要1级。</para>
        /// </summary>
        [Obsolete("开发时测试用，平时别用！")]
        static void AddChildrenRecursive(TreeElement element, int numChildren, bool force, int numTotalElements, ref int allowedDepth, List<RowElement> treeElements)
        {
            if (element.Depth >= allowedDepth)
            {
                allowedDepth = 0;
                return;
            }

            for (int i = 0; i < numChildren; ++i)
            {
                if (rowID > numTotalElements) return;

                var child = new RowElement($"- {rowID}", element.Depth + 1, ++rowID, rowID);
                treeElements.Add(child);

                if (!force && Random.value < probabilityOfBeingLeaf) continue;

                // 测试多级目录
                // AddChildrenRecursive(child, Random.Range(minNumChildren, maxNumChildren), false, numTotalElements, ref allowedDepth, treeElements);
            }
        }

        #endregion
    }
}
