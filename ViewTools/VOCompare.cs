using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// 表结构对比工具 - by HNC
    /// </summary>
    internal class VOCompareWindow : EditorWindow
    {
        #region 静态数据区
        const string GUI_BTN_STYLE = "miniButton";
        const string WINDOW_NAME = "对比配置表";
        const string MSG_INTRO_0 = "填写环境后添加表名，对比结果导出至:";
        const string MSG_TIP_0 = "请正确选择需要比较的环境！";
        const string MSG_TIP_1 = "请至少添加一项！";
        const string CONTENT_TITLE = "=-=-=-=-=-=-=-= 数据表差异项比较结果 =-=-=-=-=-=-=-=";
        #endregion

        [SerializeField] List<string> envs;
        [SerializeField] (string, string) compareEnvs; // 比较的路径
        [SerializeField] List<(string, string)> candidates; // 待比较队列

        List<string> records = new List<string>() { $"{CONTENT_TITLE}\n" }; // 记录行
        readonly Dictionary<string, List<string>> columnfilters = new Dictionary<string, List<string>>();
        Vector2 scrollPos;
        bool isExportStart;// 是否开始导出

        #region 元素UI布局
        Rect DescViewRect
        {
            get { return new Rect(20, 5, position.width - 40, 16); }
        }

        Rect TitleRect
        {
            get { return new Rect(20, 30, position.width - 40, 16); }
        }

        Rect ContentViewRect
        {
            get { return new Rect(20, 55, position.width - 40, position.height - 90); }
        }

        Rect ControlViewRect
        {
            get { return new Rect(20, position.height - 18, position.width - 40, 16); }
        }

        #endregion

        /// <summary>
        /// 打开编辑器窗口
        /// <para>- 图标参考: https://github.com/halak/unity-editor-icons</para>
        /// </summary>
        // [MenuItem("Tools/" + WINDOW_NAME)]
        public static VOCompareWindow GetWindow()
        {
            var window = GetWindow<VOCompareWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME, EditorGUIUtility.FindTexture("ToggleUVOverlay"));
            window.minSize = new Vector2(512, 256);
            window.Focus();
            window.Repaint();
            return window;
        }

        void OnEnable()
        {
            VOMapper.Refresh();
            columnfilters.Clear();
            VOMapper.RecordByLine(VOMapper.FILTER_PATH, line => AddRecordToDic(line));
            candidates = new List<(string, string)>();
            if (new DirectoryInfo(VOMapper.RES_DIR).GetDirectories() is var envPaths && envPaths != null)
            {
                envs = envPaths.Select(dirInfo => dirInfo.Name).ToList();
            }
        }

        void OnGUI()
        {
            DrawDescription(DescViewRect);
            DrawControlPanel(ControlViewRect);
            DrawTitles(TitleRect);
            DrawUILine(50);
            DrawContent(ContentViewRect);
        }

        #region UI操作

        // 随便画一条横线
        void DrawUILine(int height)
        {
            EditorGUI.DrawRect(new Rect(5, height, position.width - 10, 2), Color.gray);
        }

        // 功能描述
        /// <param name="rect"></param>
        void DrawDescription(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"说明：{MSG_INTRO_0} {VOMapper.LOG_PATH}");
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawTitles(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"数据表名称");
            GUILayout.Space(10);
            GUILayout.Label($"自定义加载");
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // 操作区
        void DrawControlPanel(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            GUILayout.Label("本地环境:");
            ShowFirstDropdownOptions();
            GUILayout.Space(10);
            GUILayout.Label("目标环境:");
            ShowSecondDropdownOptions();
            GUILayout.Space(10);
            GUILayout.FlexibleSpace();
            // 功能项
            if (GUILayout.Button("刷新", GUI_BTN_STYLE))
            {
                candidates.Clear();
                compareEnvs = (string.Empty, string.Empty);
                records = new List<string>() { $"{CONTENT_TITLE}\n" }; // 记录行
            }
            if (GUILayout.Button("+", GUI_BTN_STYLE))
            {
                candidates.Add((string.Empty, string.Empty));
            }
            if (GUILayout.Button("-", GUI_BTN_STYLE))
            {
                if (candidates.Count > 0) candidates.RemoveAt(candidates.Count - 1);
            }
            if (GUILayout.Button("导出", GUI_BTN_STYLE))
            {
                if (string.IsNullOrEmpty(compareEnvs.Item1) || string.IsNullOrEmpty(compareEnvs.Item2))
                {
                    ShowNotification(new GUIContent(MSG_TIP_0));
                    return;
                }
                if (candidates.Count == 0)
                {
                    ShowNotification(new GUIContent(MSG_TIP_1));
                    return;
                }
                // 循环遍历所有表，依次导出
                candidates.ForEach(voName => ExportDiff(voName.Item1, voName.Item2));
                isExportStart = true;
                Debug.Log($"[Editor]: 记录完毕！位置 = {VOMapper.LOG_PATH}");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // 内容
        void DrawContent(Rect rect)
        {
            GUILayout.BeginArea(rect);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            for (int i = 0; i < candidates.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var voName = EditorGUILayout.TextField(candidates[i].Item1, GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));
                var loaderName = EditorGUILayout.TextField(candidates[i].Item2, GUILayout.MaxWidth(int.MaxValue), GUILayout.Height(16));
                candidates[i] = (voName, loaderName);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            DrawExportProgress();
        }

        // 导出数据
        void DrawExportProgress()
        {
            if (isExportStart)
            {
                // for (float t = 0; t < records.Count; t++)
                // {
                //     EditorUtility.DisplayProgressBar($"导出{records.Count}条记录", $"写入{t}条记录...", t / records.Count);
                // }
                // EditorUtility.ClearProgressBar();
                if (File.Exists(VOMapper.LOG_PATH)) File.Delete(VOMapper.LOG_PATH);
                File.WriteAllLines(VOMapper.LOG_PATH, records);
            }
            isExportStart = false;
        }

        // 选项1
        void ShowFirstDropdownOptions()
        {
            if (GUILayout.Button(string.IsNullOrEmpty(compareEnvs.Item1) ? "选择环境" : compareEnvs.Item1, GUI_BTN_STYLE, GUILayout.Width(80)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var envDir in envs)
                    menu.AddItem(new GUIContent(envDir), compareEnvs.Item1 == envDir,
                        sel => { compareEnvs.Item1 = sel.ToString(); }, envDir);
                menu.ShowAsContext();
            }
        }

        // 选项2
        void ShowSecondDropdownOptions()
        {
            if (GUILayout.Button(string.IsNullOrEmpty(compareEnvs.Item2) ? "选择环境" : compareEnvs.Item2, GUI_BTN_STYLE, GUILayout.Width(80)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var envDir in envs)
                    menu.AddItem(new GUIContent(envDir), compareEnvs.Item2 == envDir,
                        sel => { compareEnvs.Item2 = sel.ToString(); }, envDir);
                menu.ShowAsContext();
            }
        }

        #endregion

        // 获得语言目录
        string GetRawPath(string lang)
        {
            return $"Assets/Res/Excel/{lang + "/"}";
        }

        // 记录字典过滤数据
        bool AddRecordToDic(string line)
        {
            // 非空, 非空白, 非注释
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#")) return false;
            string[] kv = line.Split('|');
            try
            {
                if (kv.Length != 2 || string.IsNullOrEmpty(kv[0])) return false;
                var key = kv[0].Trim().ToLowerInvariant();
                var val = kv[1].Split(',').ToList();
                if (!columnfilters.ContainsKey(key)) columnfilters.Add(
                    key,
                    string.IsNullOrEmpty(kv[1]) ? null : val.Select(vo => string.IsNullOrEmpty(vo) ? null : vo.Trim().ToLowerInvariant()).ToList()
                );
            }
            catch (ArgumentException ex)
            {
                Debug.LogError(ex);
            }
            return true;
        }

        /// <summary>
        /// 导出内容
        /// </summary>
        /// <param name="voName">表名称</param>
        /// <param name="altLoader">加载类（不填 = 默认）</param>
        void ExportDiff(string voName, string altLoader)
        {
            if (string.IsNullOrEmpty(voName)) return;
            TextAsset instance;
            HashSet<int> errIds = new HashSet<int>();
            TableView data1 = null, data2 = null;

            // 初始化
            records.Add($"当前表名: {voName} <--\n");
            string candidateName = VOMapper.Search(voName);

            // 找到原始环境VO
            instance = VOUtilities.ToTextAsset(candidateName, GetRawPath(compareEnvs.Item1));
            if (instance != null)
            {
                // 初始化原始环境VO数据
                data1 = new TableView(instance, altLoader);
            }

            // 找到目标环境VO
            instance = VOUtilities.ToTextAsset(candidateName, GetRawPath(compareEnvs.Item2));
            if (instance != null)
            {
                // 初始化目标环境VO数据
                data2 = new TableView(instance, altLoader);
            }

            if (data1 == null || data2 == null) return;

            // 对数据进行比较，写入记录
            var filterList = columnfilters.ContainsKey(voName) ? columnfilters[voName] : null;
            if (filterList != null && filterList.Count > 0) filterList = filterList.Select(colname => colname.ToLowerInvariant()).ToList();

            // 跳过根元素
            int dataCounter1 = 1;
            for (; dataCounter1 < data1.TableRows.Count; dataCounter1++)
            {
                var currentRow = data1.TableRows[dataCounter1];
                if (currentRow == null || currentRow.Content == null) continue;
                // 用1的ID去2里找, 这一步会对齐列。
                int id = int.Parse(currentRow.Content[0].rawContent);
                var targetRow = ViewParser.TryGetVOElement(id, data2.voName, data2.TableHeaders);
                if (targetRow == null) errIds.Add(id);
                // 比较1与2的差异项
                bool isDirty = false;
                for (int i = 0; i < currentRow.Content.Count; i++)
                {
                    // 跳过排除的列
                    if (filterList != null && filterList.Count > 0
                        && filterList.Contains(data1.TableHeaders[i].header.ToLowerInvariant())) continue;

                    if (currentRow.Content[i].rawContent != targetRow.Content[i].rawContent)
                    {
                        if (!isDirty)
                        {
                            records.Add($"发现差异项 ->");
                            isDirty = true;
                        }
                        records.Add($"[{data1.TableHeaders[i].header}]: {currentRow.Content[i].rawContent} -> {targetRow.Content[i].rawContent}");
                    }
                }
                if (isDirty) records.Add($"\n");
            }

            // 分割
            records.Add($"总计: 共比较{dataCounter1 - 1}项 <--\n");
            records.Add($"缺失数据: {string.Join(",", errIds.ToArray())} <--\n");
        }

    }
}