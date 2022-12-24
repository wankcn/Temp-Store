using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 数据表检查工具 - 使用说明
    /// <para>只是单纯放一些说明到这里。与其写一大堆注释，不如直接在此处大致了解功能。</para>
    /// <para>使用过程中有啥疑问，鼠标悬浮到UI元素上，很多都会提示的。</para>
    /// </summary>
    internal static class VEStr
    {
        public const string WINDOW_NAME = "检查配置表";

        public const string MSG_INTRO_0 = "修改功能暂时无法使用。";
        public const string MSG_INTRO_1 = "对比功能仅支持旧表数据。";
        public const string MSG_INTRO_2 = "老数据依旧可以双击bytes打开，并且正确加载匹配的字段。";
        public const string MSG_TIP_0 = "支持模糊搜索所有单元格数据，可通过排序筛选最接近项。";
        public const string MSG_TIP_1 = "使用指定VO类名加载总表的分片数据。";
        public const string MSG_TIP_2 = "在编辑模式下可动态修改已加载的数据，但不会保存。";
        public const string MSG_TIP_3 = "匹配最接近的记录, 搜索数据分片集。";
        public const string MSG_TIP_4 = "重新加载整个列表，编辑器编译后则重新初始化面板。";
        public const string MSG_TIP_5 = "通过bytes打开的表，点击可查看原文件。";
        public const string MSG_TIP_6 = "关联模式下，可使用主表任意字段关联目标表的主键带出对应行数据";
        public const string MSG_OPEN_FAIL = "未能根据搜索信息找到匹配的数据表。";
        public const string MSG_SAVE_FAIL = "未修改数据。";

        public const string UI_HEADER_BLANK = "[当前无数据，请输入表名或双击{0}<文件名>.bytes加载]";
        public const string UI_SQL_FLD = "字段，不填则默认t1表主键";
        public const string UI_COL_HIDEALL = "关闭所有列";
        public const string UI_COL_SHOWALL = "开启所有列";
        public const string UI_COL_GUESS = "显示可能带名称的列";
    }

    /// <summary>
    /// 数据表检查工具 - by HNC
    /// </summary>
    internal class VOEditorWindow : EditorWindow
    {
        #region 静态数据区
        const string CONFIG_PATH = "Data/BaseConfig/AppConfig";
        const string GUI_BTN_STYLE = "miniButton";// 按钮样式
        #endregion

        /// <summary>
        /// 上次加载的实例ID
        /// </summary>
        static TextAsset lastInstance;

        /// <summary>
        /// 项目通常设置
        /// </summary>
        static AppConfig genericConfig;

        /// <summary>
        /// 特殊覆盖VO名称
        /// </summary>
        static string altVoClass;

        /// <summary>
        /// 是否按显示自定义VO解析类
        /// </summary>
        [NonSerialized] bool isAltVoLoader;

        /// <summary>
        /// 是否按名称加载
        /// </summary>
        [NonSerialized] bool isLoadByName;

        /// <summary>
        /// 是否已加载数据
        /// </summary>
        [NonSerialized] bool isInitialized;

        #region 编辑器序列化保留数据

        [SerializeField] TreeViewState m_TreeViewState;

        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;

        [SerializeField] TableView tableView; // 加载资源

        [SerializeField] int[] inVisibleCols; // 隐藏列
        public int[] InVisibleCols
        {
            get { return inVisibleCols; }
            set
            {
                isInitialized = false;
                inVisibleCols = value;
            }
        }

        #endregion

        // 搜索文件名
        string voSearchName;

        // 列表搜索
        SearchField m_SearchField;

        // 主列表
        MultiColumnTreeView m_TreeView;
        public MultiColumnTreeView TreeView
        {
            get { return m_TreeView; }
        }

        // 关联模式
        bool isJoinMode;

        // 列表搜索字段
        readonly string[] searchTokens = new string[3];

        /// <summary>
        /// 解析的文件
        /// </summary>
        /// <value></value>
        public string DependencyPath
        {
            // 新版配表位置:
            get { return $"Assets/Res/Excel/{(genericConfig == null ? string.Empty : genericConfig.defaultStartLang + "/")}"; }
        }

        #region 元素UI布局
        Rect DescViewRect
        {
            get { return new Rect(20, 5, position.width - 40, 16); }
        }

        Rect ToolbarRect
        {
            get { return new Rect(20, 25, position.width - 40, 20); }
        }

        Rect TableViewRect
        {
            get { return new Rect(20, 45, position.width - 40, position.height - 70); }
        }

        Rect BottomToolbarRect
        {
            get { return new Rect(20, position.height - 18, position.width - 40, 16); }
        }
        #endregion

        /// <summary>
        /// 打开编辑器窗口
        /// <para>- 图标参考: https://github.com/halak/unity-editor-icons</para>
        /// </summary>
        [MenuItem("Tools/" + VEStr.WINDOW_NAME)]
        static VOEditorWindow GetWindow()
        {
            var window = GetWindow<VOEditorWindow>();
            window.titleContent = new GUIContent(VEStr.WINDOW_NAME, EditorGUIUtility.FindTexture("Profiler.UIDetails"));
            window.minSize = new Vector2(512, 320);
            window.Focus();
            window.Repaint();
            return window;
        }

        void OnEnable()
        {
            genericConfig = Resources.Load<AppConfig>(CONFIG_PATH);
            VOMapper.Refresh();
        }

        /// <summary>
        /// 点击了项目资源执行此处
        /// </summary>
        [OnOpenAsset]
        static bool OnOpenAsset(int instanceID, int _)
        {
            lastInstance = EditorUtility.InstanceIDToObject(instanceID) as TextAsset;
            if (lastInstance == null || lastInstance.hideFlags != HideFlags.NotEditable) return false;
            // 加载成功 = 自行处理, 失败 = 交给Unity处理
            return LoadData(lastInstance);
        }

        void OnGUI()
        {
            TryPrepareTableData();
            // 即使没有数据也需要一直绘制界面。
            DrawDescription(DescViewRect);
            DrawSearchBar(ToolbarRect);
            DrawTreeView(TableViewRect);
            DrawBottomToolBar(BottomToolbarRect);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="instanceID"></param>
        /// <returns></returns>
        static bool LoadData(TextAsset instance, string altPath = null)
        {
            TableView viewTable;
            if (altPath == null)
            {
                viewTable = new TableView(instance, altVoClass);
                // 获取到数据后弹出窗体或刷新
                if (viewTable.rawData == null) return false;
            }
            else
            {
                // 直接VO名称加载不支持自定义模板。
                viewTable = new TableView(null, altPath);
            }

            var window = GetWindow();
            window.tableView = viewTable;
            window.ResetToDefault();// 报错不刷新
            return viewTable.voName != null;
        }

        /// <summary>
        /// 处理关联表数据
        /// </summary>
        bool LoadJoinedData(string t1, string t2, string cond1)
        {
            JoinedTableView viewByteAsset = new JoinedTableView(t1, t2, cond1);
            var window = GetWindow();
            window.tableView = viewByteAsset;// 显示的时候降为一张表
            window.isInitialized = false;// 报错不刷新
            return viewByteAsset.voName != null && viewByteAsset.voAltName != null;
        }

        // 加载数据
        void TryPrepareTableData()
        {
            if (!isInitialized)
            {
                // 检查是否已存在
                if (m_TreeViewState == null) m_TreeViewState = new TreeViewState();
                // bool hasHeader = m_MultiColumnHeaderState == null;

                // 注意：虽然列表非树状，但是用到了树形表格API的一些功能与样式，所以按照它的规范初始化数据
                MultiColumnTreeView.tableColumnHeaders = InitTableHeader(InVisibleCols);
                var headerState = MultiColumnTreeView.CreateDefaultMultiColumnHeaderState(TableViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                {
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                }
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new CustomMultiColumnHeader(headerState);
                // if (hasHeader) multiColumnHeader.ResizeToFit();

                m_TreeView = new MultiColumnTreeView(m_TreeViewState, multiColumnHeader, new TreeModel<RowElement>(GetData()));

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed -= m_TreeView.SetFocusAndEnsureSelectedItem;
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                isInitialized = true;
            }
        }

        // 功能描述
        void DrawDescription(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"说明：[1]{VEStr.MSG_INTRO_0}[2]{VEStr.MSG_INTRO_1}");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("对比", GUI_BTN_STYLE))
            {
                VOCompareWindow.GetWindow();
            }
            if (GUILayout.Button("刷新", GUI_BTN_STYLE))
            {
                isInitialized = false;
                string lastInsName = tableView?.voName;
                tableView = null;
                VOMapper.Refresh();

                // 维持上次刷新资源的格式
                if (lastInstance == null)
                {
                    LoadData(null, lastInsName);
                }
                else
                {
                    LoadData(lastInstance);
                }

                // 重新展开
                // m_TreeView.ExpandAll();
            }
            DrawTooltip(VEStr.MSG_TIP_4);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // 顶部搜索
        void DrawSearchBar(Rect rect)
        {
            TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
            GUI.Label(rect, new GUIContent(string.Empty, VEStr.MSG_TIP_0));
        }

        // 主表格
        void DrawTreeView(Rect rect)
        {
            // 使用UnityEditor.IMGUI的TreeView绘制
            m_TreeView.OnGUI(rect);
        }

        // 底部工具栏
        void DrawBottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            // 准备需要显示的表头数据
            var myColumnHeader = (CustomMultiColumnHeader)TreeView.multiColumnHeader;
            myColumnHeader.OnHeaderChanged -= OnTableHeaderOnOff;
            myColumnHeader.OnHeaderChanged += OnTableHeaderOnOff;

            // 可以通过contentLayout.rect获得窗体的宽高等属性。
            using (var contentLayout = new EditorGUILayout.HorizontalScope())
            {
                if (!isJoinMode)
                {
                    // ---------------- 单表模式 ----------------
                    if (GUILayout.Button(isLoadByName ? "打开" : "加载", GUI_BTN_STYLE) || (isLoadByName && Event.current.keyCode == KeyCode.Return))
                    {
                        isLoadByName = !isLoadByName;
                        if (!isLoadByName)
                        {
                            voSearchName = VOMapper.Search(voSearchName);
                            //// var _lastInstance = VOMapper.ToTextAsset(voSearchName, DependencyPath);

                            if (!string.IsNullOrEmpty(voSearchName))
                            {
                                if (LoadData(lastInstance, voSearchName))
                                {
                                    // EditorUtility.FocusProjectWindow();
                                    myColumnHeader.ResetColumns(true);
                                    lastInstance = null;
                                }
                                else
                                {
                                    ShowNotification(new GUIContent(VEStr.MSG_OPEN_FAIL));
                                }
                            }
                        }
                    }
                    DrawTooltip(VEStr.MSG_TIP_3);

                    GUILayout.FlexibleSpace();

                    // 正下方显示文件链接或模板名称
                    if (isLoadByName)
                    {
                        voSearchName = EditorGUILayout.TextField(voSearchName, GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));
                        GUILayout.Space(10);
                    }
                    else
                    {
                        voSearchName = string.Empty;
                        // 当前加载资源位置
                        if (tableView == null)
                        {
                            GUILayout.Label(string.Empty, GUILayout.MinWidth(0));
                        }
                        else if (tableView.rawData == null && !string.IsNullOrEmpty(tableView.voName))
                        {
                            GUILayout.Label($"- 分片集: {tableView.voName} -",
                                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight }, GUILayout.MinWidth(0));
                        }
                        else if (
                            GUILayout.Button(
                                tableView != null ? AssetDatabase.GetAssetPath(tableView.rawData) : string.Empty,
                                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight }, GUILayout.MinWidth(0))
                        )
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(tableView.rawData)));
                        };
                        DrawTooltip(VEStr.MSG_TIP_5);
                    }

                    GUILayout.FlexibleSpace();

                    isAltVoLoader = GUILayout.Toggle(isAltVoLoader, $"替换视图{(isAltVoLoader ? " - " : "")}");
                    DrawTooltip(VEStr.MSG_TIP_1);

                    altVoClass = isAltVoLoader
                        ? EditorGUILayout.TextField(altVoClass, GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16))
                        : null;
                }
                else
                {
                    // ---------------- 关联模式 ----------------
                    var DEF_SQL_STYLE = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

                    // 模式切换
                    if (GUILayout.Button("查询", GUI_BTN_STYLE) || (isJoinMode && Event.current.keyCode == KeyCode.Return))
                    {
                        // 提交
                        searchTokens[0] = VOMapper.Search(searchTokens[0]);
                        searchTokens[1] = VOMapper.Search(searchTokens[1]);

                        if (!string.IsNullOrEmpty(searchTokens[0]) || !string.IsNullOrEmpty(searchTokens[1]))
                        {
                            Debug.Log($"[Editor]: 关联表数据 -> {searchTokens[0]}, {searchTokens[1]}");
                            if (!LoadJoinedData(searchTokens[0], searchTokens[1], searchTokens[2]))
                            {
                                ShowNotification(new GUIContent(VEStr.MSG_OPEN_FAIL));
                            }
                        }
                    }
                    DrawTooltip("select t1.*, t2.*");

                    GUILayout.Label("从表1:", DEF_SQL_STYLE, GUILayout.MinWidth(35));
                    DrawTooltip("from t1");

                    searchTokens[0] = EditorGUILayout.TextField(searchTokens[0], GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));

                    GUILayout.Label("关联表2:", DEF_SQL_STYLE, GUILayout.MinWidth(50));
                    DrawTooltip("left join t2");

                    searchTokens[1] = EditorGUILayout.TextField(searchTokens[1], GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));

                    GUILayout.Label("当表1.", DEF_SQL_STYLE, GUILayout.MinWidth(35));
                    DrawTooltip("on t1.");

                    searchTokens[2] = EditorGUILayout.TextField(searchTokens[2], GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));
                    DrawTooltip(VEStr.UI_SQL_FLD);

                    GUILayout.Label("=表2.主键", DEF_SQL_STYLE, GUILayout.MinWidth(55));
                    DrawTooltip("= t2.[PRIMARY KEY]");

                    GUILayout.FlexibleSpace();
                }

                // 模式切换
                if (GUILayout.Button(isJoinMode ? "返回" : "关联", GUI_BTN_STYLE))
                {
                    isJoinMode = !isJoinMode;
                    if (isJoinMode) searchTokens[0] = voSearchName;
                }
                DrawTooltip(VEStr.MSG_TIP_6);

                // ---------------- 通用操作功能 ----------------
                // TODO: 性能优化！！(这个操作换成，表头本来显示全部，之后切换部分)
                myColumnHeader.extraDescs = tableView != null
                    ? tableView.TableHeaders.Select(col => isJoinMode ? $"{col.view}.{col.name}" : col.name).ToList()
                    : default;

                if (GUILayout.Button("重置", GUI_BTN_STYLE))
                {
                    ResetToDefault();
                    myColumnHeader.SetSortingColumns(new[] { 0 }, new[] { true });

                    if (m_MultiColumnHeaderState != null && m_MultiColumnHeaderState.columns != null)
                        m_MultiColumnHeaderState.visibleColumns = Enumerable.Range(0, m_MultiColumnHeaderState.columns.Count()).ToArray();

                    // myColumnHeader.Repaint();
                    myColumnHeader.Mode = CustomMultiColumnHeader.ColumnDisplayMode.DefaultHeader;
                }

                GUILayout.Label("表头:");

                if (GUILayout.Button("默认", GUI_BTN_STYLE))
                {
                    myColumnHeader.Mode = CustomMultiColumnHeader.ColumnDisplayMode.DefaultHeader;
                }
                if (GUILayout.Button("详细", GUI_BTN_STYLE))
                {
                    myColumnHeader.Mode = CustomMultiColumnHeader.ColumnDisplayMode.LargeHeader;
                }
                if (GUILayout.Button("最简", GUI_BTN_STYLE))
                {
                    myColumnHeader.Mode = CustomMultiColumnHeader.ColumnDisplayMode.MinimumHeaderWithoutSorting;
                }

                GUILayout.Space(10);

                if (Application.isPlaying)
                {
                    if (GUILayout.Button($"共{TreeView.GetRows()?.Count}项 <-> {(TreeView.isEditMode ? "确认" : "编辑")}", GUI_BTN_STYLE))
                    {
                        // 尝试保存
                        if (TreeView.isEditMode && !TreeView.SaveChange(tableView.voName))
                        {
                            ShowNotification(new GUIContent(VEStr.MSG_SAVE_FAIL));
                        }
                        TreeView.isEditMode = !TreeView.isEditMode;
                    }
                }
                else
                {
                    GUILayout.Label($"共{TreeView.GetRows()?.Count}项 <-> 查看", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight });
                    DrawTooltip(VEStr.MSG_TIP_2);
                    TreeView.isEditMode = false;
                }

            }

            GUILayout.EndArea();
        }

        // 表头显示状态变化
        void OnTableHeaderOnOff(int[] hideCols)
        {
            InVisibleCols = hideCols;
        }

        // 重置设置
        void ResetToDefault()
        {
            isJoinMode = false;
            searchTokens[0] = searchTokens[1] = searchTokens[2] = string.Empty;
            TreeView.searchString = string.Empty;
            InVisibleCols = null;
        }

        /// <summary>
        /// 绘制提示信息
        /// </summary>
        /// <param name="tip"></param>
        void DrawTooltip(string tip)
        {
            if (!string.IsNullOrEmpty(tip)) GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, tip));
        }

        /// <summary>
        /// 数据表格头
        /// </summary>
        List<ColumnField> InitTableHeader(int[] hideCols = default)
        {
            if (tableView == null || tableView.TableHeaders == null || tableView.TableHeaders.Count == 0)
            {
                tableView = null;// 清除旧数据
                lastInstance = null;
                return new List<ColumnField> {
                    new ColumnField(0, FieldType.UNDEFINED, MultiColumnTreeView.SlashFormat(VEStr.UI_HEADER_BLANK, DependencyPath))
                };
            }
            else
            {
                return tableView.TableHeaders
                    .Select(col =>
                    {
                        col.isVisible = hideCols == null || !hideCols.Contains(col.index);
                        return col;
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// 获得表数据
        /// </summary>
        IList<RowElement> GetData()
        {
            return tableView == null
                ? new List<RowElement> {
                    ViewParser.CreateTableRoot()
                }
                : tableView.TableRows;
        }

        /// <summary>
        /// 切换到其它资源的时候（点选或拖拽）
        /// </summary>
        void OnSelectionChange()
        {
            if (!isInitialized) return;
            var viewByteAsset = Selection.activeObject as TextAsset;
            if (viewByteAsset == null || viewByteAsset.hideFlags != HideFlags.NotEditable) return;
            // 局部重新加载(初始化完毕后)
            lastInstance = viewByteAsset;
            if (tableView != null)
            {
                tableView = new TableView(lastInstance, altVoClass);
                m_TreeView.TreeModel.SetData(GetData());
                m_TreeView.Reload();
            }
        }
    }

    /// <summary>
    /// 数据表检查工具 - 定制化表头
    /// </summary>
    internal class CustomMultiColumnHeader : MultiColumnHeader
    {
        // 显示模式
        public List<string> extraDescs;
        public Action<int[]> OnHeaderChanged;

        // int currentHeader;
        ColumnDisplayMode m_Mode;

        public enum ColumnDisplayMode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        // 表头显示模式
        public ColumnDisplayMode Mode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case ColumnDisplayMode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case ColumnDisplayMode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    default:
                    case ColumnDisplayMode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        public CustomMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            Mode = ColumnDisplayMode.DefaultHeader;
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
            bool switchCondition = state.visibleColumns.Count() < 3;
            menu.AddItem(new GUIContent(
                switchCondition ? VEStr.UI_COL_SHOWALL : VEStr.UI_COL_HIDEALL), false, ResetColumns, switchCondition);
            if (!switchCondition)
            {
                menu.AddItem(new GUIContent(VEStr.UI_COL_GUESS), false, GuessNameColumns);
            }
            base.AddColumnHeaderContextMenuItems(menu);
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // 默认Table表头
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // 定制化显示内容
            if (Mode == ColumnDisplayMode.LargeHeader)
            {
                // if (columnIndex > 0)
                headerRect.xMax -= 3f;
                var oldAlignment = EditorStyles.largeLabel.alignment;
                EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                bool hasDesc = extraDescs != null && columnIndex < extraDescs.Count && extraDescs[columnIndex] != null;
                // 确信人类可以理解的开始数字不是0（比如Lua，emmm...）
                GUI.Label(
                    headerRect,
                    $"{columnIndex + 1}{(hasDesc ? $".{extraDescs[columnIndex]}" : string.Empty)}",
                    EditorStyles.largeLabel
                );
                EditorStyles.largeLabel.alignment = oldAlignment;
            }
        }

        protected override void ColumnHeaderClicked(MultiColumnHeaderState.Column column, int columnIndex)
        {
            // currentHeader = columnIndex;
            base.ColumnHeaderClicked(column, columnIndex);
        }

        protected override void OnVisibleColumnsChanged()
        {
            // 筛选出不显示的列
            OnHeaderChanged?.Invoke(
                Enumerable.Range(0, state.columns.Count()).Where(i => state.visibleColumns.All(j => j != i)).ToArray());
            base.OnVisibleColumnsChanged();
        }

        // 表头全开全关切换
        public void ResetColumns(object condition)
        {
            state.visibleColumns = (bool)condition
                ? Enumerable.Range(0, state.columns.Count()).ToArray()
                : sortedColumnIndex > 0 ? new int[] { 0, sortedColumnIndex } : new int[] { 0 };
            OnVisibleColumnsChanged();
        }

        // 显示带名称的列
        void GuessNameColumns()
        {
            state.visibleColumns = state.visibleColumns
                    .Select((col, i) => new KeyValuePair<int, string>(col, GetColumn(i).headerContent.text))
                    .Where(kv =>
                        (kv.Key == 0)// 人工智能？人肉智能。
                        || kv.Value.EndsWith("desc", StringComparison.InvariantCultureIgnoreCase)
                        || kv.Value.IndexOf("description", StringComparison.InvariantCultureIgnoreCase) >= 0
                        || kv.Value.EndsWith("intro", StringComparison.InvariantCultureIgnoreCase)
                        || kv.Value.IndexOf("introduction", StringComparison.InvariantCultureIgnoreCase) >= 0
                        || kv.Value.EndsWith("name", StringComparison.InvariantCultureIgnoreCase)
                        || kv.Value.EndsWith("msg", StringComparison.InvariantCultureIgnoreCase)
                        || kv.Value.EndsWith("tip", StringComparison.InvariantCultureIgnoreCase)
                    )
                    .Select(kv => kv.Key)
                    .ToArray();
            OnVisibleColumnsChanged();
        }
    }
}