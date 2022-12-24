using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Aby.TableData;

namespace Com.Zorro.KomoriLife.Editor
{
    /// <summary>
    /// VO工具 - 映射关系管理
    /// </summary>
    internal static class VOMapper
    {
        #region 系统变量

        // VO根目录路径
        public const string VO_ROOT = "Assets/Script/VO";

        /// <summary>
        /// 加载位置
        /// </summary>
        public const string RES_DIR = "Assets/Res/Excel";

        /// <summary>
        /// VO注册位置
        /// </summary>
        public const string DEF_PATH_ALT = "Assets/Res/Excel/vo_list.txt";

        /// <summary>
        /// 表名VO映射关系位置
        /// </summary>
        public const string DEF_PATH = "Assets/Editor/ViewTools/vo_map.txt";

        /// <summary>
        /// 多语言字段过滤
        /// </summary>
        public const string FILTER_PATH = "Assets/Editor/ViewTools/vo_col_i18n.txt";

        /// <summary>
        /// 日志导出位置
        /// </summary>
        public const string LOG_PATH = "Temp/DiffHistory.txt";

        #endregion

        /// <summary>
        /// 运行时加载的VO映射
        /// </summary>
        /// <typeparam name="string">VO类名</typeparam>
        /// <typeparam name="string">描述</typeparam>
        /// <returns></returns>
        readonly static Dictionary<string, string> voMap = new Dictionary<string, string>();

        /// <summary>
        /// 手动配置的VO列表
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        readonly static List<string> voList = new List<string>();

        /// <summary>
        /// 错误配置记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        readonly static HashSet<string> errRecords = new HashSet<string>();

        /// <summary>
        /// 初始化映射
        /// </summary>
        public static void Refresh()
        {
            voMap.Clear();
            voList.Clear();

            RecordByLine(DEF_PATH, line => AddRecordToMap(line));
            RecordByLine(DEF_PATH_ALT, line => AddRecordToList(line));

            if (errRecords.Count > 0)
                Debug.LogError($"[Editor]: 行[{string.Join(",", errRecords)}]数据存在重复，请修复后重载！");
        }

        /// <summary>
        /// 根据名称搜索bytes
        /// </summary>
        /// <param name="voSearchName">搜索名称</param>
        /// <param name="parentPath">VO的根路径位置</param>
        /// <returns>存在返回bytes的Instance，不存在返回null</returns>
        public static string Search(string voSearchName)
        {
            if (string.IsNullOrEmpty(voSearchName)) return null;

            // 最先精确匹配
            string result = VOUtilities.GetResourceVOName($"{VO_ROOT}/{voSearchName}.cs");

            // 先过滤关联
            if (string.IsNullOrEmpty(result))
            {
                result = voMap.FirstOrDefault(item =>
                    item.Key.IndexOf(voSearchName, StringComparison.OrdinalIgnoreCase) >= 0
                    || item.Value.IndexOf(voSearchName, StringComparison.OrdinalIgnoreCase) >= 0
                ).Value;
            }

            // 再过滤索引
            if (string.IsNullOrEmpty(result))
            {
                result = voList.Where(item => item.IndexOf(voSearchName, StringComparison.OrdinalIgnoreCase) >= 0).FirstOrDefault();
                // 剪切结尾
                if (!string.IsNullOrEmpty(result)
                    && result.LastIndexOf("vo", StringComparison.OrdinalIgnoreCase) is int endIndex
                    && endIndex >= 0
                ) result = result.Substring(0, endIndex);
            }

            if (string.IsNullOrEmpty(result)) return null;
            return result;
        }

        /// <summary>
        /// 解析配置加入映射字典
        /// </summary>
        /// <param name="line">读取的行</param>
        /// <returns>是否添加成功</returns>
        static bool AddRecordToMap(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            string[] kv = line.Split(':');
            try
            {
                if (kv.Length > 1) voMap.Add(kv[0].Trim(), kv[1].Trim());
            }
            catch (ArgumentException ex)
            {
                errRecords.Add(line);
                Debug.LogError(ex);
            }
            return true;
        }

        /// <summary>
        /// 解析配置加入搜索列表
        /// </summary>
        /// <param name="line">读取的行</param>
        /// <returns>是否添加成功</returns>
        static bool AddRecordToList(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            voList.Add(line);
            return true;
        }

        /// <summary>
        /// 按行读取配置文件内容
        /// <para>规则: 去掉注释行，空行。</para>
        /// </summary>
        /// <param name="confPath">配置路径</param>
        /// <param name="parseRecordAction">每一行对记录的处理</param>
        /// <returns>是否解析成功</returns>
        public static bool RecordByLine(string confPath, Action<string> parseRecordAction)
        {
            var configs = AssetDatabase.LoadAssetAtPath<TextAsset>(confPath);

            if (configs.bytes.Length < 0) return false;

            using StringReader reader = new StringReader(configs.text);
            string line;

            // 按行读取配置
            while ((line = reader.ReadLine()) != null)
            {
                // 非空, 非空白, 非注释
                if (string.IsNullOrEmpty(line)
                    || string.IsNullOrWhiteSpace(line)
                    || line.Trim().StartsWith("#")
                ) continue;
                // 执行每行操作
                parseRecordAction?.Invoke(line);
            }

            return true;
        }
    }

    /// <summary>
    /// VO工具的工具类
    /// </summary>
    internal static class VOUtilities
    {
        /// <summary>
        /// 默认角色ID
        /// <para>详见: Assets\Script\Behaviours\Littlefox\LfBDInitVos.cs</para>
        /// </summary>
        const string defaultCharacterId = "16010000";

        /// <summary>
        /// 所有VO中用于加载数据的方法名
        /// </summary>
        const string METHOD_LOAD_DATA = "_LoadData";
        const string METHOD_LOAD_DATA_PRIVATE = "Internal_LoadData";

        /// <summary>
        /// 尝试打开Bytes
        /// </summary>
        /// <param name="voSearchName"></param>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        public static TextAsset ToTextAsset(string voSearchName, string parentPath)
        {
            // 如果找到再查找是否有指定的Bytes
            string bytesPath = $"{parentPath}{voSearchName.ToLowerInvariant()}.bytes";
            if (!File.Exists(bytesPath)) return null;

            // 通过路径返回当前VO的ID便于与点击打开使用相同方法加载。
            return AssetDatabase.LoadAssetAtPath<TextAsset>(bytesPath);
        }

        /// <summary>
        /// 获得指定资源名称。
        /// </summary>
        /// <param name="bytesPath"></param>
        /// <returns></returns>
        public static string GetResourceVOName(string bytesPath)
        {
            int instanceID = AssetDatabase.LoadAssetAtPath<TextAsset>(bytesPath) is TextAsset ast ? ast.GetInstanceID() : -1;
            string resName = EditorUtility.InstanceIDToObject(instanceID) is TextAsset textData ? textData.name : null;
            return resName;
        }

        /// <summary>
        /// 在Editor模式下加载VO
        /// </summary>
        /// <param name="className">类名，不填的话按泛型类名初始化</param>
        /// <param name="bytes">额外的Bytes文件，兼容旧系统</param>
        /// <typeparam name="T">任意VO</typeparam>
        /// <returns>当前VO的实例化对象</returns>
        public static T InstantiateInEditor<T>(string className = "", byte[] bytes = default)
        {
            // 预加载数据集(内有缓存，不会重复加载。)
            AbySchemasShardsLangsDatasetHelper.GetTable();

            // 注意Editor的空间与程序集不在一起，因此需要声明。
            Type viewClass = string.IsNullOrEmpty(className) ? typeof(T) : Type.GetType($"{className}, Assembly-CSharp");
            var vo = Activator.CreateInstance(viewClass);

            try
            {
                AXTimerHelper.Mark("VO数据加载");
                // 用VO自带的加载数据方法获得数据。
                MethodInfo loadMethod = viewClass.GetMethod(METHOD_LOAD_DATA, BindingFlags.Static | BindingFlags.Public);
                // 兼容处理
                if (loadMethod == null) loadMethod = viewClass.GetMethod(METHOD_LOAD_DATA_PRIVATE, BindingFlags.Static | BindingFlags.NonPublic);
                var parameterInfos = loadMethod.GetParameters();
                // 直接初始化，此处几张特殊表特殊处理。
                if (parameterInfos.Length > 0 && parameterInfos[0].Name == "tableId")
                {
                    loadMethod.Invoke(vo, new object[] { defaultCharacterId });
                }
                else
                {
                    loadMethod.Invoke(vo, new object[] { bytes });
                }
                AXTimerHelper.Record("VO数据加载");
            }
            catch (Exception ex)
            {
                // 加载错误不要卡住表头
                Debug.LogError("[Editor]: 数据加载错误! " + ex);
            }

            return (T)vo;
        }
    }

}