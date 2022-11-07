using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

namespace GameNeon
{
    public class VOUtil
    {
        private JsonData data;
        private string className;

        // VO根目录路径
        private const string VO_ROOT = "Assets/Neon/Scripts/VOs/";

        
        public VOUtil(JsonData data, string path)
        {
            this.data = data;
            this.className = StringHelper.ToUpperFirstChar(path);;
        }


        public void ExportFile()
        {
            string filePath = VO_ROOT + className + "VO.cs";
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            
            // 写入命名空间
            WriteNamespace(sw);
            WriteVOSign(sw);
            
            sw.WriteLine("{");
            sw.WriteLine();
            sw.WriteLine("}");
            sw.WriteLine();
            sw.Flush();
            sw.Close();
        }
        
        private void WriteNamespace(StreamWriter sw)
        {
            sw.WriteLine("// 该类为自动生成的VO类，根据需求增加变量或方法");
            sw.WriteLine();
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

      
    }
}