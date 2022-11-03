using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ES3Internal;
using System;

public class ESSaveManager
{
    #region Save


    // Object
    public static void Save(string key, object value)
    {
        ES3.Save<object>(key, value, new ES3Settings());
    }

    public static void Save(string key, object value, string filePath)
    {
        ES3.Save<object>(key, value, new ES3Settings(filePath));
    }

    public static void Save(string key, object value, string filePath, ES3Settings settings)
    {
        ES3.Save<object>(key, value, new ES3Settings(filePath, settings));
    }

    public static void Save(string key, object value, ES3Settings settings)
    {
        ES3.Save<object>(key, value, settings);
    }


    public static void Save<T>(string key, T value)
    {
        ES3.Save<T>(key, value, new ES3Settings());
    }

    public static void Save<T>(string key, T value, string filePath)
    {
        ES3.Save<T>(key, value, new ES3Settings(filePath));
    }

    public static void Save<T>(string key, T value, string filePath, ES3Settings settings)
    {
        ES3.Save<T>(key, value, new ES3Settings(filePath, settings));
    }

    public static void Save<T>(string key, T value, ES3Settings settings)
    {
        ES3.Save<T>(key, value, settings);
    }

    // 二位数组
    public static void SaveBytes(byte[] bytes)
    {
        ES3.SaveRaw(bytes, new ES3Settings());
    }

    public static void SaveBytes(byte[] bytes, string filePath)
    {
        ES3.SaveRaw(bytes, new ES3Settings(filePath));
    }

    public static void SaveBytes(byte[] bytes, string filePath, ES3Settings settings)
    {
        ES3.SaveRaw(bytes, new ES3Settings(filePath, settings));
    }

    public static void SaveBytes(byte[] bytes, ES3Settings settings)
    {
        ES3.SaveRaw(bytes, settings);
    }

    // 保存原始二进制数据 SaveRaw
    // var bytes = settings.encoding.GetBytes(str);
    public static void SaveBytes(string str)
    {
        var settings = new ES3Settings();
        var bytes = settings.encoding.GetBytes(str);
        ES3.SaveRaw(bytes, settings);
    }

    public static void SaveBytes(string str, string filePath)
    {
        var settings = new ES3Settings(filePath);
        var bytes = settings.encoding.GetBytes(str);
        ES3.SaveRaw(bytes, settings);
    }

    public static void SaveBytes(string str, string filePath, ES3Settings settings)
    {
        var setting = new ES3Settings(filePath, settings);
        var bytes = setting.encoding.GetBytes(str);
        ES3.SaveRaw(bytes, setting);
    }

    public static void SaveBytes(string str, ES3Settings settings)
    {
        var bytes = settings.encoding.GetBytes(str);
        ES3.SaveRaw(bytes, settings);
    }

    // 追加数据
    public static void AppendBytes(byte[] bytes)
    {
        ES3.AppendRaw(bytes, new ES3Settings());
    }

    public static void AppendBytes(byte[] bytes, string filePath, ES3Settings settings)
    {
        ES3.AppendRaw(bytes, new ES3Settings(filePath, settings));
    }

    public static void AppendBytes(byte[] bytes, ES3Settings settings)
    {
        ES3.AppendRaw(bytes, settings);
    }

    public static void AppendBytes(string str)
    {
        ES3.AppendRaw(str, new ES3Settings());
    }

    public static void AppendBytes(string str, string filePath, ES3Settings settings)
    {
        ES3.AppendRaw(str, new ES3Settings(filePath, settings));
    }

    public static void AppendBytes(string str, ES3Settings settings)
    {
        ES3.AppendRaw(str, settings);
    }

    // Image
    public static void SaveImage(Texture2D texture, string imagePath)
    {
        ES3.SaveImage(texture, 75, new ES3Settings(imagePath));
    }

    public static void SaveImage(Texture2D texture, string imagePath, ES3Settings settings)
    {
        ES3.SaveImage(texture, 75, new ES3Settings(imagePath, settings));
    }

    public static void SaveImage(Texture2D texture, ES3Settings settings)
    {
        ES3.SaveImage(texture, 75, settings);
    }

    public static void SaveImage(Texture2D texture, int quality, string imagePath)
    {
        ES3.SaveImage(texture, quality, new ES3Settings(imagePath));
    }

    public static void SaveImage(Texture2D texture, int quality, string imagePath, ES3Settings settings)
    {
        ES3.SaveImage(texture, quality, new ES3Settings(imagePath, settings));
    }

    public static void SaveImage(Texture2D texture, int quality, ES3Settings settings)
    {
        ES3.SaveImage(texture, quality, settings);
    }

    #endregion

     #region Load

        public static object Load(string key)
        {
            return ES3.Load<object>(key, new ES3Settings());
        }

        public static object Load(string key, string filePath)
        {
            return ES3.Load<object>(key, new ES3Settings(filePath));
        }

        public static object Load(string key, string filePath, ES3Settings settings)
        {
            return ES3.Load<object>(key, new ES3Settings(filePath, settings));
        }

        public static object Load(string key, ES3Settings settings)
        {
            return ES3.Load<object>(key, settings);
        }


        public static object Load(string key, object defaultValue)
        {
            return ES3.Load<object>(key, defaultValue, new ES3Settings());
        }

        public static object Load(string key, string filePath, object defaultValue)
        {
            return ES3.Load<object>(key, defaultValue, new ES3Settings(filePath));
        }

        public static object Load(string key, string filePath, object defaultValue, ES3Settings settings)
        {
            return ES3.Load<object>(key, defaultValue, new ES3Settings(filePath, settings));
        }

        public static object Load(string key, object defaultValue, ES3Settings settings)
        {
            return ES3.Load<object>(key, defaultValue, settings);
        }


        public static T Load<T>(string key)
        {
            return ES3.Load<T>(key, new ES3Settings());
        }

        public static T Load<T>(string key, string filePath)
        {
            return ES3.Load<T>(key, new ES3Settings(filePath));
        }

        public static T Load<T>(string key, string filePath, ES3Settings settings)
        {
            return ES3.Load<T>(key, new ES3Settings(filePath, settings));
        }

        public static T Load<T>(string key, ES3Settings settings)
        {
            return ES3.Load<T>(key, settings);
        }

        public static T Load<T>(string key, T defaultValue)
        {
            return ES3.Load<T>(key, defaultValue, new ES3Settings());
        }

        public static T Load<T>(string key, string filePath, T defaultValue)
        {
            return ES3.Load<T>(key, defaultValue, new ES3Settings(filePath));
        }

        public static T Load<T>(string key, string filePath, T defaultValue, ES3Settings settings)
        {
            return ES3.Load<T>(key, defaultValue, new ES3Settings(filePath, settings));
        }

        public static T Load<T>(string key, T defaultValue, ES3Settings settings)
        {
            return ES3.Load<T>(key, defaultValue, settings);
        }

        #endregion
}