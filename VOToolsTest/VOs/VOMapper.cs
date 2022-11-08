using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameNeon
{
    /// <summary>
    /// VO 映射关系管理，初始化数据和重加载用
    /// </summary>
    public class VOMapper
    {
        #region 系统变量

        // VO根目录路径
        public const string VO_ROOT = "Assets/Neon/Scripts/VOs";
        
        /// 加载位置
        public const string RES_DIR = "Assets/Neon/Datas/";

        /// VO注册位置
        public const string DEF_PATH_ALT = "Assets/Neon/Datas/vo_list.txt";

        /// 表名VO映射关系位置
        public const string DEF_PATH = "Assets/Neon/Datas/vo_map.txt";
        
        /// 日志导出位置
        public const string LOG_PATH = "Temp/DiffHistory.txt";
        
        #endregion
        
        
        /// <summary>
        /// 运行时加载的VO映射 提供给DataManger
        /// </summary>
        /// <typeparam name="string">VO类名</typeparam>
        /// <typeparam name="string">描述</typeparam>
        /// <returns></returns>
        static readonly Dictionary<string, string> voMap = new Dictionary<string, string>();

        /// <summary>
        /// 手动配置的VO列表
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        static readonly List<string> voList = new List<string>();

        /// <summary>
        /// 错误配置记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        static readonly HashSet<string> errRecords = new HashSet<string>();
        
        /// <summary>
        /// 初始化映射
        /// </summary>
        public static void Refresh()
        {
            voMap.Clear();
            voList.Clear();
            
            // TODO 读取配置内容并加载 目前想的时从VOUntil中
            
            if (errRecords.Count > 0)
                Debug.LogError($"[Editor]: 行[{string.Join(",", errRecords)}]数据存在重复，请修复后重载！");
        }
        
        /// <summary>
        /// 根据名称搜索Jsondata
        /// </summary>
        /// <param name="voSearchName">搜索名称</param>
        /// <param name="parentPath">VO的根路径位置</param>
        /// <returns>存在返回bytes的Instance，不存在返回null</returns>
        public static string Search(string voSearchName)
        {
            if (string.IsNullOrEmpty(voSearchName)) return null;

            // 最先精确匹配
            string result = VOUtil.GetResourceVOName($"{VO_ROOT}/{voSearchName}VO.cs");

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
        
       
    }
}
