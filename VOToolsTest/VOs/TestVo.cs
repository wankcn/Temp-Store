using GameNeon.Managers;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameNeon
{
    public class TestVo : MonoBehaviour
    {
        public static string TEXT_PATH = "Test/characterConfigTest";


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
            vou.ExportFile();
        }
    }
}