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
    public class VOUtil
    {
        private JsonData data;
        private string className;

        /// <summary>
        /// 获取该表的所有键
        /// </summary>
        private string[] m_keys = { };

        /// <summary>
        /// 键值的类型
        /// </summary>
        private string[] m_types = { };

        // 补空字符串 4tab
        string tchar = "\t";
        string t2char = "\t\t";

        // VO根目录路径
        private const string VO_ROOT = "Assets/Neon/Scripts/VOs/";


        public VOUtil(JsonData data, string path)
        {
            this.data = data;
            this.className = StringEx.ToUpperFirstChar(path);
            try
            {
                ExportFile();
            }
            catch (Exception e)
            {
                throw new Exception($"{className}VO 生成异常，检查组装代码！");
            }
        }
        
        private void ExportFile()
        {
            string filePath = VO_ROOT + className + "VO.cs";
            // 如果文件存在删除创建新文件 （先这么做）
            if (File.Exists(filePath)) File.Delete(filePath);
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            // 写入命名空间
            WriteNamespace(sw);
            // 函数签名
            WriteVOSign(sw);
            sw.WriteLine("{");

            // 构造数据结构，这里只需要任意 JsonData 即可
            if (data.Count <= 0) throw new Exception($"{className}_JsonData没有数据！");
            ParasJsonData(data[0]); // 先解析
            WriteInnerClass(sw);
            sw.WriteLine();
            
            // 新增数值
            WriteDictionary(sw);
            // Func
            WriteGetVOFunc(sw);

            sw.WriteLine("}");
            sw.WriteLine();
            sw.Flush();
            sw.Close();
        }
        
        /// <summary>
        /// 解析源JsonData
        /// </summary>
        private void ParasJsonData(JsonData data)
        {
            // 拿到键
            m_keys = data.Keys.ToArray();

            // 切记给m_types开辟空间
            m_types = new string[m_keys.Length];

            // 拿到值
            string[] tmpValue = new string[m_keys.Length];
            for (int i = 0; i < tmpValue.Length; i++)
            {
                tmpValue[i] = data[m_keys[i]].ToString();
                // 顺便把m_keys全部首字母小写
                m_keys[i] = StringEx.ToLowerFirstChar(m_keys[i]);
            }

            try
            {
                // 根据临时值值判断类型去赋值type函数
                for (int i = 0; i < tmpValue.Length; i++)
                {
                    var v = tmpValue[i]; // 避免闭包
                    if (v == null) throw new Exception($"{m_keys[i]}参数非法！数据源不能为空");
                    if (StringEx.IsFloat(v))
                    {
                        if (StringEx.IsInt(v) && StringEx.IsNumber(v))
                        {
                            if (m_keys[i].ToUpper() == "ID") m_types[i] = "string";
                            else m_types[i] = "int";
                        }
                        else m_types[i] = "float";
                    }
                    else if (StringEx.IsString(v)) m_types[i] = "string";
                    else throw new Exception($"{m_keys[i]}参数非法！请检查数据源");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"JsonData_{className}解析异常！检查客户端表!");
            }
        }

        private void WriteNamespace(StreamWriter sw)
        {
            sw.WriteLine("// 该类为自动生成的VO类，根据需求增加变量或方法");
            sw.WriteLine();
            sw.WriteLine("using System;");
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using System.Collections;");
            sw.WriteLine("using System.Collections.Generic;");
            sw.WriteLine();
        }

        /// <summary>
        /// 写入类名
        /// </summary>
        /// <param name="sw"></param>
        private void WriteVOSign(StreamWriter sw)
        {
            sw.WriteLine($"public class {className}VO");
        }

        /// <summary>
        /// 构造内部类数据结构
        /// </summary>
        /// <param name="sw"></param>
        private void WriteInnerClass(StreamWriter sw)
        {
            sw.WriteLine($"{tchar}public class {className}");
            sw.WriteLine(tchar + "{");

            string paras = "";
            for (int i = 0; i < m_keys.Length; i++)
            {
                sw.WriteLine(string.Format($"{t2char}private {m_types[i]} {m_keys[i]};"));
                if (i != m_keys.Length - 1)
                    paras += $"{m_types[i]} {m_keys[i]},";
                else
                    paras += $"{m_types[i]} {m_keys[i]}";
            }

            sw.WriteLine();

            sw.WriteLine($"{t2char}// 构造时必须赋值");
            // string paras = "int id, float config_id, string name";
            sw.WriteLine($"{t2char}public {className}({paras})");
            sw.WriteLine(t2char + "{");

            for (int i = 0; i < m_keys.Length; i++)
            {
                sw.WriteLine($"{tchar + t2char}this.{m_keys[i]} = {m_keys[i]};");
            }

            sw.WriteLine(t2char + "}");
            sw.WriteLine();

            for (int i = 0; i < m_keys.Length; i++)
            {
                // get首字母大写
                var tag = StringEx.ToUpperFirstChar(m_keys[i]);
                sw.WriteLine($"{t2char}public {m_types[i]} {tag} => {m_keys[i]};");
            }

            sw.WriteLine(tchar + "}");
        }
        
        private void WriteDictionary(StreamWriter sw)
        {
            sw.WriteLine(
                $"{tchar}public Dictionary<string ,{className}> dict = new Dictionary<string ,{className}>();");
            sw.WriteLine();
            sw.WriteLine($"{tchar}public {className}VO()");
            sw.WriteLine(tchar + "{");
            sw.WriteLine(tchar + "}");
            sw.WriteLine();
        }

        private void WriteGetVOFunc(StreamWriter sw)
        {
            sw.WriteLine($"{tchar}public Dictionary<string,{className}> GetVOList()");
            sw.WriteLine(tchar + "{");
            sw.WriteLine($"{t2char}return dict;");
            sw.WriteLine(tchar + "}");
            sw.WriteLine();
            
            sw.WriteLine($"{tchar}public {className} GetVO(string key)");
            sw.WriteLine(tchar + "{");
            sw.WriteLine($"{t2char}if (!dict.ContainsKey(key))");
            sw.WriteLine($"{tchar+t2char} throw new Exception(\"{className}VO没有Id为\"+ key + \"的记录！\");");
            sw.WriteLine($"{t2char}return dict[key];");
            sw.WriteLine(tchar + "}");
            sw.WriteLine();
            
            sw.WriteLine($"{tchar}public bool HasVO(string key)");
            sw.WriteLine(tchar + "{");
            sw.WriteLine($"{t2char} return dict.ContainsKey(key) != null;");
            sw.WriteLine(tchar + "}");
            sw.WriteLine();
            sw.WriteLine(tchar + "// 自定义...");
        }

        /// <summary>
        /// 获得指定资源名称。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetResourceVOName(string filePath)
        {
            int instanceID = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath) is TextAsset ast ? ast.GetInstanceID() : -1;
            string resName = EditorUtility.InstanceIDToObject(instanceID) is TextAsset textData ? textData.name : null;
            return resName;
        }

    }
}