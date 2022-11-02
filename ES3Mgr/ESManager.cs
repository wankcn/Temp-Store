// Author: 文若
// CreateDate: 2022/11/02

using System;
using UnityEngine;
using ES3Internal;

namespace GameNeon.Managers
{
    public class ESManager
    {
        #region Save数据保存

        // 数据保存
        public static void Save(string key, object value, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            ES3.Save<object>(key, value, setting);
        }

        public static void Save<T>(string key, T value, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            ES3.Save<T>(key, value, setting);
        }

        // 字节数组
        public static void SaveBytes(byte[] bytes, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            ES3.SaveRaw(bytes, setting);
        }

        public static void SaveBytes(string str, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            var bytes = setting.encoding.GetBytes(str);
            ES3.SaveRaw(bytes, setting);
        }


        // 追加数据
        public static void AppendBytes(byte[] bytes, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            ES3.AppendRaw(bytes, setting);
        }

        public static void AppendBytes(string str, string filePath = null, ES3Settings settings = null)
        {
            var setting = new ES3Settings(filePath, settings);
            ES3.AppendRaw(str, setting);
        }

        // 保存Image
        public static void SaveImage(Texture2D texture, int quality = 75, string imagePath = null,
            ES3Settings settings = null)
        {
            var setting = new ES3Settings(imagePath, settings);
            ES3.SaveImage(texture, quality, setting);
        }

        #endregion
    }
}