using System;
using System.Collections.Generic;
using System.Linq;
using GameNeon.Managers;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameNeon
{
    public class TestVo : MonoBehaviour
    {
        public static string TEXT_PATH = "Test/characterConfigTest";

        private void Start()
        {
            Test();
        }


        [MenuItem("Tools/文若测试生成VO")]
        private static void GenVOTest()
        {
            // 点击ok返回true，点击cancel返回false；
            bool res = EditorUtility.DisplayDialog("VO测试工具",
                "确认后生成VO。", "好的", "哒咩");
            if (res) GenVO();
        }

        private static void GenVO()
        {
            string path = TEXT_PATH + ".json";
            JsonData data = DataManager.Instance.GetData(path);
            string className = TEXT_PATH.Split('/')[1];
            VOUtil vou = new VOUtil(data, className);
            // vou.ExportFile();
        }

        private static void Test()
        {
            string[] keys = { };

            string path = TEXT_PATH + ".json";
            JsonData data = DataManager.Instance.GetData(path);
            ParsJsonData(data[0]);
        }

        private static void ParsJsonData(JsonData data)
        {
            // 拿到键和临时值（用于类型判断）
            string[] m_keys = data.Keys.ToArray();
            string[] m_types = new string[m_keys.Length];
            string[] tmpValue = new string[m_keys.Length];
            
            for (int i = 0; i < tmpValue.Length; i++)
            {
                tmpValue[i] = data[m_keys[i]].ToString();
                // 顺便把m_keys全部首字母小写
                m_keys[i] = StringEx.ToLowerFirstChar(m_keys[i]);
            }

            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            // 根据临时值值判断类型去赋值type函数
            for (int i = 0; i < tmpValue.Length; i++)
            {
                var v = tmpValue[i]; // 避免闭包
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


            for (int i = 0; i < m_keys.Length; i++)
            {
                print($"public {m_types[i]} {m_keys[i]} = {tmpValue[i]}");
            }
        }
    }
}